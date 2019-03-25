module CommandLineParser

type ParseState = { holding: bool; buffer: string; items: string list }

let addBufferToItems state =
    {state with buffer=""; items=state.buffer :: state.items}

let parseChar state = 
    function
    | '"' when state.holding ->
        {state with holding=false} |> addBufferToItems
    | '"' ->
        {state with holding=true}
    | ' ' when not state.holding ->
        if state.buffer = "" then state
        else addBufferToItems state
    | c ->
        {state with buffer=state.buffer + (string c)}

let parseCommandLine line =
    let finalState = match (Seq.fold parseChar {holding=false; buffer=""; items = []} line) with
                        | fs when fs.buffer = "" -> fs
                        | fs -> addBufferToItems fs
    List.rev finalState.items
