namespace Chauffeur.Host

open FSharp.Control.Tasks.V2
open System
open System.Diagnostics
open System.IO
open System.Reflection
open Umbraco.Core
open Umbraco.Core.Configuration
open Umbraco.Core.Composing

open Chauffeur
open Chauffeur.Components
open CommandLineParser

type UmbracoHost(reader : TextReader, writer : TextWriter) =
    let runtime = new ChauffeurRuntime(reader, writer)

    interface IChauffeurHost with
        member __.Run() = task {
            let register = RegisterFactory.Create()
            RuntimeOptions.InstallEmptyDatabase <- true
            RuntimeOptions.InstallMissingDatabase <- true

            let factory = runtime.Boot(register)
            do! writer.WriteLineAsync("Welcome to Chauffeur, your Umbraco console.")
            let fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location)
            do! writer.WriteLineAsync(sprintf "You're running Chauffeur v%s against Umbraco %A" (fvi.FileVersion) (UmbracoVersion.SemanticVersion))
            do! writer.WriteLineAsync()
            do! writer.WriteLineAsync("Type `help` to list the commands and `help <command>` for help for a specific command.")
            do! writer.WriteLineAsync()

            let mutable result = DeliverableResponse.Continue

            while result <> DeliverableResponse.Shutdown do
                do! writer.WriteAsync("umbraco> ")
                let! rl = reader.ReadLineAsync()

                let deliverableResolver = factory.GetInstance<DeliverableResolver>()

                let parts = parseCommandLine rl
                match deliverableResolver.Resolve parts.[0] with
                | Some deliverable ->
                    let! runResult = deliverable.Run (List.head parts) ((List.skip 1 parts) |> List.toArray)
                    result <- runResult
                | None ->
                    let deliverable = deliverableResolver.Resolve "unknown" |> Option.get
                    let! runResult = deliverable.Run (List.head parts) ((List.skip 1 parts) |> List.toArray)
                    result <- runResult

            return result
        }

    interface IDisposable with
        member __.Dispose() =
            runtime.Terminate()
