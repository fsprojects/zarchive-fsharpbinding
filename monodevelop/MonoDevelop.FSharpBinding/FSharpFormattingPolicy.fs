namespace MonoDevelop.FSharp.Formatting

open System
open System.Text
open MonoDevelop.Ide.Gui.Content
open MonoDevelop.Core
open MonoDevelop.Core.Serialization
open System.ComponentModel
open MonoDevelop.Projects.Policies

type FSharpFormattingSettings() = 
    [<ItemProperty>]
    [<LocalizedCategory ("Refactoring")>]
    [<LocalizedDisplayName ("Reorder open declarations")>]
    member val ReorderOpenDeclaration = false with get, set

    [<ItemProperty>]
    [<LocalizedCategory ("Spacing")>]
    [<LocalizedDisplayName ("Space after commas")>]
    member val SpaceAfterComma = true with get, set

    [<ItemProperty>]
    [<LocalizedCategory ("Spacing")>]
    [<LocalizedDisplayName ("Space after semicolons")>]
    member val SpaceAfterSemicolon = true with get, set

    [<ItemProperty>]
    [<LocalizedCategory ("Spacing")>]
    [<LocalizedDisplayName ("Space around delimiters")>]
    member val SpaceAroundDelimiter = true with get, set

    [<ItemProperty>]
    [<LocalizedCategory ("Spacing")>]
    [<LocalizedDisplayName ("Space before arguments")>]
    member val SpaceBeforeArgument = false with get, set

    [<ItemProperty>]
    [<LocalizedCategory ("Spacing")>]
    [<LocalizedDisplayName ("Space before colons")>]
    member val SpaceBeforeColon = true with get, set

    [<ItemProperty>]
    [<LocalizedCategory ("Syntax")>]
    [<LocalizedDisplayName ("Semicolon at End of Line")>]
    member val SemicolonEndOfLine = true with get, set

    member x.Clone() =
        x.MemberwiseClone() :?> FSharpFormattingSettings

[<PolicyType ("F# formatting")>]
type FSharpFormattingPolicy() =
    [<ItemProperty>]
    member val Formats = ResizeArray<FSharpFormattingSettings>() with get, set
                
    [<ItemProperty>]
    member val DefaultFormat = FSharpFormattingSettings() with get, set

    member x.Clone() =
        let clone = FSharpFormattingPolicy()
        clone.DefaultFormat <- x.DefaultFormat.Clone()
        for f in x.Formats do
            clone.Formats.Add (f.Clone())
        clone

    interface IEquatable<FSharpFormattingPolicy> with
        member this.Equals(other) = 
            this.DefaultFormat = other.DefaultFormat 
            && Seq.forall (fun f -> Seq.exists (fun f' -> f' = f) other.Formats) this.Formats
