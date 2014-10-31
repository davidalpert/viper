module Error

open System
open FParsec

exception ParseError of string * ParserError

type ParseException (message:string, context:ParserError) =
    inherit ApplicationException(message, null)
    let Context = context
