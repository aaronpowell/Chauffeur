module ``User Deliverable``

open TestHelpers
open Xunit
open FsUnit.Xunit
open Chauffeur
open System.Data.SqlServerCe

let connStrings = System.Configuration.ConfigurationManager.ConnectionStrings

type ``Change password``() =
    inherit UmbracoHostTestBase()

    [<Fact>]
    member x.``Can change the password of a user``() =
        async {
            do! x.DatabaseLocation
                |> x.TextWriter.WriteLineAsync
                |> Async.AwaitTask

            let run input =
                input
                |> x.Host.Run
                |> Async.AwaitTask

            let! _ = [|"install"; "y"|]
                        |> run
            let! _ =  [|"user"; "change-password"; "admin"; "password" |]
                        |> run

            use connection = new SqlCeConnection(connStrings.["umbracoDbDSN"].ConnectionString)
            let cmd =
                new SqlCeCommand("SELECT [UserPassword] FROM [umbracoUser]",
                                 connection)
            connection.Open()
            let rec testTable (reader : SqlCeDataReader) =
                if reader.Read() then
                    let password = reader.GetString 0
                    password |> should equal "/7IIcyNxAts3fvQYe2PI3d19cDU="
                    testTable reader
                else ignore
            cmd.ExecuteReader()
            |> testTable
            |> ignore
        }
        |> Async.RunSynchronously
