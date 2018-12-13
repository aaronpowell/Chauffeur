namespace Chauffeur.Deliverables

open System
open Chauffeur
open FSharp.Control.Tasks.V2
open System.IO.Abstractions

module InstallDeliverable =
    open System.Configuration

    let connStrExists (connStr : ConnectionStringSettings) =
        (connStr |> isNull) || (connStr.ConnectionString |> String.IsNullOrEmpty)

    let isSqlCe (connStr : ConnectionStringSettings) =
        connStr.ProviderName = "System.Data.SqlServerCe.4.0"

    let createSqlCeDb dataDirectory pathCombine fileExists createDb (writeLineAsync : wla) (connStr : string) =
        let split c (s : string) = s.Split(c)
        let trim (s : string) = s.Trim()

        let dataSource = connStr.Split(';')
                         |> Seq.filter(fun s -> s.ToLowerInvariant().Contains("data source"))
                         |> Seq.head

        let dbFileName = dataSource.Split('=')
                         |> Seq.last
                         |> split [|'\\'|]
                         |> Seq.last
                         |> trim

        match fileExists (pathCombine [|dataDirectory;dbFileName|]) with
        | false ->
            task {
                do! writeLineAsync "The SqlCE database specified in the connection string doesn't appear to exist."
                createDb connStr
            }
        | _ -> task { return ignore() }

open Umbraco.Core.Migrations.Install
open InstallDeliverable

[<DeliverableName("install")>]
type InstallDeliverable
     (reader,
      writer,
      settings : IChauffeurSettings,
      fileSystem : IFileSystem,
      databaseBuilder : DatabaseBuilder,
      sqlCeFactory : ISqlCeFactory) =
    inherit Deliverable(reader, writer)

    let connStr = settings.ConnectionString

    let createSqlDb' = createSqlCeDb
                        (AppDomain.CurrentDomain.GetData("DataDirectory") :?> string)
                        fileSystem.Path.Combine
                        fileSystem.File.Exists
                        sqlCeFactory.CreateDatabase
                        writer.WriteLineAsync

    override __.Run _ _ =
        task {
            match connStrExists connStr with
            | true ->
                do! writer.WriteLineAsync("No connection string is setup for your Umbraco instance. Chauffeur expects your web.config to be setup in your deployment package before you try and install.")
                return DeliverableResponse.Continue
            | false ->
                match isSqlCe connStr with
                | true ->
                    do! createSqlDb' connStr.ConnectionString
                | false ->
                    ignore()

                // databaseBuilder.CreateSchemaAndData()

                return DeliverableResponse.Continue
        }
        