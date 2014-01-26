namespace VisualStudioSolutionFileParser.AST

type FileHeading(productName: string, majorVersion:string, minorVersion:string) =
    member x.ProductName = productName.Trim()
    member x.MajorVersion = majorVersion.Trim()
    member x.MinorVersion = minorVersion.Trim()
    with
    override this.ToString() = 
    //    sprintf @"%s[""%s"",""%s""]" (this.GetType().Name) this.Code this.Name
        sprintf "%s : %s.%s" this.ProductName this.MajorVersion this.MinorVersion

    static member FromTuple(t:string * string * string) = 
        let a,b,c = t
        new FileHeading(a,b,c)

type SolutionFile(heading:FileHeading) =
    member x.Heading = heading
    with
    override this.ToString() =
    //    sprintf @"%s [%A]" (this.GetType().Name) this.Heading
        sprintf "%A" this.Heading

    static member FromTuple(t:FileHeading) = 
        new SolutionFile(t)

