namespace Chauffeur.Tests

module UnknownDeliverableTests =
    open Xunit
    open Chauffeur.Deliverables
    open Chauffeur.Tests
    open FsUnit.Xunit
    open Chauffeur
    open FSharp.Control.Tasks.V2

    [<Fact>]
    let ``Will write a message when called``() =
        task {
            let reader = new MockTextWriter()
            let deliverable = UnknownDeliverable(null, reader)

            let! _ = deliverable.Run "foo" [|"a"; "b"|]

            reader.Messages |> should haveLength 1

            let msg = reader.Messages |> List.head

            msg |> should equal "Unknown command 'foo a b' entered, check `help` for available commands"
        }

    [<Fact>]
    let ``Will return successfully when called``() =
        task {
            let deliverable = UnknownDeliverable(null, new MockTextWriter())

            let! result = deliverable.Run "foo" Array.empty

            result |> should equal DeliverableResponse.Continue
        }