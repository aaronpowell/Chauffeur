namespace Chauffeur.TestingTools

open System
open System.IO
open Chauffeur.Host
open Chauffeur.Tests.Integration

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

    let getChauffeurFolder databaseLocation =
            let chauffeurFolder = Path.Combine [| databaseLocation; "Chauffeur" |]
            Directory.CreateDirectory chauffeurFolder

[<AbstractClass>]
type UmbracoHostTestBase() =
    let dbFolder = ChauffeurSetup.setDataDirectory()

    let writer = new MockTextWriter()
    let reader = new MockTextReader()
    let host = new UmbracoHost(reader, writer)

    member x.DatabaseLocation = dbFolder
    member x.Host = host
    member x.TextReader = reader
    member x.TextWriter = writer

    interface IDisposable with
        member x.Dispose() =
            writer.Dispose()
            reader.Dispose()
            host.Dispose()
