module Packages

open Xunit
open FsUnit.Xunit
open System.IO
open System
open TestSamples
open Chauffeur.TestingTools
open Chauffeur.TestingTools.ChauffeurSetup
open Umbraco.Core

type ``Importing packages``() =
    inherit UmbracoHostTestBase()
    
    member private x.InstallPackage packageName =
        async {
            let run = x.Host.Run
            do! x.DatabaseLocation
                |> x.TextWriter.WriteLineAsync
                |> Async.AwaitTask
            return! [| "package"; packageName |]
                    |> run
                    |> Async.AwaitTask
        }


    [<Fact>]
    member x.``Can import composite document types``() =
        let packageName = "package"
        let run = x.Host.Run
        let _ = x.CreatePackage packageName compositeDocTypeSample

        async {
            let _ = x.InstallUmbraco() |> Async.AwaitTask
            let! _ = x.InstallPackage packageName
            x.TextWriter.Flush()
            let! _ = [| "ct"; "get"; "richTextPage" |]
                                           |> run
                                           |> Async.AwaitTask
            let messages = x.TextWriter.Messages
            messages |> should haveLength 5
            let infoRow =
                messages
                |> List.rev
                |> List.skip 1
                |> List.head

            let parts = infoRow.Split([| '|' |], StringSplitOptions.RemoveEmptyEntries)
            parts.[0].Trim() |> should not' (equal "1050")
            parts.[1].Trim() |> should equal "richTextPage"
            parts.[2].Trim() |> should equal "Rich Text Page"
            parts.[3].Trim() |> should equal "-1"
        }
        |> Async.RunSynchronously

    [<Fact>]
    member x.``Can import updates to Data Type PreValues``() =
        let start, update = preValuesPackage

        x.CreatePackage "01" start |> ignore
        x.CreatePackage "02" update |> ignore

        async {
            let _ = x.InstallUmbraco() |> Async.AwaitTask

            let dts = ApplicationContext.Current.Services.DataTypeService

            let! _ = x.InstallPackage "01"
            let startDataType = dts.GetDataTypeDefinitionByName("Articulate Cropper")
            let startPreValues = dts.GetPreValuesByDataTypeId(startDataType.Id) |> Seq.head

            let! _ = x.InstallPackage "02"
            let updateDataType = dts.GetDataTypeDefinitionByName("Articulate Cropper")
            let updatePreValues = dts.GetPreValuesByDataTypeId(updateDataType.Id) |> Seq.head

            updatePreValues |> should not' (equal startPreValues)
        }
        |> Async.RunSynchronously