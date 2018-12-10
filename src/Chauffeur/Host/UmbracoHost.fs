namespace Chauffeur.Host

open System.IO
open LightInject
open System
open System.Linq
open FSharp.Control.Tasks.V2
open System.Diagnostics
open System.Reflection
open Umbraco.Core.Configuration

open Chauffeur

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

                match container.CanGetInstance(typeof<Deliverable>, registeredName) with
                | true ->
                    let deliverable = container.GetInstance<Deliverable>(registeredName)
                    let parts = rl.Split(' ')
                    let! runResult = deliverable.Run (parts.[0]) (parts.Skip(1).ToArray())
                    result <- runResult
                | false ->
                    do! writer.WriteLineAsync(sprintf "'%s' didn't match a known deliverable name or alias" rl)

            return result
        }

    interface IDisposable with
        member __.Dispose() =
            runtime.Terminate()
            container.Dispose()
