namespace Chauffeur.Deliverables

open Chauffeur
open FSharp.Control.Tasks.V2

[<DeliverableName("install")>]
type InstallDeliverable(reader, writer, settings : IChauffeurSettings) =
    inherit Deliverable(reader, writer)

    override __.Run _ _ =
        task {
            do! writer.WriteLineAsync(sprintf "ConnStr: %s" (settings.ConnectionString.ConnectionString))

            return DeliverableResponse.Continue
        }
        