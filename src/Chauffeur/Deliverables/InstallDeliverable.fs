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

    let internal makeDb writeLineAsync prompt (createDb : unit -> unit) args =
        task {
            do! writeLineAsync "The SqlCE database specified in the connection string doesn't appear to exist."
            let! response = match args |> Array.toList with
                            | "y" :: _ | "Y" :: _ -> task { return "Y" }
                            | _ -> prompt "Do you want to create it (Y/n)?" "Y"

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

open Umbraco.Core.Migrations.Install
open InstallDeliverable
open Umbraco.Core.Logging
open Umbraco.Core.Scoping

[<DeliverableName("install")>]
type InstallDeliverable
     (reader,
      writer,
      settings : IChauffeurSettings,
      fileSystem : IFileSystem,
      sqlCeFactory : ISqlCeFactory,
      logger : ILogger,
      scopeProvider : IScopeProvider) =
    inherit Deliverable(reader, writer)

    let connStr = settings.ConnectionString

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
                    match fileSystem.File.Exists dbPath with
                    | true -> ignore()
                    | false ->
                        let! _ = makeDb
                                        writer.WriteLineAsync
                                        (prompt writer.WriteAsync reader.ReadLineAsync)
                                        (fun () -> sqlCeFactory.CreateDatabase connStr.ConnectionString)
                                        args
                        ignore()
                | false -> ignore()

                use scope = scopeProvider.CreateScope()
                let umbracoDatabase = scope.Database
                let creator = DatabaseSchemaCreator(umbracoDatabase, logger)
                creator.InitializeDatabaseSchema()
                scope.Complete() |> ignore

                match scope.Complete() with
                | true -> return DeliverableResponse.Continue
                | false ->
                    do! writer.WriteLineAsync("Failed to install db. Check the Umbraco logs.")
                    return DeliverableResponse.FinishedWithError
        }
