// Learn more about F# at http://fsharp.net

module Parser

open System
open FParsec
open VisualStudioSolutionFileParser.AST

let ws = spaces

let ch c = pchar c
let ch_ws c = ch c .>> ws
let str s = pstring s
let str_ws s = str s .>> ws

let fileVersion = (pint32 .>> pchar '.' .>>. pint32) |>> FileVersion.FromTuple
let header = str_ws "Microsoft Visual Studio Solution File, Format Version" >>. fileVersion .>> skipRestOfLine true
let productName = ch_ws '#' >>. restOfLine true 
let fileHeading = (header .>>. productName) |>> FileHeading.FromTuple

let solutionFile = (fileHeading) |>> SolutionFile.FromTuple

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