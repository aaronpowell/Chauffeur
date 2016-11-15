namespace Chauffeur.Tests.Integration
open System.Diagnostics
open System.IO
open System.Threading.Tasks

type MockTextWriter() =
    inherit TextWriter()

    let mutable messages = []

    member x.Messages = messages

    override x.Flush() =
        messages <- []

    override x.Encoding = System.Text.Encoding.Default

    override x.WriteLineAsync value =
        Debug.WriteLine value
        messages <- value :: messages
        Task.FromResult value :> System.Threading.Tasks.Task

type MockTextReader() =
    inherit TextReader()

    let mutable remainingCommands = []

    member x.AddCommand command =
        remainingCommands <- remainingCommands @ [command]

    override x.ReadLineAsync() =
        match remainingCommands with
        | [] -> failwith "No commands"
        | head::tail ->
            remainingCommands <- tail
            head
        |> Task.FromResult