namespace Chauffeur.Deliverables

open Chauffeur
open FSharp.Control.Tasks.V2
open Umbraco.Core.Services

[<DeliverableName("change-alias")>]
[<DeliverableAlias("ca")>]
type ChangeAliasDeliverable(reader, writer, contentTypeService : IContentTypeService) =
    inherit Deliverable(reader, writer)

    override __.Run _ args =
        match args with
        | [|"document-type"; old; ``new``|]
        | [|"doc-type"; old; ``new``|]
        | [|"dt"; old; ``new``|] -> task {
            let ct = contentTypeService.Get old
            ct.Alias <- ``new``
            contentTypeService.Save ct
            return DeliverableResponse.Continue }

        | _ -> task {
            do! writer.WriteLineAsync "In valid arguments, expected format of `change-alias what old new`."
            return DeliverableResponse.Continue }