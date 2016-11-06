namespace Chauffeur.Tests.Integration

open System
open Chauffeur.Host
open TestHelpers
open Xunit

type BasicHostCollectionFixture() = 
    let dbFolder = setDataDirectory()

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


[<CollectionDefinition("Basic host")>]
type BasicHostCollection() = 
    interface ICollectionFixture<BasicHostCollectionFixture>
