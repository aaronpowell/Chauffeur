module ``Install Deliverable``
    open System.IO
    open Chauffeur
    open Chauffeur.Host
    open Chauffeur.Tests.Integration
    open Xunit
    open FsUnit.Xunit

    open TestHelpers

    [<Fact>]
    let ``Installation should result in a continuation when successful``() =
        let dbFolder = setDataDirectory()

        use writer = new MockTextWriter()
        use reader = new MockTextReader(["Y"])
        use host = new UmbracoHost(reader, writer)

        async {
            do! writer.WriteLineAsync dbFolder |> Async.AwaitTask
            let! response = host.Run([|"install"|]) |> Async.AwaitTask

            response |> should equal DeliverableResponse.Continue
        } |> Async.RunSynchronously

    [<Fact>]
    let ``Installation should create a bunch of umbraco tables``() =
        let dbFolder = setDataDirectory()

        use writer = new MockTextWriter()
        use reader = new MockTextReader(["Y"])
        use host = new UmbracoHost(reader, writer)

        async {
            do! writer.WriteLineAsync dbFolder |> Async.AwaitTask
            let! response = host.Run([|"install"|]) |> Async.AwaitTask

            let connStrings = System.Configuration.ConfigurationManager.ConnectionStrings

            use connection = new System.Data.SqlServerCe.SqlCeConnection(connStrings.["umbracoDbDSN"].ConnectionString)
            let cmd =  new System.Data.SqlServerCe.SqlCeCommand("select table_name from information_schema.tables where TABLE_TYPE <> 'VIEW'", connection)
            connection.Open()
            let reader = cmd.ExecuteReader()

            while reader.Read() do
                let tableName = reader.GetString 0
                List.contains tableName knownTables |> should equal true

        } |> Async.RunSynchronously
