namespace Chauffeur.Deliverables

open System
open Chauffeur
open Umbraco.Core.Services
open System.IO.Abstractions
open FSharp.Control.Tasks.V2
open System.Xml.Linq
open Packaging

[<DeliverableName("data-type")>]
type DataTypeDeliverable
    (reader,
     writer,
     dataTypeService : IDataTypeService,
     fileSystem : IFileSystem,
     settings : IChauffeurSettings,
     serializer : IEntityXmlSerializer,
     packageInstallation : IPackageInstallWrapper) =
     inherit Deliverable(reader, writer)

     override __.Run _ args =
        match args |> Array.toList with
        | "import" :: fileName :: _ -> task {
            match settings.TryGetChauffeurDirectory() with
            | (true, dir) ->
                let importFile = fileSystem.Path.Combine(dir, sprintf "%s.xml" fileName)
                match (fileSystem.File.Exists importFile) with
                | true ->
                    let xml = XDocument.Load importFile
                    let pkgDef = createPackageDefinition
                    let pkgCompiled = createCompiledPackage
                    pkgCompiled.DataTypes <- [| xml.Root |]
                    let _ = packageInstallation.ImportPackage pkgDef pkgCompiled

                    do! writer.WriteLineAsync("Data Type Definitions has been imported")
                | false -> do! writer.WriteLineAsync(sprintf "Unable to locate the import script '%s'" fileName)
            | (false, _) ->
                ignore()
            return DeliverableResponse.Continue
            }

        | "export" :: ids -> task {
            match settings.TryGetChauffeurDirectory() with
            | (false, _) -> return DeliverableResponse.Continue
            | (true, dir) ->
                let dataTypes = match ids |> List.length with
                                | 0 -> dataTypeService.GetAll()
                                | _ -> 
                                    let parsed = ids |> List.map(fun i -> Int32.Parse(i)) |> List.toArray
                                    dataTypeService.GetAll parsed

                let xml = XDocument(serializer.Serialize dataTypes)
                let filename = DateTime.UtcNow.ToString("yyyyMMdd") + "-data-type-definitions.xml"
                fileSystem.File.WriteAllText(fileSystem.Path.Combine(dir, filename), xml.ToString())

                do! writer.WriteLineAsync(sprintf "Data Type Definitions have been exported with file name '%s'" filename)

                return DeliverableResponse.Continue
            }

        | op :: _ -> task {
            do! writer.WriteLineAsync(sprintf "The operation `%s` is not currently supported" op)
            return DeliverableResponse.Continue
            }

        | _ -> task {
            do! writer.WriteLineAsync "Please specify a command to run and any arguments it requires"
            return DeliverableResponse.Continue
            }