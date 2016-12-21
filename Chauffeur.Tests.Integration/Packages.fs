module Packages

open Xunit
open TestHelpers
open FsUnit.Xunit
open System.IO
open System
open TestSamples

type ``Importing packages``() =
    inherit UmbracoHostTestBase()
    
    let packageName = "package"
    member private x.InstallPackage =
        async {
            let run = x.Host.Run
            do! x.DatabaseLocation
                |> x.TextWriter.WriteLineAsync
                |> Async.AwaitTask
            let! response = [| "install" |]
                            |> run
                            |> Async.AwaitTask
            return! [| "package"; packageName |]
                    |> run
                    |> Async.AwaitTask
        }


    [<Fact>]
    member x.``Can import composite document types``() =
        x.TextReader.AddCommand "Y"
        let run = x.Host.Run
        let chauffeurFolder = getChauffeurFolder x.DatabaseLocation
        let filePath =
            Path.Combine [| chauffeurFolder.FullName
                            sprintf "%s.xml" packageName |]
        File.WriteAllText(filePath, compositeDocTypeSample)

        async {
            let! contentTypeImportResponse = x.InstallPackage
            x.TextWriter.Flush()
            let! contentTypeInfoResponse = [| "ct"; "get"; "richTextPage" |]
                                           |> run
                                           |> Async.AwaitTask
            let messages = x.TextWriter.Messages
            messages |> should haveLength 5
            let infoRow =
                messages
                |> List.rev
                |> List.skip 1
                |> List.head

            let parts = infoRow.Split([| '\t' |], StringSplitOptions.RemoveEmptyEntries)
            parts.[0] |> should not' (equal "1050")
            parts.[1] |> should equal "richTextPage"
            parts.[2] |> should equal "Rich Text Page"
            parts.[3] |> should equal "-1"
        }
        |> Async.RunSynchronously

