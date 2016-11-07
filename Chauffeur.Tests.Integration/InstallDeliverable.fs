module ``Install Deliverable``

open System.Data.SqlServerCe
open Chauffeur
open Chauffeur.Tests.Integration
open Xunit
open FsUnit.Xunit
open TestHelpers

let connStrings = System.Configuration.ConfigurationManager.ConnectionStrings

[<CollectionAttribute("Basic host")>]
type ``Successfully setup the database``(fixture : BasicHostCollectionFixture) =
    [<Fact>]
    member x.``Results in a Continue response``() =
        fixture.TextReader.AddCommand "Y"
        async { 
            do! fixture.TextWriter.WriteLineAsync fixture.DatabaseLocation |> Async.AwaitTask
            let! response = fixture.Host.Run([| "install" |]) |> Async.AwaitTask
            response |> should equal DeliverableResponse.Continue
        }
        |> Async.RunSynchronously
    
    [<Fact>]
    member x.``Creates known tables``() =
        fixture.TextReader.AddCommand "Y"
        async { 
            do! fixture.TextWriter.WriteLineAsync fixture.DatabaseLocation |> Async.AwaitTask
            let! response = fixture.Host.Run([| "install" |]) |> Async.AwaitTask
            use connection = new SqlCeConnection(connStrings.["umbracoDbDSN"].ConnectionString)
            let cmd = 
                new SqlCeCommand("select table_name from information_schema.tables where TABLE_TYPE <> 'VIEW'",
                                 connection)
            connection.Open()
            let reader = cmd.ExecuteReader()
            while reader.Read() do
                let tableName = reader.GetString 0
                List.contains tableName knownTables |> should equal true
        }
        |> Async.RunSynchronously

[<CollectionAttribute("Basic host")>]
type ``Unsuccessfully setup the database``(fixture : BasicHostCollectionFixture) =
    [<Fact>]
    member x.``Won't create the database when you say not to``() = 
        fixture.TextReader.AddCommand "N"
        async { 
            do! fixture.TextWriter.WriteLineAsync fixture.DatabaseLocation |> Async.AwaitTask
            let! response = fixture.Host.Run([| "install" |]) |> Async.AwaitTask
            use connection = new SqlCeConnection(connStrings.["umbracoDbDSN"].ConnectionString)
            connection.Open |> should throw typeof<SqlCeException>
        }
        |> Async.RunSynchronously
