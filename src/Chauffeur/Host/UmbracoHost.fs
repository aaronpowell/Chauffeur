namespace Chauffeur.Host

open LightInject
open FSharp.Control.Tasks.V2
open System
open System.Diagnostics
open System.IO
open System.Reflection
open Umbraco.Core.Configuration

open Chauffeur
open ServiceContainerExtensions

type UmbracoHost(reader : TextReader, writer : TextWriter) =
    let runtime = new ChauffeurRuntime(reader, writer)
    let container = new ServiceContainer()

    interface IChauffeurHost with
        member __.Run() = task {
            runtime.Boot(container)
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

                let registeredName = sprintf "chauffeur:%s" rl

                match container.TryGetInstance<Deliverable>(registeredName) with
                | Some deliverable ->
                    let parts = rl.Split(' ')
                    let! runResult = deliverable.Run (Array.head parts) (Array.skip 1 parts)
                    result <- runResult
                | None ->
                    do! writer.WriteLineAsync(sprintf "'%s' didn't match a known deliverable name or alias" rl)

            return result
        }

    interface IDisposable with
        member __.Dispose() =
            runtime.Terminate()
            container.Dispose()
