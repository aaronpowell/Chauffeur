namespace Chauffeur.Tests

module UnknownDeliverableTests =
    open Xunit
    open Chauffeur.Deliverables
    open Chauffeur.Tests
    open FsUnit.Xunit
    open Chauffeur

    [<Fact>]
    let ``Will write a message when called``() =
        let reader = new MockTextWriter()
        let deliverable = UnknownDeliverable(null, reader)

        let _ = deliverable.Run "foo" Array.empty

        reader.Messages |> should haveLength 1

    [<Fact>]
    let ``Will return successfully when called``() =
        let deliverable = UnknownDeliverable(null, new MockTextWriter())

        let result = deliverable.Run "foo" Array.empty

        result |> should equal DeliverableResponse.Continue