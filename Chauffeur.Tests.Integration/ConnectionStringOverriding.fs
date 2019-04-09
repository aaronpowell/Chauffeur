module ``Connection string override``

open Xunit
open FsUnit.Xunit
open System.Configuration
open Chauffeur.TestingTools
open Chauffeur.TestingTools.ChauffeurSetup

type ``Override connection string as argument``() =
    inherit UmbracoHostTestBase()

    [<Fact(Skip="It impacts beyond just this test I think")>]
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

    [<Fact(Skip="It impacts beyond just this test I think")>]
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
