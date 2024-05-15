open System
open System.IO

let (/>) x y = y

type Result<'a> = Result<'a, string>
let bind (f:'a->Result<'b>) (x:Result<'a>) : Result<'b> =
    match x with
    | Error(msg) -> Error(msg)
    | Ok(x') -> f x'

let (>=>) (f:'a->Result<'b>) (g:'b->Result<'c>) : 'a->Result<'c> = f >> (bind g) 

// Parse expr
let private execExpr exp =
    Interpreter.Evaluate exp Map.empty /> Ok()

// Read string from file
let private readFile filename =
    try 
        Ok(File.ReadAllText filename)
    with
        | ex -> Error(ex.ToString())

let private execFile filename =
    let result = (readFile >=> Parser.ParseString >=> execExpr) filename
    match result with
    | Error(msg) -> printfn "Error: %s" msg
    | Ok(_) -> ()

[<EntryPoint>]
let main(args) =
    if (Array.length args) < 1 then printfn "Error. No file was given." 
    else execFile args[0];
    0
