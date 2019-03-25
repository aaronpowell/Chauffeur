namespace Chauffeur.TestingTools
open System.IO
open System.Threading.Tasks

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