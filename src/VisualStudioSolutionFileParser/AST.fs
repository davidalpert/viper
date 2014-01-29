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

type ProjectNode(projectType, name, path, guid) =
    member x.ProjectType = projectType
    member x.Name = name
    member x.Path = path
    member x.Guid = guid

    static member FromTuple(t:Guid * string * string * Guid) =
        let pt,name,path,guid = t
        new ProjectNode(pt, name, path, guid)

type LoadSequence =
    | PreSolution
    | PostSolution

type SectionName = string
type SectionContents = string
type GlobalSection =
    | SolutionPropertiesNode of LoadSequence * SolutionProperty list 
    | UnrecognizedGlobalSection of SectionName * LoadSequence * SectionContents option

type SolutionFile = SolutionFile of FileHeading * ProjectNode list * GlobalSection list
