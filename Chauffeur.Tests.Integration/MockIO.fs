namespace Chauffeur.Tests.Integration
open System.Diagnostics
open System.IO
open System.Threading.Tasks

type MockTextWriter() =
    inherit TextWriter()

    let mutable messages = List.empty<string>

    member x.Messages = messages

    override x.Flush() =
        messages <- List.empty<string>

    override x.Encoding = System.Text.Encoding.Default

    override x.WriteLineAsync value =
        Debug.WriteLine value
        messages <- value :: messages
        Task.FromResult value :> System.Threading.Tasks.Task

type MockTextReader() =
    inherit TextReader()

    let mutable remainingCommands = List.empty<string>

    member x.AddCommand command =
        remainingCommands <- command :: remainingCommands

    override x.ReadLineAsync() =
        let command = List.head remainingCommands
        remainingCommands <- List.skip 1 remainingCommands
        Task.FromResult command