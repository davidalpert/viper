namespace VisualStudioSolutionFileParser.AST

open System 

type MajorVersion = int
type MinorVersion = int
type FileVersion = FileVersion of MajorVersion * MinorVersion

type ProductName = string
type FileHeading = FileHeading of ProductName * FileVersion 

type PropertyName = string
type PropertyValue = string
type SolutionProperty = SolutionProperty of PropertyName * PropertyValue

type ProjectType = Guid
type ProjectID = Guid
type ProjectName = string
type ProjectPath = string // relative path, as expressed in the solution file
type ProjectNode = ProjectNode of ProjectType * ProjectName * ProjectPath * ProjectID

type LoadSequence =
    | PreSolution
    | PostSolution

type SectionName = string
type SectionContents = string
type GlobalSection =
    | SolutionPropertiesNode of LoadSequence * SolutionProperty list 
    | UnrecognizedGlobalSection of SectionName * LoadSequence * SectionContents option

type SolutionFile = SolutionFile of FileHeading * ProjectNode list * GlobalSection list
