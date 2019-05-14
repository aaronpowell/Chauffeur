module ``Install Deliverable``

open System.Data.SqlServerCe
open Chauffeur
open Xunit
open FsUnit.Xunit
open TestHelpers
open Chauffeur.TestingTools

let connStrings = System.Configuration.ConfigurationManager.ConnectionStrings

// this is a hack to ensure that that DLL is loaded in the app domain
let ctd = typeof<Chauffeur.Deliverables.ContentTypeDeliverable>
printfn "%s" ctd.FullName

type ``Successfully setup the database``() =
    inherit UmbracoHostTestBase()

    [<Fact>]
    member x.``Results in a Continue response``() =
        async {
            do! x.DatabaseLocation
                |> x.TextWriter.WriteLineAsync
                |> Async.AwaitTask
            let! response = [| "install y" |]
                            |> x.Host.RunWithArgs
                            |> Async.AwaitTask
            response |> should equal DeliverableResponse.Continue
        }
        |> Async.RunSynchronously

    [<Fact>]
    member x.``Creates known tables``() =
        async {
            do! x.DatabaseLocation
                |> x.TextWriter.WriteLineAsync
                |> Async.AwaitTask
            let! _ = [| "install y" |]
                            |> x.Host.RunWithArgs
                            |> Async.AwaitTask
            use connection = new SqlCeConnection(connStrings.["umbracoDbDSN"].ConnectionString)
            let cmd =
                new SqlCeCommand("select table_name from information_schema.tables where TABLE_TYPE <> 'VIEW'",
                                 connection)
            connection.Open()
            let rec testTable (reader : SqlCeDataReader) =
                if reader.Read() then
                    let tableName = reader.GetString 0
                    knownTables |> should contain tableName
                    testTable reader
                else ignore
            cmd.ExecuteReader()
            |> testTable
            |> ignore
        }
        |> Async.RunSynchronously

type ``Unsuccessfully setup the database``() =
    inherit UmbracoHostTestBase()
    [<Fact>]
    member x.``Won't create the database when you say not to``() =
        async {
            do! x.DatabaseLocation
                |> x.TextWriter.WriteLineAsync
                |> Async.AwaitTask
            let! _ = [| "install n" |]
                            |> x.Host.RunWithArgs
                            |> Async.AwaitTask
            use connection = new SqlCeConnection(connStrings.["umbracoDbDSN"].ConnectionString)
            connection.Open |> should throw typeof<SqlCeException>
        }
        |> Async.RunSynchronously