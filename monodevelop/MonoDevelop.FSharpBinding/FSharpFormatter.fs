namespace MonoDevelop.FSharp.Formatting

open System
open System.Diagnostics
open MonoDevelop.Ide.Gui.Content
open MonoDevelop.Ide
open MonoDevelop.Projects.Policies
open MonoDevelop.Ide.CodeFormatting
open Mono.TextEditor
open Fantomas
open Fantomas.FormatConfig
open Microsoft.FSharp.Compiler

#if DEBUG
type Debug = Console
#endif

type FormattingOption =
    | Document
    | Selection of int * int

type FSharpFormatter() = 
    inherit AbstractAdvancedFormatter()
    let offsetToPos (positions : _ []) offset =
        let rec searchPos start finish = 
            if start >= finish then None
            elif start + 1 = finish then
                Some (Range.mkPos (start + 1) (offset - positions.[start]))  
            else
                let mid = (start + finish) / 2
                if offset = positions.[mid] then Some (Range.mkPos (mid + 1) 0)
                elif offset > positions.[mid] then searchPos mid finish
                else searchPos start mid
        
        searchPos 0 (positions.Length - 1)

    let format isFsiFile (textStylePolicy : TextStylePolicy) (formattingPolicy : FSharpFormattingPolicy) input formattingOption =
        let config = 
            match textStylePolicy, formattingPolicy with
            | null, null -> 
                Debug.WriteLine("**Fantomas**: Fall back to default config")
                FormatConfig.Default
            | null, _ ->
                let format = formattingPolicy.DefaultFormat
                { FormatConfig.Default with
                    ReorderOpenDeclaration = format.ReorderOpenDeclaration
                    SpaceAfterComma = format.SpaceAfterComma
                    SpaceAfterSemicolon = format.SpaceAfterSemicolon
                    SpaceAroundDelimiter = format.SpaceAroundDelimiter
                    SpaceBeforeArgument = format.SpaceBeforeArgument
                    SpaceBeforeColon = format.SpaceBeforeColon 
                    SemicolonAtEndOfLine = format.SemicolonAtEndOfLine }
            | _, null ->
                { FormatConfig.Default with
                    PageWidth = textStylePolicy.FileWidth
                    IndentSpaceNum = textStylePolicy.IndentWidth }
            | _ ->
                let format = formattingPolicy.DefaultFormat
                { FormatConfig.Default with
                    PageWidth = textStylePolicy.FileWidth
                    IndentSpaceNum = textStylePolicy.IndentWidth
                    ReorderOpenDeclaration = format.ReorderOpenDeclaration
                    SpaceAfterComma = format.SpaceAfterComma
                    SpaceAfterSemicolon = format.SpaceAfterSemicolon
                    SpaceAroundDelimiter = format.SpaceAroundDelimiter
                    SpaceBeforeArgument = format.SpaceBeforeArgument
                    SpaceBeforeColon = format.SpaceBeforeColon 
                    SemicolonAtEndOfLine = format.SemicolonAtEndOfLine }
        Debug.WriteLine("**Fantomas**: Read config - \n{0}", sprintf "%A" config)
        match formattingOption with
        | Document -> 
            try 
                CodeFormatter.formatSourceString isFsiFile input config
            with :? FormatException as ex ->
                Debug.WriteLine("Error occurs: {0}", ex.Message)
                input

        | Selection(fromOffset, toOffset) ->
            // Convert from offsets to line and column position
            let positions = 
                input.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n')
                |> Seq.map (fun s -> String.length s + 1)
                |> Seq.scan (+) 0
                |> Seq.toArray
            let fromOffset = max 0 fromOffset
            let toOffset = min input.Length toOffset
            Debug.WriteLine("**Fantomas**: Offsets from {0} to {1}", fromOffset, toOffset)
            let startPos = offsetToPos positions fromOffset
            let endPos = offsetToPos positions toOffset
            Diagnostics.Debug.Assert(startPos.IsSome && endPos.IsSome, "Offsets are within valid ranges.")
            let r = Range.mkRange "/tmp.fsx" startPos.Value endPos.Value
            Debug.WriteLine("**Fantomas**: Try to format range {0}.", r)
            let output =
                try 
                    CodeFormatter.formatSourceString isFsiFile input config
                with :? FormatException as ex ->
                    Debug.WriteLine("Error occurs: {0}", ex.Message)
                    input
            // Since Fantomas expands the range for valid F# code, we have to get 
            // handle of the document and replace the whole text
            let doc = IdeApp.Workbench.ActiveDocument
            if doc <> null && doc.Editor <> null then
                doc.Editor.Replace(0, doc.Editor.Length, output) |> ignore
                null
            else
                Debug.WriteLine("**Fantomas**: Can't access active document.")
                null

    let formatText (policyParent : PolicyContainer) (mimeTypeInheritanceChain : string seq) (input : string) formattingOption =
        let isFsiFile = 
            let doc = IdeApp.Workbench.ActiveDocument
            if doc = null then false 
            else 
                doc.FileName.Extension.Equals(".fsi", StringComparison.OrdinalIgnoreCase)
        Debug.WriteLine("**Fantomas**: Is this an fsi file? {0}", isFsiFile)
        let textStylePolicy = policyParent.Get<TextStylePolicy>(mimeTypeInheritanceChain)
        let formattingPolicy = policyParent.Get<FSharpFormattingPolicy>(mimeTypeInheritanceChain)

        format isFsiFile textStylePolicy formattingPolicy input formattingOption

    static member MimeType = "text/x-fsharp"

    override __.SupportsOnTheFlyFormatting = false
    override __.SupportsCorrectingIndent = false

    override __.CorrectIndenting(policyParent : PolicyContainer, mimeTypeChain : string seq, data : TextEditorData, line : int) =
        raise <| NotSupportedException()

    override __.OnTheFlyFormat(doc : MonoDevelop.Ide.Gui.Document, startOffset : int, endOffset : int) =
        raise <| NotSupportedException()

    override __.FormatText(policyParent, mimeTypeInheritanceChain, input, fromOffset, toOffset) =
        if fromOffset = 0 && toOffset = String.length input then 
            Debug.WriteLine("**Fantomas**: Formatting document")
            formatText policyParent mimeTypeInheritanceChain input Document
        else
            Debug.WriteLine("**Fantomas**: Formatting selection")
            formatText policyParent mimeTypeInheritanceChain input (Selection(fromOffset, toOffset))

