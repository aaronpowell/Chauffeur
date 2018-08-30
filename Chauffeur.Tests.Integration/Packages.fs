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