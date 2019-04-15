module Packaging
open System
open System.Xml.Linq
open Umbraco.Core.Models.Packaging
open Umbraco.Core.Models
open Umbraco.Core.Packaging
open Umbraco.Core.PropertyEditors
open Umbraco.Core.Services
open Umbraco.Core

let inline xel s (xml : XElement) = s |> XName.Get |> xml.Element
let inline xels s (xml : XElement) = s |> XName.Get |> xml.Elements
let inline xev (x : XElement) = x.Value

let (|InvariantEqual|_|) (str : string) arg = 
  if String.Compare(str, arg, StringComparison.OrdinalIgnoreCase) = 0
    then Some() else None

let createPackageDefinition =
    let pkgDef = PackageDefinition()
    pkgDef.DocumentTypes <- new System.Collections.Generic.List<string>()
    pkgDef

let createCompiledPackage =
    let pkgCompiled = CompiledPackage()
    pkgCompiled.DocumentTypes <- Array.empty
    pkgCompiled.DataTypes <- Array.empty
    pkgCompiled.Templates <- Array.empty
    pkgCompiled.DictionaryItems <- Array.empty
    pkgCompiled.Macros <- Array.empty
    pkgCompiled.Stylesheets <- Array.empty
    pkgCompiled.Documents <- Array.empty
    pkgCompiled.Languages <- Array.empty
    pkgCompiled.Actions <- "<Actions></Actions>"
    pkgCompiled

type IPackageInstallWrapper =
    abstract ImportPackage : PackageDefinition -> CompiledPackage -> InstallationSummary

type PackageImportWrapper
        (packageInstaller : IPackageInstallation,
         dataTypeService : IDataTypeService,
         contentTypeService : IContentTypeService) =

    let editorByAlias alias =
        match dataTypeService.GetByEditorAlias alias with
        | null ->
            dataTypeService.GetByEditorAlias(Constants.PropertyEditors.Aliases.Label) |> Seq.head
        | types when types |> Seq.isEmpty ->
            dataTypeService.GetByEditorAlias(Constants.PropertyEditors.Aliases.Label) |> Seq.head
        | types  -> types |> Seq.head

    let getDataTypeDef (property : XElement) =
        let id = Guid(xel "Definition" property |> xev)
        let alias = xel "Type" property |> xev
        match dataTypeService.GetDataType id with
        | null -> editorByAlias alias
        | dt when dt.EditorAlias = alias -> dt
        | _ -> editorByAlias alias

    let mapPropertyType (dt : IContentType) = fun property ->
        let sortOrder = match xel "SortOrder" property with
                        | null -> 0
                        | el ->
                            match Int32.TryParse el.Value with
                            | (true, sortOrder) -> sortOrder
                            | _ -> 0

        let pt = dt.PropertyTypes |> Seq.find (fun pt -> pt.Alias = (xel "Alias" property |> xev))
        pt.SortOrder <- sortOrder
        pt.Name <- xel "Name" property |> xev
        pt.Description <- xel "Description" property |> xev
        pt.Mandatory <- match xel "Mandatory" property |> xev with
                        | InvariantEqual "true" -> true
                        | _ -> false
        pt.ValidationRegExp <- xel "Validation" property |> xev
        let dt = getDataTypeDef property
        pt.DataTypeId <- dt.Id

    interface IPackageInstallWrapper with
        member __.ImportPackage def compiled =
            let summary = packageInstaller.InstallPackageData(def, compiled, 0)

            match summary.DocumentTypesInstalled |> Seq.toList with
            | [] -> summary
            | docTypes ->
                docTypes
                |> List.map (fun dt ->
                    let xml = compiled.DocumentTypes
                                |> Seq.find (fun x ->
                                            let alias = xel "Info" x
                                                        |> xel "Alias"
                                            alias.Value = dt.Alias)
                    (xml, dt))
                |> List.map (fun (xml, dt) ->
                    let properties = xel "GenericProperties" xml
                                        |> xels "GenericProperty"
                    (properties, dt))
                |> List.iter (fun (properties, dt) ->
                                properties |> Seq.iter(mapPropertyType dt)
                                contentTypeService.Save dt)
                summary
