module ``Upgrade Deliverable``

open Xunit
open FsUnit.Xunit
open Chauffeur.TestingTools
open Chauffeur

type ``Upgrade Umbraco``() =
    inherit UmbracoHostTestBase()

    [<Fact>]
    member x.``Upgrade will run return a minor error if no upgrade can be done``() =
        async {
            let! _ = x.InstallUmbraco() |> Async.AwaitTask

            let! response = x.Host.Run([| "upgrade" |]) |> Async.AwaitTask

            response |> should equal DeliverableResponse.FinishedWithError
        } |> Async.RunSynchronously

    [<Fact>]
    member x.``Upgrade will return a message of no work to do if already at the latests``() =
        async {
            let! _ = x.InstallUmbraco() |> Async.AwaitTask

            x.TextWriter.Flush()

            let! _ = x.Host.Run([| "upgrade" |]) |> Async.AwaitTask

            x.TextWriter.Messages |> should haveLength 1
        } |> Async.RunSynchronously
        
    [<Fact>]
    member x.``Upgrade will return a message saying there is no pending upgrade``() =
        async {
            let! _ = x.InstallUmbraco() |> Async.AwaitTask
        
            x.TextWriter.Flush()
        
            let! response = x.Host.Run([| "upgrade check" |]) |> Async.AwaitTask
        
            response |> should equal DeliverableResponse.FinishedWithError
            x.TextWriter.Messages |> should haveLength 1
        } |> Async.RunSynchronously
               
