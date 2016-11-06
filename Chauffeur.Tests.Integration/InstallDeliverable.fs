module ``Install Deliverable``

open System.Data.SqlServerCe
open Chauffeur
open Chauffeur.Tests.Integration
open Xunit
open FsUnit.Xunit
open TestHelpers

[<CollectionAttribute("Basic host")>]
type ``Install Deliverable Tests``(fixture : BasicHostCollectionFixture) = 
    
    [<Fact>]
    member x.``Installation should result in a continuation when successful``() = 
        fixture.TextReader.AddCommand "Y"
        async { 
            do! fixture.TextWriter.WriteLineAsync dbFolder |> Async.AwaitTask
            let! response = fixture.Host.Run([| "install" |]) |> Async.AwaitTask
            response |> should equal DeliverableResponse.Continue
        }
        |> Async.RunSynchronously
    
    [<Fact>]
    member x.``Installation should create a bunch of umbraco tables``() = 
        fixture.TextReader.AddCommand "Y"
        async { 
            do! fixture.TextWriter.WriteLineAsync dbFolder |> Async.AwaitTask
            let! response = fixture.Host.Run([| "install" |]) |> Async.AwaitTask
            let connStrings = System.Configuration.ConfigurationManager.ConnectionStrings
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
