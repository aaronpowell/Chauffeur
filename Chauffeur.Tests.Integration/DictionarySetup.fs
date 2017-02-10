module ``Dictionary``
//
//open System.IO
//open Chauffeur
//open Chauffeur.Host
//open Chauffeur.Tests.Integration
//open Xunit
//open FsUnit.Xunit
//open TestHelpers
//open System
//
//type ``Importing dictionary items``() =
//    inherit UmbracoHostTestBase()
//
//    member private x =
//        async {
//            let run = x.Host.Run
//            do! x.DatabaseLocation
//                |> x.TextWriter.WriteLineAsync
//                |> Async.AwaitTask
//            let! response = [| "install" |]
//                            |> run
//                            |> Async.AwaitTask
//            return! [| "ct"; "import"; doctypeName |]
//                    |> run
//                    |> Async.AwaitTask
//        }
//
//    [<Fact>]
//    member x.``Will log an error if you don't have the import file on disk``() =
//        x.TextReader.AddCommand "Y"
//        async {
//            let! contentTypeImportResponse = x.ImportDocType
//            let messages = x.TextWriter.Messages
//            List.head messages |> should equal (sprintf "Unable to locate the import script '%s'" doctypeName)
//        }
//        |> Async.RunSynchronously
