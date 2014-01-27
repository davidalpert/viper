namespace VisualStudioSolutionFileParser.AST

type FileVersion(major:int, minor:int) =
    member x.Major = major
    member x.Minor = minor

    static member FromTuple(t:int * int) = 
        let major,minor = t
        new FileVersion(major, minor)

type FileHeading(productName: string, version:FileVersion) =
    member x.ProductName = productName.Trim()
    member x.Version = version
    with
    override this.ToString() = 
    //    sprintf @"%s[""%s"",""%s""]" (this.GetType().Name) this.Code this.Name
        sprintf "%s : %i.%i" this.ProductName this.Version.Major this.Version.Minor

    static member FromTuple(t:FileVersion * string) = 
        let version, productName = t
        new FileHeading(productName,version)

type SolutionProperty(name:string, value:string) =
    member x.Name = name.Trim()
    member x.Value = value.Trim()

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

    static member FromTuple2(t:string * LoadSequence) =
        let a,b = t
        new GlobalSection(a,b,[])
    static member FromTuple(t:string * LoadSequence * SolutionProperty list) =
        let a,b,c = t
        new GlobalSection(a,b,c)

type SolutionFile(heading:FileHeading, globals:GlobalSection list) =
    member x.Heading = heading
    member x.GlobalSections = globals
    with
    override this.ToString() =
    //    sprintf @"%s [%A]" (this.GetType().Name) this.Heading
        sprintf "%A" this.Heading

    static member FromTuple(t:FileHeading*GlobalSection list option) = 
        let a,b = t
        match b with 
            | Some list -> new SolutionFile(a,list)
            | None -> new SolutionFile(a,[])

