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

#if DEBUG
let (<!>) (p: Parser<_,_>) label : Parser<_,_> =
    fun stream ->
        printfn "%A: Entering %s" stream.Position label
        let reply = p stream
        printfn "%A: Leaving %s (%A)" stream.Position label reply.Status
        reply
#else
let (<!>) (p: Parser<_,_>) label : Parser<_,_> =
    fun stream -> p stream
#endif
let flatten ((a,b),c) = (a,b,c)
let flatten5 ((((a,b),c),d),e) = (a,b,c,d,e)

let rec repeat n f a = 
    match n with
    | 1 -> f a
    | x -> repeat (x-1) f a

let anyStringUntil c = manySatisfy ((<>) c)
let between_ch cStart cEnd p = between (ch cStart) (ch cEnd) p
let string_between_ch cStart cEnd = between_ch cStart cEnd (anyStringUntil cEnd) //<!> "stringSurroundedBy"
let quotedString = string_between_ch '"' '"'
let hexn n = manyMinMaxSatisfy n n isHex
let guidFromTuple t =
    let (((a,b),c),d),e = t
    let s = sprintf "%s-%s-%s-%s-%s" a b c d e
    Guid.Parse(s)
let guid : Parser<Guid,State> = (hexn 8 .>> ch '-' .>>. hexn 4 .>> ch '-' .>>. hexn 4 .>> ch '-' .>>. hexn 4 .>> ch '-' .>>. hexn 12) |>> guidFromTuple //<!> "guid"
let guid_between_str sStart sEnd = between (str sStart) (str sEnd) guid

let header = str_ws "Microsoft Visual Studio Solution File, Format Version" 
let fileVersion = pipe2 (pint32 .>> pchar '.') (pint32) 
                     (fun maj min -> FileVersion(maj,min))
let productName = ws_ch_ws '#' >>. restOfLine true
let fileHeading = pipe3 header fileVersion productName (fun hd version name -> FileHeading(name, version)) <!> "fileHeading"

let projectNodeStart = skipString "Project"
let projectNodeContent = pipe4 (guid_between_str "(\"{" "}\")" .>> ws_ch_ws '=') (quotedString .>> ws_ch_ws ',') (quotedString .>> ws_ch_ws ',') (guid_between_str "\"{" "}\"") (fun a b c d -> (a,b,c,d))
let projectNodeEnd = skipString "EndProject"
let projectNode = (ws >>. projectNodeStart >>. projectNodeContent .>> ws .>> projectNodeEnd .>> ws) |>> ProjectNode.FromTuple         <!> "projectnode"
let projects = many (attempt projectNode)                                                                                             <!> "Projects"

let pPreSolution = str_ws "preSolution" |>> fun _ -> LoadSequence.PreSolution
let pPostSolution = str_ws "postSolution" |>> fun _ -> LoadSequence.PostSolution
let loadSequence = pPreSolution <|> pPostSolution                                                                                     //<!> "loadSequence"

let globalSectionStart = ws >>. skipString "GlobalSection" >>. string_between_ch '(' ')' .>>. (ws_ch_ws '=' >>. loadSequence .>> ws)  //<!> "sectionStart" 
let solutionProperty : Parser<SolutionProperty,State> = 
    (anyStringUntil '=' .>> ch_ws '=' .>>. restOfLine true) |>> SolutionProperty.FromTuple                                            //<!> "property"
let globalSectionEnd = ws >>. skipString "EndGlobalSection" .>> ws                                                                    //<!> "sectionEnd"

let globalSection =
    globalSectionStart .>>. manyTill solutionProperty globalSectionEnd |>> flatten |>> GlobalSection.FromTuple                        <!> "globalSection"
    //globalSectionStart .>> globalSectionEnd |>> GlobalSection.FromTuple2

let globalSectionsStart = skipString "Global" .>> notFollowedBy (str "Section")                                                       //<!> "Global" 
let globalSectionsEnd = skipString "EndGlobal"                                                                                        //<!> "EndGlobal"
let globalSections = globalSectionsStart >>. manyTill globalSection globalSectionsEnd                                                 <!> "Globals"

let solutionFile = (pipe3 (fileHeading) (opt projects) (opt globalSections) (fun a b c -> (a,b,c))) |>> SolutionFile.FromTuple

let parser = ws >>. solutionFile .>> ws .>> eof

exception ParseError of string * ParserError

type ParseException (message:string, context:ParserError) =
    inherit ApplicationException(message, null)
    let Context = context

let Parse str =
    match run parser str with
    | Success(result, _, _)   -> result
    | Failure(errorMsg, errorContext, _) -> raise (new ParseException(errorMsg, errorContext))

#if DEBUG
let Test p str =
    match run p str with
    | Success(result, _, _)   -> printfn "Success: %A" result
    | Failure(errorMsg, _, _) -> printfn "Failure: %s" errorMsg

let Run p str =
    match run p str with
    | Success(result, _, _)   -> result
    | Failure(errorMsg, errorContext, _) -> raise (new ParseException(errorMsg, errorContext))
#endif