namespace Chauffeur.TestingTools

open System
open System.IO
open Chauffeur.Host
open System.Reflection
open System.Threading

module ChauffeurSetup =
    let private cwd = FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName
    let private dbFolder = "databases"

    let internal setData (domain : AppDomain) key value =
        domain.SetData(key, value)

    let internal setDataDirectory() =
        let now = DateTimeOffset.Now
        let ticks = now.Ticks.ToString()

        let folderForRun = Path.Combine [|cwd; dbFolder; ticks|]

        Directory.CreateDirectory folderForRun |> ignore

        let setDomainData = setData AppDomain.CurrentDomain
        let setThreadData = Thread.GetDomain() |> setData

        let asm = Assembly.GetExecutingAssembly()
        let exePath = (new FileInfo(asm.Location)).Directory

        setDomainData "DataDirectory" folderForRun
        setDomainData ".appDomain" "From Domain"
        setDomainData ".appId" "From Domain"
        setDomainData ".appVPath" exePath.FullName
        setDomainData ".appPath" exePath.FullName

        setThreadData ".appDomain" "From Thread"
        setThreadData ".appId" "From Thread"
        setThreadData ".appVPath" exePath.FullName
        setThreadData ".appPath" exePath.FullName

        folderForRun

[<AbstractClass>]
type UmbracoHostTestBase() =
    let dbFolder = ChauffeurSetup.setDataDirectory()

    let writer = new MockTextWriter()
    let reader = new MockTextReader()
    let host = new UmbracoHost(reader, writer) :> IChauffeurHost

    /// <summary>
    /// The temp path that was generated for the Umbraco `App_Data` folder, and where the Umbraco database will
    /// if you use the SQL CE database provider
    /// </summary>
    member __.DatabaseLocation = dbFolder

    /// <summary>
    /// The Chauffeur host to run Chauffeur deliverables again
    /// </summary>
    member __.Host = host

    /// <summary>
    /// An output stream that you can read the messages Chauffeur writes to
    /// </summary>
    member __.TextReader = reader

    /// <summary>
    /// An input stream that Chauffeur will read from
    /// </summary>
    member __.TextWriter = writer

    /// <summary>
    /// Installs the Umbraco database using the Chauffeur `install` deliverable.
    /// If you are using SQL CE it'll also create the file for you.
    /// </summary>
    member x.InstallUmbraco() =
        [| "install y" |] |> x.Host.RunWithArgs

    /// <summary>
    /// Gets the path on disk that Chauffeur would look for packages/delivery/etc. files within.
    /// This is the path that Chauffeur will resolve from its settings API internally.
    /// </summary>
    member __.GetChauffeurFolder() =
        let chauffeurFolder = Path.Combine [| dbFolder; "Chauffeur" |]

        match (Directory.Exists chauffeurFolder) with
        | true -> DirectoryInfo chauffeurFolder
        | false -> Directory.CreateDirectory chauffeurFolder

    member x.CreatePackage packageName packageContents =
        let chauffeurFolder = x.GetChauffeurFolder()
        let packageFilename = sprintf "%s.xml" packageName
        let filePath =
            Path.Combine [| chauffeurFolder.FullName
                            packageFilename |]
        File.WriteAllText(filePath, packageContents)
        packageFilename

    member __.GetSiteRootFolder() =
        let asm = Assembly.GetAssembly(host.GetType())
        let dir = (new FileInfo(asm.Location)).Directory.FullName
        Path.Combine(dir, "..")

    interface IDisposable with
        member __.Dispose() =
            writer.Dispose()
            reader.Dispose()
            (host :?> IDisposable).Dispose()
