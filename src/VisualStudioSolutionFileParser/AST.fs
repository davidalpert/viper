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

type SolutionFile(heading:FileHeading, projectNodes:ProjectNode list, globals:GlobalSection list) =
    member x.Heading = heading
    member x.Projects = projectNodes
    member x.GlobalSections = globals
    with
    override this.ToString() =
    //    sprintf @"%s [%A]" (this.GetType().Name) this.Heading
        sprintf "%A" this.Heading

    static member FromTuple(t:FileHeading*ProjectNode list option*GlobalSection list option) = 
        let a,b,c = t
        let projects = 
            match b with 
                | Some list -> list
                | None -> []
        let globals = 
            match c with 
                | Some list -> list
                | None -> []
        new SolutionFile(a,projects,globals)


