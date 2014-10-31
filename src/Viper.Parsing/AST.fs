module AST

type Whitespace = Whitespace of string

type FileFormatMajorVersion = int
type FileFormatMinorVersion = int

type VisualStudioVersion =
| VS2010
| VS2012
| VS2013
| UnrecognizedVisualStudioVersion of string

type UnrecognizedContent = UnrecognizedContent of string
//type GlobalSection = | UnrecognizedGlobalSection of GlobalSectionName * SectionLoadSequence * UnrecognizedContent

type Token =
| GlobalSectionListStart // Global
| GlobalSectionListEnd   // EndGlobal
| GlobalSectionStart     // GlobalSection
| GlobalSectionEnd       // EndGlobalSection
| Whitespace of string
| FileFormatDeclaration of FileFormatMajorVersion * FileFormatMinorVersion
| VisualStudioVersionDeclaration of VisualStudioVersion
| LoadSequence of string

type GlobalSectionStart = GlobalSectionStart of string
type GlobalSectionEnd = GlobalSectionEnd of string
//type GlobalSectionList = GlobalSectionStart * GlobalSection list * GlobalSectionEnd

//type SolutionNode = SolutionNode of FileFormatDeclaration * VisualStudioVersion * GlobalSectionList
