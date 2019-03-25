namespace Chauffeur
    open System.Collections.Generic
    open System.Threading.Tasks
    open System.IO
    open System
    open System.Reflection

    [<System.Runtime.CompilerServices.Extension>]
    module TextWriterExtensions =
        let properties (type' : Type) =
            type'.GetProperties(BindingFlags.Public ||| BindingFlags.Instance)

        let header (properties : PropertyInfo array) (columnMappings : IDictionary<string, string>) =
            properties
            |> Array.map (fun p -> match columnMappings.ContainsKey(p.Name) with
                                   | true -> columnMappings.[p.Name]
                                   | false -> p.Name)

        let data (properties : PropertyInfo array) rows =
            rows
            |> Seq.map (fun row -> properties
                                    |> Array.map (fun p -> p.GetValue(row).ToString()))
            |> Seq.toArray

        let colLengths (header : string array) (rows : string array array) =
            header
            |> Array.mapi (fun i _ -> rows
                                      |> Array.map (fun row -> row.[i].Length)
                                      |> Array.max)

        [<System.Runtime.CompilerServices.Extension>]
        let WriteTableAsync<'T> (writer : TextWriter) (rows : IEnumerable<'T>) (columnMappings : IDictionary<string, string>) =
            let props = properties typeof<'T>

            let headerRow = header props columnMappings
            let dataRows = data props rows
            let lengths = colLengths headerRow dataRows

            let allData = Array.concat [[| headerRow |]; dataRows]

            Task.WhenAll(
                allData
                |> Array.map (fun dataRow ->
                                let str = dataRow
                                          |> Array.mapi (fun i dr -> dr.PadRight(lengths.[i], ' '))
                                          |> String.concat " | "

                                writer.WriteLineAsync str)
            )
