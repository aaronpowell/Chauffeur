namespace Chauffeur.Tests

open System.IO
open System.Threading.Tasks
open System.Diagnostics

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
