module ``Install Deliverable``
    open System.IO
    open Chauffeur
    open Chauffeur.Host
    open Chauffeur.Tests.Integration
    open Xunit
    open FsUnit.Xunit

    open TestHelpers

    [<Fact>]
    let ``Installation should result in a continuation when successful``() =
        let dbFolder = setDataDirectory()

        use writer = new MockTextWriter()
        use reader = new MockTextReader(["Y"])
        let host = UmbracoHost(reader, writer)

        async {
            do! writer.WriteLineAsync dbFolder |> Async.AwaitTask
            let! response = host.Run([|"install"|]) |> Async.AwaitTask

            response |> should equal DeliverableResponse.Continue
        } |> Async.RunSynchronously