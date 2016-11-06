module ``Install Deliverable``
    open System.IO
    open Chauffeur
    open Chauffeur.Host
    open Chauffeur.Tests.Integration
    open Xunit
    open FsUnit.Xunit

    open TestHelpers

    let setup() =
        let dbFolder = setDataDirectory()

        let writer = new MockTextWriter()
        let reader = new MockTextReader()
        let host = new UmbracoHost(reader, writer)

        (dbFolder, host, writer, reader)

    [<Fact>]
    let ``Installation should result in a continuation when successful``() =
        let dbFolder, host, writer, reader = setup()

        reader.AddCommand "Y"

        async {
            do! writer.WriteLineAsync dbFolder |> Async.AwaitTask
            let! response = host.Run([|"install"|]) |> Async.AwaitTask

            writer.Dispose()
            reader.Dispose()
            host.Dispose()

            response |> should equal DeliverableResponse.Continue
        } |> Async.RunSynchronously

    [<Fact>]
    let ``Installation should create a bunch of umbraco tables``() =
        let dbFolder, host, writer, reader = setup()

        reader.AddCommand "Y"

        async {
            do! writer.WriteLineAsync dbFolder |> Async.AwaitTask
            let! response = host.Run([|"install"|]) |> Async.AwaitTask

            writer.Dispose()
            reader.Dispose()
            host.Dispose()

            let connStrings = System.Configuration.ConfigurationManager.ConnectionStrings

            use connection = new System.Data.SqlServerCe.SqlCeConnection(connStrings.["umbracoDbDSN"].ConnectionString)
            let cmd =  new System.Data.SqlServerCe.SqlCeCommand("select table_name from information_schema.tables where TABLE_TYPE <> 'VIEW'", connection)
            connection.Open()
            let reader = cmd.ExecuteReader()

            while reader.Read() do
                let tableName = reader.GetString 0
                List.contains tableName knownTables |> should equal true

        } |> Async.RunSynchronously
