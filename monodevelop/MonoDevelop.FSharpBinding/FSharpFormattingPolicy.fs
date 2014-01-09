namespace MonoDevelop.FSharp.Formatting

open System

type FSharpFormattingPolicy() =
    interface IEquatable<FSharpFormattingPolicy> with
        member __.Equals(other) = false

