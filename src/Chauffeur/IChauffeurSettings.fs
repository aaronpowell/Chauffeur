namespace Chauffeur

open System.Configuration
open System.Diagnostics
open System.Reflection
open System.Runtime.InteropServices
open System.IO
open System.IO.Abstractions
open System

type IChauffeurSettings =
    abstract ConnectionString : ConnectionStringSettings
    abstract UmbracoVersion : string
    abstract ChauffeurVersion : string

    abstract TryGetChauffeurDirectory : [<Out>] exportDirectory : byref<string> -> bool
    abstract TryGetSiteRootDirectory : [<Out>] siteRootDirectory : byref<string> -> bool
    abstract TryGetUmbracoDirectory : [<Out>] umbracoDirectory : byref<string> -> bool


type internal ChauffeurSettings(writer : TextWriter, fileSystem : IFileSystem) =
    let tryGetSiteRootDirectory() =
            let rootFolder = AppDomain.CurrentDomain.GetData("DataDirectory") :?> string
            let siteRootDirectory = fileSystem.Path.Combine(rootFolder, "..")
            (fileSystem.Directory.Exists siteRootDirectory, siteRootDirectory)

    interface IChauffeurSettings with
        member __.ConnectionString = ConfigurationManager.ConnectionStrings.["umbracoDbDSN"]
        member __.UmbracoVersion = ConfigurationManager.AppSettings.["umbracoConfigurationStatus"]
        member __.ChauffeurVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion

        member __.TryGetChauffeurDirectory ([<Out>] exportDirectory) =
            exportDirectory <- ""

            let rootFolder = AppDomain.CurrentDomain.GetData("DataDirectory") :?> string
            exportDirectory <- fileSystem.Path.Combine(rootFolder, "Chauffeur")
            match fileSystem.Directory.Exists exportDirectory with
            | true ->
                true
            | false ->
                try
                    let _ = fileSystem.Directory.CreateDirectory exportDirectory
                    true
                with
                | :? UnauthorizedAccessException ->
                    writer.WriteLine("Chauffer directory 'App_Data\\Chauffeur' cannot be created, check directory permissions")
                    false

        member __.TryGetSiteRootDirectory ([<Out>] siteRootDirectory) =
            let pass, path = tryGetSiteRootDirectory()
            siteRootDirectory <- path
            pass

        member __.TryGetUmbracoDirectory ([<Out>] umbracoDirectory) =
            umbracoDirectory <- ""
            match tryGetSiteRootDirectory() with
            | true, root ->
                umbracoDirectory <- fileSystem.Path.Combine(root, "umbraco")
                fileSystem.Directory.Exists umbracoDirectory
            | false, _ -> false