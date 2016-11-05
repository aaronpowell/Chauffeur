namespace Chauffeur.Tests.Integration
open System.Diagnostics
open System.IO
open System.Threading.Tasks

type MockTextWriter() =
    inherit TextWriter()

    let mutable messages = List.empty<string>

    override x.Encoding = System.Text.Encoding.Default

    override x.WriteLineAsync value =
        Debug.WriteLine value
        messages <- value :: messages
        Task.FromResult value :> System.Threading.Tasks.Task

type MockTextReader(commands) =
    inherit TextReader()

    let mutable remainingCommands = commands

    override x.ReadLineAsync() =
        let command = List.head remainingCommands
        remainingCommands <- List.skip 1 remainingCommands
        Task.FromResult command