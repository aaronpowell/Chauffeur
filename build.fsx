#r @"tools/FAKE.Core/tools/FakeLib.dll"
#r @"tools/FSharpLint.Fake/tools/FSharpLint.Fake.dll"

open Fake
open Fake.Testing.XUnit2
open Fake.AssemblyInfoFile
open FSharpLint.Fake

let authors = ["Aaron Powell"]

let chauffeurDir = "./Chauffeur/bin/"
let chauffeurRunnerDir = "./Chauffeur.Runner/bin/"
let packagingRoot = "./.packaging/"
let packagingDir = packagingRoot @@ "chauffeur"
let packagingRunnerDir = packagingRoot @@ "chauffeur.runner"
let testDir = "./.testresults"
let buildMode = getBuildParamOrDefault "buildMode" "Release"
let isAppVeyorBuild = not (isNull (environVar "APPVEYOR"))
let projectName = "Chauffeur"
let chauffeurSummary = "Chauffeur is a tool for helping with delivering changes to an Umbraco instance."
let chauffeurDescription = chauffeurSummary

let chauffeurRunnerSummary = "Chauffeur Runner is a CLI for executing Chauffeur deliverables against an Umbraco instance."
let chauffeurRunnerDescription = chauffeurRunnerSummary

let releaseNotes =
    ReadFile "ReleaseNotes.md"
        |> ReleaseNotesHelper.parseReleaseNotes

let trimBranchName (branch: string) =
    let trimmed = match branch.Length > 10 with
                    | true -> branch.Substring(0, 10)
                    | _ -> branch

    trimmed.Replace(".", "")

let prv = match environVar "APPVEYOR_REPO_BRANCH" with
            | "master" -> ""
            | branch -> sprintf "-%s%s" (trimBranchName branch) (
                            match environVar "APPVEYOR_BUILD_NUMBER" with
                            | null -> ""
                            | _ -> sprintf "-%s" (environVar "APPVEYOR_BUILD_NUMBER")
                            )
let nugetVersion = sprintf "%d.%d.%d%s" releaseNotes.SemVer.Major releaseNotes.SemVer.Minor releaseNotes.SemVer.Patch prv

Target "Default" DoNothing

Target "AssemblyInfo" (fun _ ->
    CreateCSharpAssemblyInfo "SolutionInfo.cs"
      [ Attribute.Product projectName
        Attribute.Version releaseNotes.AssemblyVersion
        Attribute.FileVersion releaseNotes.AssemblyVersion
        Attribute.ComVisible false ]
)

Target "Clean" (fun _ ->
    CleanDirs [chauffeurDir; chauffeurRunnerDir; testDir]
)

Target "RestoreChauffeurPackages" (fun _ ->
    RestorePackage id "./Chauffeur/packages.config"
)

Target "RestoreChauffeurDemoPackages" (fun _ ->
    RestorePackage id "./Chauffeur.Demo/packages.config"
)

Target "RestoreChauffeurTestsPackages" (fun _ ->
    RestorePackage id "./Chauffeur.Tests/packages.config"
    RestorePackage id "./Chauffeur.Tests.Integration/packages.config"
)

Target "Build" (fun _ ->
    MSBuild null "Build" ["Configuration", buildMode] ["Chauffeur.sln"]
    |> Log "AppBuild-Output: "
)

Target "UnitTests" (fun _ ->
    !! (sprintf "./Chauffeur.Tests/bin/%s/**/Chauffeur.Tests.dll" buildMode)
    |> xUnit2 (fun p -> { p with HtmlOutputPath = Some (testDir @@ "xunit.html") })
)

Target "EnsureSqlExpressAssemblies" (fun _ ->
    CopyDir (sprintf "./Chauffeur.Tests.Integration/bin/%s" buildMode) "packages/UmbracoCms.7.6.1/UmbracoFiles/bin" (fun x -> true)
)

Target "CleanXUnitVSRunner" (fun _ ->
    DeleteFile (sprintf "./Chauffeur.Tests.Integration/bin/%s/xunit.runner.visualstudio.testadapter.dll" buildMode)
)

Target "IntegrationTests" (fun _ ->
    !! (sprintf "./Chauffeur.Tests.Integration/bin/%s/**/Chauffeur.Tests.Integration.dll" buildMode)
    |> xUnit2 (fun p -> { p with HtmlOutputPath = Some (testDir @@ "xunit-integration.html") })
)

Target "CreateChauffeurPackage" (fun _ ->
    let libDir = packagingDir @@ "lib/net45/"
    CleanDirs [libDir]

    CopyFile libDir (chauffeurDir @@ "Release/Chauffeur.dll")
    CopyFiles packagingDir ["LICENSE.md"; "README.md"]


    NuGet (fun p ->
        {p with
            Authors = authors
            Project = projectName
            Description = chauffeurDescription
            OutputPath = packagingRoot
            Summary = chauffeurSummary
            WorkingDir = packagingDir
            Version = nugetVersion
            ReleaseNotes = toLines releaseNotes.Notes
            SymbolPackage = NugetSymbolPackage.Nuspec
            AccessKey = getBuildParamOrDefault "nugetkey" ""
            Dependencies =
                ["System.IO.Abstractions", "1.4.0.93"]
            Publish = hasBuildParam "nugetkey" }) "Chauffeur/Chauffeur.nuspec"
)

Target "CreateRunnerPackage" (fun _ ->
    let libDir = packagingRunnerDir @@ "lib/net45/"
    CleanDirs [libDir]

    CopyFile libDir (chauffeurRunnerDir @@ "Release/Chauffeur.Runner.exe")
    CopyFiles packagingDir ["LICENSE.md"; "README.md"]


    NuGet (fun p ->
        {p with
            Authors = authors
            Project = projectName
            Description = chauffeurRunnerDescription
            OutputPath = packagingRoot
            Summary = chauffeurRunnerSummary
            WorkingDir = packagingRunnerDir
            Version = nugetVersion
            ReleaseNotes = toLines releaseNotes.Notes
            SymbolPackage = NugetSymbolPackage.Nuspec
            Dependencies =
                ["Chauffeur", NormalizeVersion nugetVersion]
            AccessKey = getBuildParamOrDefault "nugetkey" ""
            Publish = hasBuildParam "nugetkey" }) "Chauffeur.Runner/Chauffeur.Runner.nuspec"
)

Target "BuildVersion" (fun _ ->
    Shell.Exec("appveyor", sprintf "UpdateBuild -Version \"%s\"" nugetVersion) |> ignore
)

Target "Package" DoNothing

Target "Lint" (fun _ ->
    !! "src/**/*.fsproj"
        |> Seq.iter (FSharpLint id))

"Clean"
    =?> ("BuildVersion", isAppVeyorBuild)
    ==> "Lint"
    ==> "Build"

"RestoreChauffeurPackages"
    ==> "RestoreChauffeurDemoPackages"
    ==> "RestoreChauffeurTestsPackages"
    ==> "Build"

"UnitTests"
    ==> "Default"

"EnsureSqlExpressAssemblies"
    ==> "CleanXUnitVSRunner"
    ==> "IntegrationTests"

"CreateChauffeurPackage"
    ==> "CreateRunnerPackage"
    ==> "Package"

RunTargetOrDefault "Default"