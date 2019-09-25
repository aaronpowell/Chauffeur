namespace Chauffeur.Deliverables

open System
open System.Data.Common
open System.IO
open Chauffeur
open FSharp.Control.Tasks.V2
open System.IO.Abstractions
open Chauffeur.Host
open Umbraco.Core.Scoping
open Umbraco.Core.Logging
open Umbraco.Core.Migrations.Install
open Umbraco.Core.Migrations
open Umbraco.Core.Persistence
open Umbraco.Core.Migrations.Expressions.Create
open NPoco
open Umbraco.Core.Persistence.DatabaseAnnotations
open System.Threading.Tasks
open System.Text
open System.Text.RegularExpressions
open System.Collections.Generic

type ChauffeurMockMigrationContext(database : IUmbracoDatabase, logger : ILogger) =
    let mutable postMigrations = Seq.empty
    interface IMigrationContext with
        member this.AddPostMigration<'T when 'T :> IMigration>() =
            postMigrations <- Seq.append postMigrations [typeof<'T>]
        member val BuildingExpression = false with get, set
        member __.Database = database
        member val Index = 0 with get, set
        member __.Logger = logger
        member __.SqlContext = database.SqlContext

module DeliveryDeliverable =
    [<Literal>]
    let TableName = "Chauffeur_Delivery";
    let tokenRegex = Regex(@"\$(\w+)\$", RegexOptions.Compiled)

    // TODO: Can this be a record type rather than a class?
    [<TableNameAttribute(TableName)>]
    [<PrimaryKey("Id")>]
    [<AllowNullLiteral>]
    type ChauffeurDeliveryTable() =
        [<Column("Id")>]
        [<PrimaryKeyColumn(Name = "PK_id", IdentitySeed = 1)>]
        member val Id = 0 with get, set

        [<Column("Name")>]
        member val Name = "" with get, set

        [<Column("ExecutionDate")>]
        member val ExecutionDate = DateTime.MinValue with get, set

        [<Column("SignedFor")>]
        member val SignedFor = false with get, set

        [<Column("Hash")>]
        member val Hash = "" with get, set

    let setupDatabase logger (writer : TextWriter) db =
        task {
            do! writer.WriteLineAsync "Chauffeur Delivery does not have its database setup. Setting up now."
            let mctx = ChauffeurMockMigrationContext(db, logger)
            let builder = CreateBuilder mctx

            builder.Table<ChauffeurDeliveryTable>().Do()

            do! writer.WriteLineAsync "Successfully created database table."
            return ignore()
        }

    let findStopDeliverable (args : string[]) =
        args
        |> Array.tryFind(fun arg -> arg.StartsWith("-s:"))
        |> Option.map (fun arg -> arg.Replace("-s:", ""))

    let getFiles (writer : TextWriter) (fileSystem : IFileSystem) dir = task {
        return! match fileSystem.Directory.GetFiles(dir, "*.delivery", SearchOption.TopDirectoryOnly) with
                | arr when arr |> Array.length = 0 -> task {
                    do! writer.WriteLineAsync "No deliveries found."
                    return None }
                | arr -> task {
                    return arr
                           |> Array.map fileSystem.FileInfo.FromFileName
                           |> Array.toList
                           |> Some} }

    let parseParamTokens (args : string []) dir (settings : IChauffeurSettings) =
        let chauffeurConsts = seq {
            yield ("ChauffeurPath", dir)
            yield ("WebsiteRoot", match settings.TryGetSiteRootDirectory() with
                                  | (true, path) -> path
                                  | _ -> "")
            yield ("UmbracoPath", match settings.TryGetUmbracoDirectory() with
                                  | (true, path) -> path
                                  | _ -> "")
            yield ("UmbracoVersion", settings.UmbracoVersion) }

        args
        |> Array.filter (fun arg -> arg.StartsWith("-p:"))
        |> Array.map (fun arg -> arg.Replace("-p:", ""))
        |> Array.map (fun arg -> arg.Split '=')
        |> Array.map (fun s -> (s.[0], s.[1]))
        |> Array.toSeq
        |> Seq.append chauffeurConsts
        |> dict

    let getPendingDeliveries (writer : TextWriter) (db : IUmbracoDatabase) (files : IFileInfo list) = task {
        let! entries = files
                        |> List.map (fun file ->
                            db.Query<ChauffeurDeliveryTable>().Where(fun (t : ChauffeurDeliveryTable) -> t.Name = file.Name).FirstOrDefaultAsync())
                        |> Task.WhenAll

        let! _ = entries
                 |> Array.filter (fun e -> e <> null)
                 |> Array.filter (fun e -> e.SignedFor)
                 |> Array.map (fun e -> writer.WriteLineAsync(sprintf "'%s' is already signed for, skipping it" e.Name))
                 |> Task.WhenAll

        return files
               |> List.filter (fun file -> match entries |> Array.tryFind (fun e -> e <> null && e.Name = file.Name) with
                                            | Some e -> e.SignedFor = false
                                            | None -> true) }

    let getDeliveryContent (file : IFileInfo) =
        (file,
         file.FileSystem.File.ReadAllLines(file.FullName)
         |> Array.filter (fun s -> String.IsNullOrEmpty(s) <> true)
         |> Array.filter (fun s -> s.StartsWith("##") <> true))

    open System.Security.Cryptography

    let hash (file : IFileInfo) =
        use fs = file.OpenRead()
        use bs = new BufferedStream(fs)
        use sha1 = new SHA1Managed()
        let hash = sha1.ComputeHash bs
        let formatted = StringBuilder(2 * hash.Length)
        hash |> Array.iter (fun b -> formatted.AppendFormat("{0:X2}", b) |> ignore)
        formatted.ToString()

    let areAllParamsSupplied (writer : TextWriter) content (p : IDictionary<string, string>) =
        let getInstr s =
            tokenRegex.Matches s
            |> Seq.cast<Match>
            |> Seq.map (fun m -> m.Groups.[1].Value)

        let pp = content
                 |> Array.map getInstr
                 |> Array.collect (fun a -> a |> Seq.toArray)
                 |> Array.distinct
                 |> Array.sortBy (fun s -> s)
                 |> Array.toSeq

        let missing = pp
                     |> Seq.except p.Keys

        match missing |> Seq.length with
        | 0 -> task { return true }
        | _ -> task {
            do! writer.WriteLineAsync "The following parameters have not been specified:"
            let! _ = missing
                     |> Seq.map (fun m -> writer.WriteLineAsync(sprintf " - %s" m))
                     |> Task.WhenAll
            return false }
            

    let replaceTokens (p : IDictionary<string, string>) instruction =
        tokenRegex.Replace(instruction, fun (m : Match) -> p.[m.Groups.[1].Value])

    let makeTracker (file : IFileInfo) =
        let tracker = ChauffeurDeliveryTable()
        tracker.Name <- file.Name
        tracker.ExecutionDate <- DateTime.Now
        tracker.Hash <- hash file
        tracker.SignedFor <- true
        tracker

    let deliver areAllParamsSupplied' p (host : IChauffeurHost) = fun (tracker : ChauffeurDeliveryTable, content) -> task {
        let! allParamsSuppled = areAllParamsSupplied' content p
        if allParamsSuppled <> true then
            tracker.SignedFor <- false
            return tracker
        else
            let rec run instructions =
                match instructions with
                | head :: rest ->
                    let result = [| replaceTokens p head |] |> host.RunWithArgs
                    result.Wait()

                    if result.Result <> DeliverableResponse.Continue then
                        tracker.SignedFor <- false
                        tracker
                    else run rest
                | [] ->
                    tracker
            return content |> Array.toList |> run }

open DeliveryDeliverable

[<DeliverableName("delivery")>]
[<DeliverableAlias("d")>]
type DeliveryDeliverable
    (reader,
     writer,
     settings : IChauffeurSettings,
     fileSystem : IFileSystem,
     host : IChauffeurHost,
     scopeProvider : IScopeProvider,
     logger : ILogger) =
    inherit Deliverable(reader, writer)

    let areAllParamsSupplied' = areAllParamsSupplied writer

    override __.Run _ args = task {
        use scope = scopeProvider.CreateScope()
        let! dbNotReady = task {
                            try
                                let helper = DatabaseSchemaCreator(scope.Database, logger)
                                match helper.TableExists TableName with
                                | false -> do! setupDatabase logger writer scope.Database
                                | true -> ignore()
                                return false
                            with
                            | :? DbException ->
                                writer.WriteLine "There was an error checking for the database Chauffeur Delivery tracking table, most likely your connection string is invalid or your database doesn't exist."
                                writer.WriteLine "Chauffeur will attempt to run the first delivery, expecting it to call `install`."
                                return true
                            | _ ->
                                writer.WriteLine "There was an error checking for the database Chauffeur Delivery tracking table, most likely your connection string is invalid or your database doesn't exist."
                                writer.WriteLine "Chauffeur will attempt to run the first delivery, expecting it to call `install`."
                                return true }

        return! match settings.TryGetChauffeurDirectory() with
                | (false, _) -> task {
                    do! writer.WriteLineAsync "Error accessing the Chauffeur directory. Check your file system permissions"
                    return DeliverableResponse.Continue }
                | (true, dir) -> task {
                    let! files = getFiles writer fileSystem dir
                    let t = files
                             |> Option.map (fun files ->
                                match findStopDeliverable args with
                                | Some stop -> files |> List.filter (fun file -> file.Name <> stop)
                                | None -> files)
                             |> Option.map (fun files ->
                                let p = parseParamTokens args dir settings
                                (files, p))
                             |> Option.map (fun (files, p) ->
                                match (dbNotReady, files) with
                                | (true, first::rest) -> task {
                                    return try
                                            let (_, content) = getDeliveryContent first
                                            let tracking = (makeTracker first, content)
                                                            |> deliver areAllParamsSupplied' p host

                                            tracking.Wait()
                                            let trackingResult = tracking.Result

                                            match trackingResult.SignedFor with
                                            | false -> DeliverableResponse.FinishedWithError
                                            | true ->
                                                let setupDatabaseTask = setupDatabase logger writer scope.Database
                                                setupDatabaseTask.Wait()

                                                scope.Database.Insert(trackingResult) |> ignore

                                                let filesAndContent = rest |> List.map getDeliveryContent
                                                let tracking = filesAndContent
                                                               |> List.map(fun (file, content) ->
                                                                    let tracker = makeTracker file
                                                                    (tracker, content))
                                                               |> List.map (deliver areAllParamsSupplied' p host)

                                                let _ = tracking |> List.map scope.Database.Insert

                                                DeliverableResponse.Continue
                                            with
                                            | :? DbException ->
                                                writer.WriteLine "Ok, I tried. Chauffeur had a database error, either a missing connection string or the DB couldn't be setup."
                                                DeliverableResponse.FinishedWithError }
                                | (false, files) -> task {
                                        let! filesToProcess = getPendingDeliveries writer scope.Database files
                                        let filesAndContent = filesToProcess |> List.map getDeliveryContent
                                        let tracking = filesAndContent
                                                       |> List.map(fun (file, content) ->
                                                            let tracker = makeTracker file
                                                            (tracker, content))
                                                       |> List.map (deliver areAllParamsSupplied' p host)

                                        let! _ = tracking
                                                 |> List.map scope.Database.InsertAsync
                                                 |> Task.WhenAll

                                        return DeliverableResponse.Continue }
                                | (_, []) -> task { return DeliverableResponse.Continue })

                    return! match t with
                            | Some t -> task {
                                let! result = t
                                return result
                             }
                            | None -> task { return DeliverableResponse.Continue } }
    }
