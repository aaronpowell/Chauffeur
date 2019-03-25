namespace Chauffeur.Deliverables

open Chauffeur
open Chauffeur.Components
open Umbraco.Core
open Umbraco.Core.Composing
open FSharp.Control.Tasks.V2
open System.Threading.Tasks

[<DeliverableName("help")>]
[<DeliverableAlias("h")>]
[<DeliverableAlias("?")>]
type HelpDeliverable(reader, writer, container : IFactory) =
    inherit Deliverable(reader, writer)

    let printDeliverable name =
        let resolver = container.GetInstance<DeliverableResolver>()
        match resolver.Resolve name with
        | Some deliverable ->
            task {
                match box deliverable with
                | :? IProvideDirections as hd ->
                    let! _ = hd.Directions()
                    ignore()
                | _ ->
                    let msg = sprintf "The deliverable '%s' doesn't implement help, you best contact the author" name
                    do! writer.WriteLineAsync msg

                return DeliverableResponse.Continue
            }
        | None ->
            task {
                let msg = sprintf "The deliverable '%s' doesn't implement help, you best contact the author" name
                do! writer.WriteLineAsync msg
                return DeliverableResponse.Continue
            }

    let printAll() =
        let metadata d =
            let type' = d.GetType()
            let name = type'.GetCustomAttribute<DeliverableNameAttribute>(false).Name
            let aliases = type'.GetCustomAttributes<DeliverableAliasAttribute>(false) |> Seq.map (fun attr -> attr.Alias)

            (name, aliases)

        task {
            do! writer.WriteLineAsync "The following deliverables are loaded. Use `help <deliverable>` for detailed help."

            let deliverables = container.GetAllInstances<Deliverable>()

            let tasks = deliverables
                        |> Seq.map(metadata)
                        |> Seq.map(fun (name, aliases) ->
                                    let msg = match aliases |> Seq.length with
                                              | 0 -> name
                                              | _ ->
                                                sprintf "%s (aliases: %s)" name (aliases |> String.concat(", "))

                                    writer.WriteLineAsync(msg))

            do! Task.WhenAll(tasks)

            return DeliverableResponse.Continue
        }

    override __.Run _ args =
        task {
            match args |> Array.toList with
            | name :: _ ->
                return! printDeliverable name
            | [] ->
                return! printAll()
        }

    interface IProvideDirections with
        member __.Directions() =
            task {
                do! writer.WriteLineAsync("help")
                do! writer.WriteLineAsync("\taliases: h, ?")
                do! writer.WriteLineAsync("\tUse `help` to display system help")
                do! writer.WriteLineAsync("\tUse `help <Deliverable>` to display help for a deliverable")

                return true
            }