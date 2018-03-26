namespace Chauffeur.TestingTools

open System
open System.IO
open Chauffeur.Host

module ChauffeurSetup =
    open System.Reflection
    let private cwd = FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName
    let private dbFolder = "databases"

    let internal setDataDirectory() =
        let now = DateTimeOffset.Now
        let ticks = now.Ticks.ToString()

        let folderForRun = Path.Combine [|cwd; dbFolder; ticks|]

        Directory.CreateDirectory folderForRun |> ignore

        AppDomain.CurrentDomain.SetData("DataDirectory", folderForRun)

        folderForRun

[<AbstractClass>]
type UmbracoHostTestBase() =
    let dbFolder = ChauffeurSetup.setDataDirectory()

    let writer = new MockTextWriter()
    let reader = new MockTextReader()
    let host = new UmbracoHost(reader, writer)

    /// <summary>
    /// The temp path that was generated for the Umbraco `App_Data` folder, and where the Umbraco database will
    /// if you use the SQL CE database provider
    /// </summary>
    member x.DatabaseLocation = dbFolder

    /// <summary>
    /// The Chauffeur host to run Chauffeur deliverables again
    /// </summary>
    member x.Host = host

    /// <summary>
    /// An output stream that you can read the messages Chauffeur writes to
    /// </summary>
    member x.TextReader = reader

    /// <summary>
    /// An input stream that Chauffeur will read from
    /// </summary>
    member x.TextWriter = writer

    /// <summary>
    /// Installs the Umbraco database using the Chauffeur `install` deliverable.
    /// If you are using SQL CE it'll also create the file for you.
    /// </summary>
    member x.InstallUmbraco() =
        [| "install y" |] |> x.Host.Run

    /// <summary>
    /// Gets the path on disk that Chauffeur would look for packages/delivery/etc. files within.
    /// This is the path that Chauffeur will resolve from its settings API internally.
    /// </summary>
    member x.GetChauffeurFolder() =
        let chauffeurFolder = Path.Combine [| dbFolder; "Chauffeur" |]

        match (Directory.Exists chauffeurFolder) with
        | true -> DirectoryInfo chauffeurFolder
        | false -> Directory.CreateDirectory chauffeurFolder

    interface IDisposable with
        member x.Dispose() =
            writer.Dispose()
            reader.Dispose()
            host.Dispose()
