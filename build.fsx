#r "paket: groupref build //"
#load "./.fake/build.fsx/intellisense.fsx"

#if !FAKE
#r "netstandard"
#r "Facades/netstandard" // https://github.com/ionide/ionide-vscode-fsharp/issues/839#issuecomment-396296095
#endif

open System
open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators

Environment.setEnvironVar "VisualStudioVersion" "15.0"

let authors = ["Aaron Powell"]

let chauffeurDir = "./src/Chauffeur/bin/"
let chauffeurDeliverablesDir = "./src/Chauffeur.Deliverables/bin/"
let chauffeurRunnerDir = "./src/Chauffeur.Runner/bin/"
let chauffeurTestingToolsDir = "./src/Chauffeur.TestingTools/bin/"
let packagingRoot = "../../.packaging/"
let packagingDir = packagingRoot @@ "chauffeur"
let packagingRunnerDir = packagingRoot @@ "chauffeur.runner"
let packagingTestingToolsDir = packagingRoot @@ "chauffeur.testingtools"
let testDir = "./.testresults"
let buildMode = match Environment.environVarOrDefault "buildMode" "Debug" with
                | "Release" -> DotNet.BuildConfiguration.Release
                | _ -> DotNet.BuildConfiguration.Debug
let isCIBuild = not (isNull (Environment.environVar "AGENT_ID"))
let projectName = "Chauffeur"
let chauffeurSummary = "Chauffeur is a tool for helping with delivering changes to an Umbraco instance."
let chauffeurDescription = chauffeurSummary

let chauffeurRunnerSummary = "Chauffeur Runner is a CLI for executing Chauffeur deliverables against an Umbraco instance."
let chauffeurRunnerDescription = chauffeurRunnerSummary

let chauffeurTestingToolsSummary = "Chauffeur Testing Tools is a series of helpers for using Chauffeur to setup Umbraco for integration testing with Umbraco's API"
let chauffeurTestingToolsDescription = chauffeurRunnerSummary

let install = lazy DotNet.install DotNet.Versions.FromGlobalJson

let inline withWorkDir wd =
    DotNet.Options.lift install.Value
    >> DotNet.Options.withWorkingDirectory wd

let inline dotnetSimple arg = DotNet.Options.lift install.Value arg

let releaseNotes =
    File.read "ReleaseNotes.md"
        |> ReleaseNotes.parse

let trimBranchName (branch: string) =
    let trimmed = match branch.Length > 10 with
                  | true -> branch.Substring(0, 10)
                  | _ -> branch

    trimmed.Replace(".", "")

let prv = match Environment.environVar "BUILD_SOURCEBRANCHNAME" with
          | null -> ""
          | "master" -> ""
          | branch -> sprintf "-%s%s" (trimBranchName branch) (
                            match Environment.environVar "BUILD_BUILDNUMBER" with
                            | null -> ""
                            | _ -> sprintf "-%s" (Environment.environVar "BUILD_BUILDNUMBER")
                            )
let nugetVersion = sprintf "%d.%d.%d%s" releaseNotes.SemVer.Major releaseNotes.SemVer.Minor releaseNotes.SemVer.Patch prv

Target.create "Clean" (fun _ ->
    !! "src/**/bin"
    ++ "src/**/obj"
    -- "src/Chauffeur.Demo"
    |> Shell.cleanDirs
)

Target.create "Build" (fun _ ->
    DotNet.build (fun p -> { p with Configuration = buildMode } |> dotnetSimple) "src/Chauffeur.sln"
)

Target.create "DotNetRestore" (fun _ ->
    DotNet.restore (fun args -> args |> dotnetSimple) "src/Chauffeur.sln"
)

Target.create "Package Chauffeur" (fun _ ->
    DotNet.pack (fun p ->
        {p with
            Configuration = buildMode
            OutputPath = Some packagingDir
            NoBuild = true
            MSBuildParams = { p.MSBuildParams
                              with Properties =
                                   [("Author", authors |> String.concat(","))
                                    ("PackageVersion", nugetVersion)] }
        } |> dotnetSimple) (chauffeurDir + "../Chauffeur.fsproj")
)

Target.create "Default" ignore
Target.create "Package" ignore

"Clean"
    ==> "DotNetRestore"
    ==> "Build"
    ==> "Default"

"Package Chauffeur"
    ==> "Package"

Target.runOrDefault "Default"