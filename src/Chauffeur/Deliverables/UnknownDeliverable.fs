namespace Chauffeur.Deliverables

open Chauffeur
open FSharp.Control.Tasks.V2

[<DeliverableName("unknown")>]
type UnknownDeliverable(reader, writer) =
    inherit Deliverable(reader, writer)

    override __.Run name args =
        task {
            let cmd = [| name |]
                      |> Array.append args
                      |> String.concat " "
            do! writer.WriteLineAsync(sprintf "Unknown command '%s' entered, check `help` for available commands" cmd)
            return DeliverableResponse.Continue
        }

    interface IProvideDirections with
        member __.Directions() =
            task {
                do! writer.WriteLineAsync("Seriously, you're asking for help on the unknown command? Good luck with that")
                return true
            }
