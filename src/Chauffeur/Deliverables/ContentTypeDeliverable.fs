namespace Chauffeur.Deliverables

open System
open Chauffeur
open FSharp.Control.Tasks.V2
open Umbraco.Core.Services
open System.IO.Abstractions
open System.Xml.Linq

module ContentTypeDeliverable =
    open Umbraco.Core.Models
    open System.IO
    open System.Xml.Linq

    let get getById getByAlias id =
        match Int32.TryParse id with
        | (true, id) -> getById id
        | (false, _) -> getByAlias id

    let print (writer : TextWriter) (ct : IContentType) = task {
        let record = 
            {| Id = ct.Id
               Alias = ct.Alias
               Name = ct.Name
               ``Parent Id`` = ct.ParentId |}

        do! TextWriterExtensions.WriteTableAsync writer [| record |] (Seq.empty |> dict)

        do! writer.WriteLineAsync "Property Types"

        let boolToWords b =
            if b then
                "Yes"
            else
                "No"

        let properties = ct.PropertyTypes
                         |> Seq.map (fun p -> {| Id = p.Id
                                                 Name = p.Name
                                                 Alias = p.Alias
                                                 Mandatory = p.Mandatory |> boolToWords
                                                 ``Property Editor Alias`` = p.PropertyEditorAlias |})
                        |> Seq.toArray

        do! TextWriterExtensions.WriteTableAsync writer properties (Seq.empty |> dict)
    }

    let getAll (getAll : unit -> seq<IContentType>) (writer : TextWriter) =
        let types = getAll()

        match types |> Seq.length with
        | 0 -> task {
            do! writer.WriteLineAsync "No content types found." }
        | _ -> task {
            let printable = types
                            |> Seq.map (fun t -> {| Id = t.Id
                                                    Alias = t.Alias
                                                    Name = t.Name |})
            do! TextWriterExtensions.WriteTableAsync writer printable (Seq.empty |> dict)
            }

    let export (serializer : IContentType -> XElement) contentType =
        let xml = XDocument()
        contentType |> serializer |> xml.Add
        xml

open ContentTypeDeliverable
open Umbraco.Core.Models
open Umbraco.Core.Packaging
open Umbraco.Core.Models.Packaging

[<DeliverableName("content-type")>]
[<DeliverableAlias("ct")>]
type ContentTypeDeliverable
     (reader,
      writer,
      contentTypeService : IContentTypeService,
      serializer : IEntityXmlSerializer,
      fileSystem : IFileSystem,
      settings : IChauffeurSettings,
      packageInstallation : IPackageInstallation) =
    inherit Deliverable(reader, writer)

    let byId (id : int) = contentTypeService.Get id
    let byAlias (alias : string) = contentTypeService.Get alias
    let get' = get byId byAlias

    override __.Run _ args =
        match args |> Array.toList with
        | "get" :: id :: _ -> task {
            do! get' id |> print writer
            return DeliverableResponse.Continue }

        | "get-all" :: _ -> task {
            do! getAll contentTypeService.GetAll writer
            return DeliverableResponse.Continue }

        | "export" :: id :: _ -> task {
            match settings.TryGetChauffeurDirectory() with
            | (true, dir) ->
                let s (ct : IContentType) = serializer.Serialize ct
                let ct = get' id
                let xml = export s ct
                let fileName = DateTime.UtcNow.ToString("yyyyMMdd") + "-" + ct.Alias + ".xml"
                fileSystem.Path.Combine(dir, fileName) |> xml.Save
            | _ ->
                ignore()
            return DeliverableResponse.Continue }

        | "import" :: fileName :: _ -> task {
            match settings.TryGetChauffeurDirectory() with
            | (true, dir) ->
                let importFile = fileSystem.Path.Combine(dir, sprintf "%s.xml" fileName)
                match (fileSystem.File.Exists importFile) with
                | true ->
                    let xml = XDocument.Load importFile
                    let pkgDef = PackageDefinition()
                    pkgDef.DocumentTypes <- new System.Collections.Generic.List<string>()
                    let pkgCompiled = CompiledPackage()
                    pkgCompiled.DocumentTypes <- [| xml.Root |]
                    pkgCompiled.DataTypes <- Array.empty
                    pkgCompiled.Templates <- Array.empty
                    pkgCompiled.DictionaryItems <- Array.empty
                    pkgCompiled.Macros <- Array.empty
                    pkgCompiled.Stylesheets <- Array.empty
                    pkgCompiled.Documents <- Array.empty
                    pkgCompiled.Languages <- Array.empty
                    pkgCompiled.Actions <- "<Actions></Actions>"
                    let summary = packageInstallation.InstallPackageData(pkgDef, pkgCompiled, 0)

                    do! writer.WriteLineAsync("Content Type has been imported")
                | false ->
                    do! writer.WriteLineAsync(sprintf "Unable to locate the import script '%s'" fileName)
            | (false, _) ->
                ignore()

            return DeliverableResponse.Continue }

        | "remove" :: id :: _ -> task {
            get' id |> contentTypeService.Delete
            do! writer.WriteLineAsync(sprintf "Removed the content type '%s'." id)
            return DeliverableResponse.Continue }

        | "remove-property" :: id :: properties -> task {
            let ct = get' id
            properties
            |> List.iter ct.RemovePropertyType

            contentTypeService.Save ct

            do! writer.WriteLineAsync(sprintf "Remvoed the following property types: %s" (String.Join(", ", properties |> List.toArray)))
            return DeliverableResponse.Continue }

        | operation :: _ -> task {
            do! writer.WriteLineAsync(sprintf "The operation `%s` is not supported" operation)
            return DeliverableResponse.Continue }

        | [] -> task {
            do! writer.WriteLineAsync "Invalid arguments provided"
            return DeliverableResponse.Continue }