// Learn more about F# at http://fsharp.net

module Parser

open System
open FParsec
open VisualStudioSolutionFileParser.AST

// convenience type for locking down generic type inference issues
// from: http://www.quanttec.com/fparsec/tutorial.html#fs-value-restriction
type State = unit

let ws = spaces

let ch c = pchar c
let ch_ws c = ch c .>> ws
let ws_ch_ws c = ws >>. ch c .>> ws

let str s = pstring s
let str_ws s = str s .>> ws
let ws_str_ws s = ws >>. str s .>> ws

let (<!>) (p: Parser<_,_>) label : Parser<_,_> =
    fun stream ->
        printfn "%A: Entering %s" stream.Position label
        let reply = p stream
        printfn "%A: Leaving %s (%A)" stream.Position label reply.Status
        reply
//let (<!>) (p: Parser<_,_>) label : Parser<_,_> =
//    fun stream -> p stream

let anyStringUntil c = manySatisfy ((<>) c)
let stringSurroundedBy cStart cEnd : Parser<string,State> = between (ch cStart) (ch cEnd) (anyStringUntil cEnd) <!> "stringSurroundedBy"

let roundBracketedString = stringSurroundedBy '(' ')'

let fileVersion = (pint32 .>> pchar '.' .>>. pint32) |>> FileVersion.FromTuple
let header = str_ws "Microsoft Visual Studio Solution File, Format Version" >>. fileVersion .>> skipRestOfLine true
let productName = ch_ws '#' >>. restOfLine true
let fileHeading = (header .>>. productName) |>> FileHeading.FromTuple 

let pPreSolution = str_ws "preSolution" |>> fun _ -> LoadSequence.PreSolution
let pPostSolution = str_ws "postSolution" |>> fun _ -> LoadSequence.PostSolution
let loadSequence = pPreSolution <|> pPostSolution <!> "loadSequence"

let globalSectionStart = ws >>. skipString "GlobalSection" >>. stringSurroundedBy '(' ')' .>>. (ws_ch_ws '=' >>. loadSequence .>> ws) <!> "sectionStart" 
let solutionProperty : Parser<SolutionProperty,State> = 
    (anyStringUntil '=' .>> ch_ws '=' .>>. restOfLine true) |>> SolutionProperty.FromTuple                                            <!> "property"
let globalSectionEnd = ws >>. skipString "EndGlobalSection" .>> ws                                                                    <!> "sectionEnd"

let flatten ((a,b),c) = (a,b,c)
let globalSection =
    globalSectionStart .>>. manyTill solutionProperty globalSectionEnd |>> flatten |>> GlobalSection.FromTuple
    //globalSectionStart .>> globalSectionEnd |>> GlobalSection.FromTuple2

let globalSectionsStart = skipString "Global"                                                                                         <!> "globals start"
let globalSectionsEnd = skipString "EndGlobal"                                                                                        <!> "globals end"
let globalSections = globalSectionsStart >>. manyTill globalSection globalSectionsEnd                                                 <!> "globals"

let solutionFile = (fileHeading .>>. opt globalSections) |>> SolutionFile.FromTuple

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

let Run p str =
    match run p str with
    | Success(result, _, _)   -> result
    | Failure(errorMsg, errorContext, _) -> raise (new ParseException(errorMsg, errorContext))
