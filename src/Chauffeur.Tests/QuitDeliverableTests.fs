namespace Chauffeur.Tests


module QuitDeliverableTests =
    open Xunit
    open Chauffeur.Deliverables
    open FsUnit.Xunit
    open Chauffeur
    open FSharp.Control.Tasks.V2

    [<Fact>]
    let ``Shutdown response returned when run``() =
        task {
            let deliverable = QuitDeliverable(null, null)

            let! result = deliverable.Run "" Array.empty

            result |> should equal DeliverableResponse.Shutdown
        }