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

let authors = ["Aaron Powell"]

let chauffeurDir = "./src/Chauffeur/bin/"
let chauffeurRunnerDir = "./src/Chauffeur.Runner/bin/"
let chauffeurTestingToolsDir = "./src/Chauffeur.TestingTools/bin/"
let packagingDir = "../../.packaging/"
let testDir = "./.testresults"
let buildMode = match Environment.environVarOrDefault "buildMode" "Debug" with
                | "Release" -> DotNet.BuildConfiguration.Release
                | _ -> DotNet.BuildConfiguration.Debug

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

let pack project =
    DotNet.pack (fun p ->
        {p with
            Configuration = buildMode
            OutputPath = Some packagingDir
            NoBuild = true
            MSBuildParams = { p.MSBuildParams
                              with Properties =
                                   [("Authors", authors |> String.concat(","))
                                    ("PackageVersion", nugetVersion)
                                    ("PackageReleaseNotes", releaseNotes.Notes |> String.concat("\n"))] }
        } |> dotnetSimple) project

Target.create "Package Chauffeur" (fun _ ->
    pack (chauffeurDir + "../Chauffeur.fsproj")
)

Target.create "Package Chauffeur Runner" (fun _ ->
    pack (chauffeurRunnerDir + "../Chauffeur.Runner.fsproj")
)

Target.create "Package Chauffeur Testing Tools" (fun _ ->
    pack (chauffeurTestingToolsDir + "../Chauffeur.TestingTools.fsproj")
)

Target.create "Unit Tests" (fun _ ->
    CreateProcess.fromRawCommand
        "./.fake/coverlet.exe"
        [".\\src\\Chauffeur.Tests\\bin\\Debug\\net472\\Chauffeur.Tests.dll"
         "--target"
         "dotnet"
         "--targetargs"
         "test ./src/Chauffeur.Tests/ --no-build"
         "--exclude"
         "[xunit.*]*"
         "--output"
         testDir @@ "unit-tests.xml"
         "--format"
         "cobertura"]
    |> Proc.run
    |> ignore

    CreateProcess.fromRawCommand
        "./.fake/coverlet.exe"
        [sprintf ".\\src\\Chauffeur.Deliverables.Tests\\bin\\%A\\net472\\Chauffeur.Deliverables.Tests.dll" buildMode
         "--target"
         "dotnet"
         "--targetargs"
         "test ./src/Chauffeur.Deliverables.Tests/ --no-build"
         "--exclude"
         "[xunit.*]*"
         "--output"
         testDir @@ "legacy-unit-tests.xml"
         "--format"
         "cobertura"]
    |> Proc.run
    |> ignore
)

Target.create "Default" ignore
Target.create "Package" ignore
Target.create "Test" ignore

"Clean"
    ==> "DotNetRestore"
    ==> "Build"
    ==> "Default"

"Package Chauffeur"
    ==> "Package"

"Package Chauffeur Runner"
    ==> "Package"

"Package Chauffeur Testing Tools"
    ==> "Package"

"Unit Tests"
    ==> "Test"

Target.runOrDefault "Default"