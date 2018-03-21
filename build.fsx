#r @"tools/FAKE.Core/tools/FakeLib.dll"
#r @"tools/FSharpLint.Fake/tools/FSharpLint.Core.dll"
#r @"tools/FSharpLint.Fake/tools/FSharpLint.Fake.dll"

open Fake
open Fake.Core
open Fake.Core.Environment
open Fake.Core.Globbing.Operators
open Fake.Core.TargetOperators
open Fake.DotNet.AssemblyInfoFile
open Fake.DotNet.NuGet.Restore
open Fake.DotNet.NuGet.NuGet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.DotNet.Testing.XUnit2
open Fake.Tools
open FSharpLint.Fake

Environment.setEnvironVar "VisualStudioVersion" "15.0"

let authors = ["Aaron Powell"]

let chauffeurDir = "./Chauffeur/bin/"
let chauffeurRunnerDir = "./Chauffeur.Runner/bin/"
let packagingRoot = "./.packaging/"
let packagingDir = packagingRoot @@ "chauffeur"
let packagingRunnerDir = packagingRoot @@ "chauffeur.runner"
let testDir = "./.testresults"
let buildMode = environVarOrDefault "buildMode" "Release"
let isAppVeyorBuild = not (isNull (environVar "APPVEYOR"))
let projectName = "Chauffeur"
let chauffeurSummary = "Chauffeur is a tool for helping with delivering changes to an Umbraco instance."
let chauffeurDescription = chauffeurSummary

let chauffeurRunnerSummary = "Chauffeur Runner is a CLI for executing Chauffeur deliverables against an Umbraco instance."
let chauffeurRunnerDescription = chauffeurRunnerSummary

let releaseNotes =
    File.read "ReleaseNotes.md"
        |> Fake.ReleaseNotesHelper.parseReleaseNotes

let trimBranchName (branch: string) =
    let trimmed = match branch.Length > 10 with
                    | true -> branch.Substring(0, 10)
                    | _ -> branch

    trimmed.Replace(".", "")

let prv = match environVar "APPVEYOR_REPO_BRANCH" with
            | null -> ""
            | "master" -> ""
            | branch -> sprintf "-%s%s" (trimBranchName branch) (
                            match environVar "APPVEYOR_BUILD_NUMBER" with
                            | null -> ""
                            | _ -> sprintf "-%s" (environVar "APPVEYOR_BUILD_NUMBER")
                            )
let nugetVersion = sprintf "%d.%d.%d%s" releaseNotes.SemVer.Major releaseNotes.SemVer.Minor releaseNotes.SemVer.Patch prv

Target.Create "Default" Target.DoNothing

Target.Create "AssemblyInfo" (fun _ ->
    let commitHash = Git.Information.getCurrentHash()

    let attributes =
        [ Fake.DotNet.AssemblyInfo.Product projectName
          Fake.DotNet.AssemblyInfo.Title "Chauffeur"
          Fake.DotNet.AssemblyInfo.Version releaseNotes.AssemblyVersion
          Fake.DotNet.AssemblyInfo.FileVersion releaseNotes.AssemblyVersion
          Fake.DotNet.AssemblyInfo.InformationalVersion releaseNotes.AssemblyVersion
          Fake.DotNet.AssemblyInfo.ComVisible false
          Fake.DotNet.AssemblyInfo.Metadata("githash", commitHash) ]

    CreateCSharp "SolutionInfo.cs" attributes
)

Target.Create "Clean" (fun _ ->
    Shell.CleanDirs [chauffeurDir; chauffeurRunnerDir; testDir]
)

Target.Create "RestoreChauffeurPackages" (fun _ ->
    RestorePackage id "./Chauffeur/packages.config"
)

Target.Create "RestoreChauffeurDemoPackages" (fun _ ->
    RestorePackage id "./Chauffeur.Demo/packages.config"
)

Target.Create "RestoreChauffeurTestsPackages" (fun _ ->
    RestorePackage id "./Chauffeur.Tests/packages.config"
    RestorePackage id "./Chauffeur.Tests.Integration/packages.config"
)

Target.Create "Build" (fun _ ->
    let setParams (defaults: MSBuildParams) =
        let p = { defaults with
                    Verbosity = Some(Quiet)
                    Targets = ["Build"]
                    Properties =
                    [
                        "Configuration", buildMode
                        "Optimize", "True"
                        "DebugSymbols", "True"
                    ] }
        if isAppVeyorBuild then p
        else { p with ToolPath = "C:\Program Files (x86)\Microsoft Visual Studio\Preview\Enterprise\MSBuild\15.0\Bin\msbuild.exe" }

    build setParams "./Chauffeur.sln"
)

Target.Create "UnitTests" (fun _ ->
    !! (sprintf "./Chauffeur.Tests/bin/%s/**/Chauffeur.Tests.dll" buildMode)
    |> xUnit2 (fun p -> { p with HtmlOutputPath = Some (testDir @@ "xunit.html") })
)

Target.Create "EnsureSqlExpressAssemblies" (fun _ ->
    Shell.CopyDir (sprintf "./Chauffeur.Tests.Integration/bin/%s" buildMode) "packages/UmbracoCms.7.6.1/UmbracoFiles/bin" (fun x -> true)
)

Target.Create "CleanXUnitVSRunner" (fun _ ->
    Fake.IO.File.delete (sprintf "./Chauffeur.Tests.Integration/bin/%s/xunit.runner.visualstudio.testadapter.dll" buildMode)
)

Target.Create "IntegrationTests" (fun _ ->
    !! (sprintf "./Chauffeur.Tests.Integration/bin/%s/**/Chauffeur.Tests.Integration.dll" buildMode)
    |> xUnit2 (fun p -> { p with HtmlOutputPath = Some (testDir @@ "xunit-integration.html") })
)

Target.Create "CreateChauffeurPackage" (fun _ ->
    let libDir = packagingDir @@ "lib/net45/"
    Shell.CleanDirs [libDir]

    Shell.CopyFile libDir (chauffeurDir @@ "Release/Chauffeur.dll")
    Shell.CopyFiles packagingDir ["LICENSE.md"; "README.md"]


    NuGet (fun p ->
        {p with
            Authors = authors
            Project = projectName
            Description = chauffeurDescription
            OutputPath = packagingRoot
            Summary = chauffeurSummary
            WorkingDir = packagingDir
            Version = nugetVersion
            ReleaseNotes = String.toLines releaseNotes.Notes
            SymbolPackage = NugetSymbolPackage.Nuspec
            AccessKey = environVarOrDefault "nugetkey" ""
            Dependencies =
                ["System.IO.Abstractions", "1.4.0.93"]
            Publish = hasEnvironVar "nugetkey" }) "Chauffeur/Chauffeur.nuspec"
)

Target.Create "CreateRunnerPackage" (fun _ ->
    let libDir = packagingRunnerDir @@ "lib/net45/"
    Shell.CleanDirs [libDir]

    Shell.CopyFile libDir (chauffeurRunnerDir @@ "Release/Chauffeur.Runner.exe")
    Shell.CopyFiles packagingDir ["LICENSE.md"; "README.md"]


    NuGet (fun p ->
        {p with
            Authors = authors
            Project = projectName
            Description = chauffeurRunnerDescription
            OutputPath = packagingRoot
            Summary = chauffeurRunnerSummary
            WorkingDir = packagingRunnerDir
            Version = nugetVersion
            ReleaseNotes = String.toLines releaseNotes.Notes
            SymbolPackage = NugetSymbolPackage.Nuspec
            Dependencies =
                ["Chauffeur", nugetVersion]
            AccessKey = environVarOrDefault "nugetkey" ""
            Publish = hasEnvironVar "nugetkey" }) "Chauffeur.Runner/Chauffeur.Runner.nuspec"
)

Target.Create "BuildVersion" (fun _ ->
    Process.Shell.Exec("appveyor", sprintf "UpdateBuild -Version \"%s\"" nugetVersion) |> ignore
)

Target.Create "Package" Target.DoNothing

Target.Create "Lint" (fun _ ->
    !! "src/**/*.fsproj"
        |> Seq.iter (FSharpLint id))

"Clean"
    =?> ("BuildVersion", isAppVeyorBuild)
    //==> "Lint"
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

Target.RunOrDefault "Default"