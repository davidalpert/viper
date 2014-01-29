namespace VisualStudioSolutionFileParser.AST

open System 

type FileVersion = FileVersion of int * int

type FileHeading = FileHeading of string * FileVersion

type ProjectNode(projectType, name, path, guid) =
    member x.ProjectType = projectType
    member x.Name = name
    member x.Path = path
    member x.Guid = guid

    static member FromTuple(t:Guid * string * string * Guid) =
        let pt,name,path,guid = t
        new ProjectNode(pt, name, path, guid)

type SolutionProperty(name:string, value:string) =
    member x.Name = name.Trim()
    member x.Value = value.Trim()
    with
    override this.ToString() =
        sprintf "%s[%s = %s]" (this.GetType().Name) this.Name this.Value

    static member FromTuple(t:string * string) = 
        let name,value = t
        new SolutionProperty(name,value)

type LoadSequence =
    | PreSolution  = 0
    | PostSolution = 1

type GlobalSection(name:string, loadSequence:LoadSequence, properties:SolutionProperty list) =
    member x.Name = name.Trim()
    member x.LoadSequence = loadSequence
    member x.Properties = properties
    with
    override this.ToString() =
        sprintf "%s[%s - %s]" (this.GetType().Name) this.Name (this.LoadSequence.ToString())

    static member FromTuple2(t:string * LoadSequence) =
        let a,b = t
        new GlobalSection(a,b,[])
    static member FromTuple(t:string * LoadSequence * SolutionProperty list) =
        let a,b,c = t
        new GlobalSection(a,b,c)

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


