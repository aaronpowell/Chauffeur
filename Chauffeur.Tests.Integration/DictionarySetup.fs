module ``Dictionary``

open System.IO
open Chauffeur
open Chauffeur.Host
open Chauffeur.Tests.Integration
open Xunit
open FsUnit.Xunit
open TestHelpers
open System
open TestSamples
open System.Data.SqlServerCe

let connStrings = System.Configuration.ConfigurationManager.ConnectionStrings

let getDictionaryItems = async {
    use connection = new SqlCeConnection(connStrings.["umbracoDbDSN"].ConnectionString)
    let cmd =
        new SqlCeCommand("select * from cmsDictionary",
                            connection)
    connection.Open()
    let! reader = cmd.ExecuteReaderAsync() |> Async.AwaitTask
    let dbSet = new System.Data.DataSet()
    dbSet.Load(reader, System.Data.LoadOption.OverwriteChanges, [| "cmsDictionary" |])

    return dbSet.Tables.["cmsDictionary"]
}

type ``Importing dictionary items``() =
    inherit UmbracoHostTestBase()
    let dictionaryName = "something"

    member private x.ImportDictionaryItems =
        async {
            let run = x.Host.Run
            do! x.DatabaseLocation
                |> x.TextWriter.WriteLineAsync
                |> Async.AwaitTask
            let! response = [| "install" |]
                            |> run
                            |> Async.AwaitTask
            return! [| "dictionary"; dictionaryName |]
                    |> run
                    |> Async.AwaitTask
        }

    [<Fact>]
    member x.``Will import all the dictionary items from the package``() =
        x.TextReader.AddCommand "Y"

        let chauffeurFolder = getChauffeurFolder x.DatabaseLocation
        let filePath =
            Path.Combine [| chauffeurFolder.FullName
                            sprintf "%s.xml" dictionaryName |]
        File.WriteAllText(filePath, sampleDictionary)

        async {
            let! contentTypeImportResponse = x.ImportDictionaryItems
            let messages = x.TextWriter.Messages

            let! tables = getDictionaryItems

            tables.Rows.Count |> should equal 4
        }
        |> Async.RunSynchronously

    [<Fact>]
    member x.``Will import dictionary data properly``() =
        x.TextReader.AddCommand "Y"

        let chauffeurFolder = getChauffeurFolder x.DatabaseLocation
        let filePath =
            Path.Combine [| chauffeurFolder.FullName
                            sprintf "%s.xml" dictionaryName |]
        File.WriteAllText(filePath, sampleDictionary)

        async {
            let! contentTypeImportResponse = x.ImportDictionaryItems
            let messages = x.TextWriter.Messages

            let! table = getDictionaryItems

            table.Rows.[0].["Key"] |> should equal "TestTwo"
            table.Rows.[1].["Key"] |> should equal "TestTwo.Child2"
            table.Rows.[2].["Key"] |> should equal "TestTwo.Child1"
            table.Rows.[3].["Key"] |> should equal "TestOne"
        }
        |> Async.RunSynchronously
