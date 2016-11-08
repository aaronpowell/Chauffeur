module ``Install Deliverable``

open System
open System.Data.SqlServerCe
open Chauffeur
open Chauffeur.Host
open Chauffeur.Tests.Integration
open Xunit
open FsUnit.Xunit
open TestHelpers

let connStrings = System.Configuration.ConfigurationManager.ConnectionStrings

type ``Successfully setup the database``() =
    inherit UmbracoHostTestBase()

    [<Fact>]
    member x.``Results in a Continue response``() =
        x.TextReader.AddCommand "Y"
        async {
            do! x.DatabaseLocation
                |> x.TextWriter.WriteLineAsync
                |> Async.AwaitTask
            let! response = [| "install" |]
                            |> x.Host.Run
                            |> Async.AwaitTask
            response |> should equal DeliverableResponse.Continue
        }
        |> Async.RunSynchronously

    [<Fact>]
    member x.``Creates known tables``() =
        x.TextReader.AddCommand "Y"
        async {
            do! x.DatabaseLocation
                |> x.TextWriter.WriteLineAsync
                |> Async.AwaitTask
            let! response = [| "install" |]
                            |> x.Host.Run
                            |> Async.AwaitTask
            use connection = new SqlCeConnection(connStrings.["umbracoDbDSN"].ConnectionString)
            let cmd =
                new SqlCeCommand("select table_name from information_schema.tables where TABLE_TYPE <> 'VIEW'",
                                 connection)
            connection.Open()

            let rec testTable (reader: SqlCeDataReader) =
                if reader.Read() then
                    let tableName = reader.GetString 0
                    List.contains tableName knownTables |> should equal true
                    testTable reader
                else
                    ignore

            cmd.ExecuteReader() |> testTable |> ignore
        }
        |> Async.RunSynchronously

type ``Unsuccessfully setup the database``() =
    inherit UmbracoHostTestBase()
    [<Fact>]
    member x.``Won't create the database when you say not to``() =
        x.TextReader.AddCommand "N"
        async {
            do! x.TextWriter.WriteLineAsync x.DatabaseLocation |> Async.AwaitTask
            let! response = x.Host.Run([| "install" |]) |> Async.AwaitTask
            use connection = new SqlCeConnection(connStrings.["umbracoDbDSN"].ConnectionString)
            connection.Open |> should throw typeof<SqlCeException>
        }
        |> Async.RunSynchronously
