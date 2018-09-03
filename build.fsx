#load "./.fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.DotNet.NuGet
open Fake.DotNet.NuGet.NuGet
open Fake.DotNet.Testing
open Fake.IO.Globbing.Operators
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.Tools

Environment.setEnvironVar "VisualStudioVersion" "15.0"

let authors = ["Aaron Powell"]

let chauffeurDir = "./Chauffeur/bin/"
let chauffeurRunnerDir = "./Chauffeur.Runner/bin/"
let chauffeurTestingToolsDir = "./Chauffeur.TestingTools/bin/"
let packagingRoot = "./.packaging/"
let packagingDir = packagingRoot @@ "chauffeur"
let packagingRunnerDir = packagingRoot @@ "chauffeur.runner"
let packagingTestingToolsDir = packagingRoot @@ "chauffeur.testingtools"
let testDir = "./.testresults"
let buildMode = Environment.environVarOrDefault "buildMode" "Release"
let isAppVeyorBuild = not (isNull (Environment.environVar "APPVEYOR"))
let projectName = "Chauffeur"
let chauffeurSummary = "Chauffeur is a tool for helping with delivering changes to an Umbraco instance."
let chauffeurDescription = chauffeurSummary

let chauffeurRunnerSummary = "Chauffeur Runner is a CLI for executing Chauffeur deliverables against an Umbraco instance."
let chauffeurRunnerDescription = chauffeurRunnerSummary

let chauffeurTestingToolsSummary = "Chauffeur Testing Tools is a series of helpers for using Chauffeur to setup Umbraco for integration testing with Umbraco's API"
let chauffeurTestingToolsDescription = chauffeurRunnerSummary

let releaseNotes =
    File.read "ReleaseNotes.md"
        |> ReleaseNotes.parse

let trimBranchName (branch: string) =
    let trimmed = match branch.Length > 10 with
                    | true -> branch.Substring(0, 10)
                    | _ -> branch

    trimmed.Replace(".", "")

let prv = match Environment.environVar "APPVEYOR_REPO_BRANCH" with
            | null -> ""
            | "master" -> ""
            | branch -> sprintf "-%s%s" (trimBranchName branch) (
                            match Environment.environVar "APPVEYOR_BUILD_NUMBER" with
                            | null -> ""
                            | _ -> sprintf "-%s" (Environment.environVar "APPVEYOR_BUILD_NUMBER")
                            )
let nugetVersion = sprintf "%d.%d.%d%s" releaseNotes.SemVer.Major releaseNotes.SemVer.Minor releaseNotes.SemVer.Patch prv

Target.create "Default" ignore

Target.create "AssemblyInfo" (fun _ ->
    let commitHash = Git.Information.getCurrentHash()

    let attributes =
        [ Fake.DotNet.AssemblyInfo.Version releaseNotes.AssemblyVersion
          Fake.DotNet.AssemblyInfo.FileVersion releaseNotes.AssemblyVersion
          Fake.DotNet.AssemblyInfo.InformationalVersion releaseNotes.AssemblyVersion
          Fake.DotNet.AssemblyInfo.ComVisible false
          Fake.DotNet.AssemblyInfo.Metadata("githash", commitHash) ]

    AssemblyInfoFile.createCSharp "SolutionInfo.cs" attributes

    let fsAttributes =
        [ Fake.DotNet.AssemblyInfo.Product projectName
          Fake.DotNet.AssemblyInfo.Title "Chauffeur.TestingTools"
          Fake.DotNet.AssemblyInfo.Version releaseNotes.AssemblyVersion
          Fake.DotNet.AssemblyInfo.FileVersion releaseNotes.AssemblyVersion
          Fake.DotNet.AssemblyInfo.InformationalVersion releaseNotes.AssemblyVersion
          Fake.DotNet.AssemblyInfo.ComVisible false
          Fake.DotNet.AssemblyInfo.Metadata("githash", commitHash) ]

    AssemblyInfoFile.createFSharp "./Chauffeur.TestingTools/AssemblyInfo.fs" fsAttributes
)

Target.create "Clean" (fun _ ->
    Shell.cleanDirs [chauffeurDir; chauffeurRunnerDir; testDir]
)

Target.create "RestoreChauffeurPackages" (fun _ ->
    Restore.RestorePackage id "./Chauffeur/packages.config"
)

Target.create "RestoreChauffeurTestingToolsPackages" (fun _ ->
    Restore.RestorePackage id "./Chauffeur.TestingTools/packages.config"
)

Target.create "RestoreChauffeurDemoPackages" (fun _ ->
    Restore.RestorePackage id "./Chauffeur.Demo/packages.config"
)

Target.create "RestoreChauffeurTestsPackages" (fun _ ->
    Restore.RestorePackage id "./Chauffeur.Tests/packages.config"
    Restore.RestorePackage id "./Chauffeur.Tests.Integration/packages.config"
)

Target.create "Build" (fun _ ->
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

    MSBuild.build setParams "./Chauffeur.sln"
)

Target.create "UnitTests" (fun _ ->
    !! (sprintf "./Chauffeur.Tests/bin/%s/**/Chauffeur.Tests.dll" buildMode)
    |> XUnit2.run (fun p -> { p with HtmlOutputPath = Some (testDir @@ "xunit.html") })
)

Target.create "EnsureSqlExpressAssemblies" (fun _ ->
    Shell.copyDir (sprintf "./Chauffeur.Tests.Integration/bin/%s" buildMode) "packages/UmbracoCms.7.7.0/UmbracoFiles/bin" (fun _ -> true)
)

Target.create "CleanXUnitVSRunner" (fun _ ->
    Fake.IO.File.delete (sprintf "./Chauffeur.Tests.Integration/bin/%s/xunit.runner.visualstudio.testadapter.dll" buildMode)
)

Target.create "IntegrationTests" (fun _ ->
    !! (sprintf "./Chauffeur.Tests.Integration/bin/%s/**/Chauffeur.Tests.Integration.dll" buildMode)
    |> XUnit2.run (fun p -> { p with HtmlOutputPath = Some (testDir @@ "xunit-integration.html") })
)

Target.create "CreateChauffeurPackage" (fun _ ->
    let libDir = packagingDir @@ "lib/net45/"
    Shell.cleanDirs [libDir]

    Shell.copyFile libDir (chauffeurDir @@ "Release/Chauffeur.dll")
    Shell.copyFiles packagingDir ["License.md"; "readme.md"]

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
            AccessKey = Environment.environVarOrDefault "nugetkey" ""
            Dependencies =
                ["System.IO.Abstractions", "1.4.0.93"]
            Publish = Environment.hasEnvironVar "nugetkey" }) "Chauffeur/Chauffeur.nuspec"
)

Target.create "CreateRunnerPackage" (fun _ ->
    let libDir = packagingRunnerDir @@ "lib/net45/"
    Shell.cleanDirs [libDir]

    Shell.copyFile libDir (chauffeurRunnerDir @@ "Release/Chauffeur.Runner.exe")
    Shell.copyFiles packagingDir ["License.md"; "readme.md"]


    NuGet (fun p ->
        {p with
            Authors = authors
            Project = (sprintf "%s.Runner" projectName)
            Description = chauffeurRunnerDescription
            OutputPath = packagingRoot
            Summary = chauffeurRunnerSummary
            WorkingDir = packagingRunnerDir
            Version = nugetVersion
            ReleaseNotes = String.toLines releaseNotes.Notes
            SymbolPackage = NugetSymbolPackage.Nuspec
            Dependencies =
                ["Chauffeur", nugetVersion]
            AccessKey = Environment.environVarOrDefault "nugetkey" ""
            Publish = Environment.hasEnvironVar "nugetkey" }) "Chauffeur.Runner/Chauffeur.Runner.nuspec"
)

Target.create "CreateTestingToolsPackage" (fun _ ->
    let libDir = packagingTestingToolsDir @@ "lib/net45/"
    Shell.cleanDirs [libDir]

    Shell.copyFile libDir (chauffeurTestingToolsDir @@ "Release/Chauffeur.TestingTools.dll")
    Shell.copyFiles packagingDir ["License.md"; "readme.md"]

    NuGet (fun p ->
        {p with
            Authors = authors
            Project = (sprintf "%s.TestingTools" projectName)
            Description = chauffeurTestingToolsDescription
            OutputPath = packagingRoot
            Summary = chauffeurTestingToolsSummary
            WorkingDir = packagingTestingToolsDir
            Version = nugetVersion
            ReleaseNotes = String.toLines releaseNotes.Notes
            SymbolPackage = NugetSymbolPackage.Nuspec
            Dependencies =
                [ "Chauffeur", nugetVersion
                  "FSharp.Core", "4.3.4" ]
            AccessKey = Environment.environVarOrDefault "nugetkey" ""
            Publish = Environment.hasEnvironVar "nugetkey" }) "Chauffeur.TestingTools/Chauffeur.TestingTools.nuspec"
)

Target.create "BuildVersion" (fun _ ->
    Shell.Exec("appveyor", sprintf "UpdateBuild -Version \"%s\"" nugetVersion) |> ignore
)

Target.create "Package" ignore

"AssemblyInfo"
    ==> "Build"

"Clean"
    =?> ("BuildVersion", isAppVeyorBuild)
    ==> "Build"

"RestoreChauffeurPackages"
    ==> "RestoreChauffeurTestingToolsPackages"
    ==> "RestoreChauffeurDemoPackages"
    ==> "RestoreChauffeurTestsPackages"
    ==> "Build"

"Build"
    ==> "Default"

"EnsureSqlExpressAssemblies"
    ==> "CleanXUnitVSRunner"
    ==> "IntegrationTests"

"CreateChauffeurPackage"
    ==> "CreateRunnerPackage"
    ==> "CreateTestingToolsPackage"
    ==> "Package"

Target.runOrDefault "Default"