namespace Viper.Parsing

module SolutionFileParser =

    open FParsec
    open Error
    open Helpers
    open Viper.Model

    let i_to_product_version i =
        match i with
        | 2010 -> VisualStudioVersion.VS2010
        | 2012 -> VisualStudioVersion.VS2012
        | 2013 -> VisualStudioVersion.VS2013
        | _ -> VisualStudioVersion.Unrecognized

    let pFileFormat = ws_str_ws "Microsoft Visual Studio Solution File, Format Version" >>. pint32 .>> ch '.' .>>. pint32
    let pProductVersion = ws_str_ws "# Visual Studio" >>. pint32 .>> ws |>> i_to_product_version
    let pHeader = tuple2 pFileFormat pProductVersion <!> "file header"

    let string_surrounded_by openingChar closingChar = between_ch openingChar (manySatisfy ((<>) closingChar)) closingChar
    let pRoundBracketedString = string_surrounded_by '(' ')' <!> "round bracketed string"
    let pSectionLoadSequence = restOfLine false |>> (fun s ->
                                    match s.Trim() with
                                    | "preSolution" -> (SectionLoadSequence.PreSolution,"preSolution")
                                    | "postSolution" -> (SectionLoadSequence.PostSolution,"postSolution")
                                    | s -> (SectionLoadSequence.Unrecognized,s)) <!> "load sequence"

    let text_between_str openString closeString =
        ws_str_ws openString >>. manyCharsTill anyChar (ws_str_ws closeString) .>> ws_str_ws closeString

    // parse START(type) = loadSequence
    // switch on type -> parse contents
    // parse END
    let pUnrecognizedGlobalSection sectionType loadSequence closingTag = manyCharsTill anyChar (str closingTag) |>> (fun content -> new UnrecognizedGlobalSection(sectionType, content, loadSequence) ) <!> "UnrecognizedGlobalSection"

    let pGlobalSection : Parser<SolutionGlobalSection,unit> =
        let sectionHeader = pRoundBracketedString .>> ws_ch_ws '=' .>>. pSectionLoadSequence .>> ws
        fun stream ->
            let state = stream.State

            let headerReply = (ws_str_ws "GlobalSection" >>. sectionHeader) stream

            if headerReply.Status <> Ok then
                let msg = expected (sprintf "a global section")
                Reply(Error, msg)
            else
                let (sectionName, (parsedLoadSequence, actualLoadSequence)) = headerReply.Result
                let sectionParser = match sectionName with
                                    | _ -> pUnrecognizedGlobalSection
                let sectionReply = sectionParser sectionName parsedLoadSequence "EndGlobalSection" stream
                if sectionReply.Status <> Ok then
                    Reply(Error, sectionReply.Error)
                else
                    let section = sectionReply.Result
                    Reply(section :> SolutionGlobalSection)

    let pGlobalSections = ws_str_ws "Global" >>. many (attempt pGlobalSection) .>> ws_str_ws "EndGlobal"

    let build_object_model header globals =
        let ((major, minor), version) = header
        new SolutionFile(major, minor, version, globals)

    let private pPre2014SolutionFile = pipe2 pHeader pGlobalSections build_object_model

    let private parser = pPre2014SolutionFile .>> eof

    let private ParseAST str =
        match run parser str with
        | Success(result, _, _)   -> result
        | Failure(errorMsg, errorContext, _) -> raise (new ParseException(errorMsg, errorContext))

    let Parse str = ParseAST str

    // --------------------------------------------------------------------------------------------

    let PrettyPrint a = sprintf "%A" a

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
