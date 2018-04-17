module ``User Deliverable``

open Xunit
open FsUnit.Xunit
open System.Data.SqlServerCe
open Chauffeur.TestingTools

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

type ``Create User``() as this =
    inherit UmbracoHostTestBase()

    let run input =
        input
        |> this.Host.Run
        |> Async.AwaitTask

    [<Fact>]
    member _x.``Use created if their name doesn't have a space``() =
        async {
            let! _ = [|"install"; "y"|]
                        |> run

            let! _ = [| "user"; "create-user"; "Aaron"; "aaron"; "email@place.com"; "password!1"; "admin" |]
                        |> run

            use connection = new SqlCeConnection(connStrings.["umbracoDbDSN"].ConnectionString)
            let cmd = new SqlCeCommand("SELECT COUNT(*) FROM [umbracoUser]", connection)

            connection.Open()

            cmd.ExecuteScalar() |> should equal 2
        } |> Async.RunSynchronously

    [<Fact>]
    member _x.``User created with a multi-part name``() =
        async {
            let! _ = [|"install"; "y"|]
                        |> run

            let! _ = [| "user"; "create-user"; "\"Aaron Powell\""; "aaron"; "email@place.com"; "password!1"; "admin" |]
                        |> run

            use connection = new SqlCeConnection(connStrings.["umbracoDbDSN"].ConnectionString)
            let cmd = new SqlCeCommand("SELECT COUNT(*) FROM [umbracoUser]", connection)

            connection.Open()

            cmd.ExecuteScalar() |> should equal 2
        } |> Async.RunSynchronously
