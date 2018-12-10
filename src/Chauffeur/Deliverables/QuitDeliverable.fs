namespace Chauffeur.Deliverables

open Chauffeur
open System.Threading.Tasks

[<DeliverableName("quit")>]
[<DeliverableAlias("q")>]
type QuitDeliverable(reader, writer) =
    inherit Deliverable(reader, writer)
    override __.Run _ _ = Task.FromResult(DeliverableResponse.Shutdown)

