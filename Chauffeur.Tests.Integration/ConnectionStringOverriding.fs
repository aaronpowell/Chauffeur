module ``Connection string override``

open Chauffeur.Host
open Xunit
open FsUnit.Xunit
open TestHelpers
open System.Configuration

type ``Override connection string as argument``() =
    inherit UmbracoHostTestBase()

    [<Fact>]
    member x.``Setting connection string via argument sets it on the config manager``() =
        let run = x.Host.Run
        let connectionString = "Data Source=blah;Initial Catalog=blah;UID=blah;password=blah"
        [| sprintf "-c:%s" connectionString; "q" |]
            |> run
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> ignore

        ConfigurationManager.ConnectionStrings.["umbracoDbDSN"].ConnectionString
            |> should equal connectionString

    [<Fact>]
    member x.``Setting connection string via argument won't set it on disk``() =
        let run = x.Host.Run
        let connectionString = "Data Source=blah;Initial Catalog=blah;UID=blah;password=blah"
        [| sprintf "-c:%s" connectionString; "q" |]
            |> run
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> ignore

        let config = ConfigurationManager.OpenExeConfiguration ConfigurationUserLevel.None
        config.ConnectionStrings.ConnectionStrings.["umbracoDbDSN"].ConnectionString
            |> should not' (equal connectionString)
