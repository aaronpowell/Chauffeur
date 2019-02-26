namespace Chauffeur.Tests

module QuitDeliverableTests =
    open Xunit
    open Chauffeur.Deliverables
    open FsUnit.Xunit
    open Chauffeur

    [<Fact>]
    let ``Shutdown response returned when run``() =
        let deliverable = QuitDeliverable(null, null)

        deliverable.Run "" Array.empty |> should equal DeliverableResponse.Shutdown