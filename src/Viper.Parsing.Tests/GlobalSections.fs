namespace Viper.Parsing.Tests.GlobalSections

open NUnit.Framework
open FsUnit
open Viper.Model
open Viper.Parsing.SolutionFileParser
open System
open _Extensions

type ``When parsing an unrecongized global section``() =

    let input = @"    GlobalSection(Mal) = preSolution
                          HideSolutionNode = FALSE
                      EndGlobalSection
                 "
    let result = Run pGlobalSection input

    [<Test>] member x.
     ``should parse name``() =
        result.Name |> should equal "Mal"

    [<Test>] member x.
     ``should parse load sequence``() =
        result.LoadSequence |> should equal SectionLoadSequence.PreSolution

    [<Test>] member x.
     ``should be the expected type``() =
        result |> should be instanceOfType<UnrecognizedGlobalSection>

    [<Test>] member x.
     ``should capture content``() =
        (result :?> UnrecognizedGlobalSection).Content.Trim()
        |> should equal @"HideSolutionNode = FALSE"

type ``When parsing a SolutionProperties section``() =

    let input = @"     GlobalSection(SolutionProperties) = preSolution
                           HideSolutionNode = FALSE
                       EndGlobalSection"

    let result = Run pGlobalSection input

    [<Test>] member x.
     ``should parse name``() =
        result.Name |> should equal "SolutionProperties"

    [<Test>] member x.
     ``should parse load sequence``() =
        result.LoadSequence |> should equal SectionLoadSequence.PreSolution

    [<Test>] member x.
     ``should be the expected type``() =
        result |> should be instanceOfType<SolutionPropertiesGlobalSection>

    [<Test>] member x.
     ``should capture the properties as a bag``() =
        (result :?> SolutionPropertiesGlobalSection).["HideSolutionNode"] |> should equal "FALSE"

    [<Test>] member x.
     ``should expose HideSolutionNode as a property``() =
        (result :?> SolutionPropertiesGlobalSection).HideSolutionNode |> should equal false

type ``When parsing an empty solution file``()=

    let input = @"Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio 2012
Global
    GlobalSection(SolutionProperties) = preSolution
        HideSolutionNode = FALSE
    EndGlobalSection
EndGlobal
"
    let sln = Parse input

    [<Test>] member x.
     ``should have correct format version``() =
        sprintf "%i.%i" sln.FormatVersion.Major sln.FormatVersion.Minor |> should equal "12.0"





