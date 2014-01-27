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

type SolutionFile(heading:FileHeading) =
    member x.Heading = heading
    with
    override this.ToString() =
    //    sprintf @"%s [%A]" (this.GetType().Name) this.Heading
        sprintf "%A" this.Heading

    static member FromTuple(t:FileHeading) = 
        new SolutionFile(t)

