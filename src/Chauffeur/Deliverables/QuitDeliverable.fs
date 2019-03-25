namespace Chauffeur.Deliverables

open Chauffeur
open FSharp.Control.Tasks.V2

[<DeliverableName("quit")>]
[<DeliverableAlias("q")>]
type QuitDeliverable(reader, writer) =
    inherit Deliverable(reader, writer)
    override __.Run _ _ = task { return DeliverableResponse.Shutdown }

