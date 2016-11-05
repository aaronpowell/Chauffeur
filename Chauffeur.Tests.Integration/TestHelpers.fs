module TestHelpers
open System
open System.IO
open System.Reflection

let cwd = FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName
let dbFolder = "databases"

let setDataDirectory() =
    let now = DateTimeOffset.Now
    let ticks = now.Ticks.ToString()

    let folderForRun = Path.Combine [|cwd; dbFolder; ticks|]

    Directory.CreateDirectory folderForRun |> ignore

    AppDomain.CurrentDomain.SetData("DataDirectory", folderForRun)

    folderForRun