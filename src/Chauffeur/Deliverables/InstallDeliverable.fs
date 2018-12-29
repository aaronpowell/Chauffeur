namespace Chauffeur.Deliverables

open System
open Chauffeur
open FSharp.Control.Tasks.V2
open System.IO.Abstractions
open System.Threading.Tasks

type wla = string -> Task
type rla = unit -> Task<string>
type cdb = string -> unit

module InstallDeliverable =
    open System.Configuration

    let connStrExists (connStr : ConnectionStringSettings) =
        (connStr |> isNull) || (connStr.ConnectionString |> String.IsNullOrEmpty)

    let isSqlCe (connStr : ConnectionStringSettings) =
        connStr.ProviderName = "System.Data.SqlServerCe.4.0"

    let prompt (writeAsync : wla) readLineAsync (message : string) defaultValue =
        task {
            do! writeAsync message
            let! value = readLineAsync()
            return match value with
                   | value when value = String.Empty -> defaultValue
                   | _ -> value
        }

    let internal makeDb writeLineAsync readLineAsync (createDb : unit -> unit) args =
        task {
            do! writeLineAsync "The SqlCE database specified in the connection string doesn't appear to exist."
            let! response = match args |> Array.toList with
                            | "y" :: _ | "Y" :: _ -> task { return "Y" }
                            | _ -> prompt writeLineAsync readLineAsync "" "Y"

            return! match response with
                    | "Y" | "y" -> task {
                        do! writeLineAsync "Creating the database"
                        createDb()
                        return true
                     }
                    | _ -> task {
                        do! writeLineAsync "Installation is being aborted"
                        return false
                     }
        }

    let getSqlCePath dataDirectory pathCombine (connStr : string) =
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

        pathCombine [|dataDirectory;dbFileName|]

    let createSqlCeDb (readLineAsync : rla)
                      fileExists
                      (createDb : unit -> unit)
                      (writeLineAsync : wla)
                      args
                      dbPath =
        match fileExists dbPath with
        | true -> task { return Some(dbPath) }
        | false -> task {
            let! result = makeDb writeLineAsync readLineAsync createDb args
            match result with
            | true -> return Some(dbPath)
            | false -> return None
        }

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
                        reader.ReadLineAsync
                        fileSystem.File.Exists
                        (fun () -> sqlCeFactory.CreateDatabase connStr.ConnectionString)
                        writer.WriteLineAsync

    override __.Run _ args =
        task {
            match connStrExists connStr with
            | true ->
                do! writer.WriteLineAsync("No connection string is setup for your Umbraco instance. Chauffeur expects your web.config to be setup in your deployment package before you try and install.")
                return DeliverableResponse.Continue
            | false ->
                match isSqlCe connStr with
                | true ->
                    let dbPath = getSqlCePath (AppDomain.CurrentDomain.GetData("DataDirectory") :?> string) fileSystem.Path.Combine connStr.ConnectionString
                    let! _ = createSqlDb' args dbPath
                    ignore()
                | false ->
                    ignore()

                let result = databaseBuilder.CreateSchemaAndData()

                match result.Success with
                | true -> return DeliverableResponse.Continue
                | false ->
                    do! writer.WriteLineAsync(sprintf "Failed to install db: %s" result.Message)
                    return DeliverableResponse.FinishedWithError
        }
        