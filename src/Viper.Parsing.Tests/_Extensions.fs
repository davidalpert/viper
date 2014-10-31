module _Extensions

open System

type System.String with
    member this.NormalizeLineEndings() = this.Replace(@"\r\n","\n")
