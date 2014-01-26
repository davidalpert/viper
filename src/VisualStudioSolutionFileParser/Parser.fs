// Learn more about F# at http://fsharp.net

module Parser

open System
open FParsec
open VisualStudioSolutionFileParser.AST

let ws = spaces
let str s = pstring s
let str_ws s = str s .>> ws
let modelCode = manyChars (noneOf ":")
//let modelName = manyChars anyChar
//let heading = (modelCode .>> (pchar ':') .>>. modelName) |>> ModelHeading.FromTuple
//let heading = modelCode >> modelCode >> modelCode
//let solutionFile = (heading) |>> SolutionFile.FromTuple
let buildOne s = new SolutionFile(new FileHeading(s, "", ""))
let solutionFile = (modelCode) |>> buildOne
let parser = solutionFile

let Test p str =
    match run p str with
    | Success(result, _, _)   -> printfn "Success: %A" result
    | Failure(errorMsg, _, _) -> printfn "Failure: %s" errorMsg

exception ParseError of string * ParserError

type ParseException (message:string, context:ParserError) =
    inherit ApplicationException(message, null)
    let Context = context

let Parse str =
    match run parser str with
    | Success(result, _, _)   -> result
    | Failure(errorMsg, errorContext, _) -> raise (new ParseException(errorMsg, errorContext))