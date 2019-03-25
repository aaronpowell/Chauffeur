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
    do
        RuntimeOptions.InstallEmptyDatabase <- true
        RuntimeOptions.InstallMissingDatabase <- true

    let runtime = new ChauffeurRuntime(reader, writer)
    let register = RegisterFactory.Create()
    let factory = runtime.Boot(register)

    let handleInput (factory : IFactory) (rl : string) =
        task {
            let deliverableResolver = factory.GetInstance<DeliverableResolver>()

            let parts = parseCommandLine rl
            match deliverableResolver.Resolve parts.[0] with
            | Some deliverable ->
                return! deliverable.Run (List.head parts) ((List.skip 1 parts) |> List.toArray)
            | None ->
                let deliverable = deliverableResolver.Resolve "unknown" |> Option.get
                return! deliverable.Run (List.head parts) ((List.skip 1 parts) |> List.toArray)
        }

    let handleInput' = handleInput factory

    interface IChauffeurHost with
        member __.Run() = task {
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
                let! r = handleInput' rl
                result <- r

            return result
        }

        member __.RunWithArgs args = handleInput' (String.concat " " args)

    interface IDisposable with
        member __.Dispose() =
            runtime.Terminate()
