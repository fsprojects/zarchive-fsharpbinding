namespace MonoDevelop.FSharp.Formatting

open System
open System.Diagnostics
open MonoDevelop.Ide.Gui.Content
open MonoDevelop.Ide
open MonoDevelop.Projects.Policies
open MonoDevelop.Ide.CodeFormatting
open Fantomas
open Fantomas.FormatConfig
open Microsoft.FSharp.Compiler

type FormattingOption =
    | Document
    | Selection of int * int

type FSharpFormatter() = 
    let offsetToPos lines offset =
        let rec loop i j = 
            if i >= Array.length lines then None
            elif lines.[i] >= j then Some (Range.mkPos i j)
            else loop (i + 1) (j - lines.[i])
        loop 0 offset

    let format (textStylePolicy : TextStylePolicy) (formattingPolicy : FSharpFormattingPolicy) input formattingOption =
        let config = FormatConfig.Default
        match formattingOption with
        | Document -> 
            // TODO: Need to determine current input is F# implementation or signature file
            defaultArg (CodeFormatter.tryFormatSourceString false input config) input

        | Selection(fromOffset, toOffset) ->
            // Convert from offsets to line and column position
            let lines = 
                input.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n')
                |> Seq.map (fun s -> String.length s + 1)
                |> Seq.scan (+) 0
                |> Seq.toArray
            let fromOffset = max 0 fromOffset
            let toOffset = min input.Length toOffset
            let startPos = offsetToPos lines fromOffset
            let endPos = offsetToPos lines toOffset
            Debug.Assert(startPos.IsSome && endPos.IsSome, "Offsets are within valid ranges.")
            let r = Range.mkRange "/tmp.fsx" startPos.Value endPos.Value
            let output = defaultArg (CodeFormatter.tryFormatSelectionFromString false r input config) input
            let delta = input.Length - toOffset
            output.[fromOffset..output.Length - delta - 1]

    // We don't support on-the-fly formatting and smart indenting yet. 
    // There's no point to use IAdvanceCodeFormatter.
    interface ICodeFormatter with
        member __.FormatText(policyParent : PolicyContainer, mimeTypeInheritanceChain : string seq, input : string) =
            Debug.WriteLine("Formatting document")
            let textStylePolicy = policyParent.Get<TextStylePolicy>(mimeTypeInheritanceChain)
            let formattingPolicy = policyParent.Get<FSharpFormattingPolicy>(mimeTypeInheritanceChain)
            format textStylePolicy formattingPolicy input Document

        member __.FormatText(policyParent : PolicyContainer, mimeTypeInheritanceChain : string seq, input : string, fromOffset : int, toOffset : int) =
            Debug.WriteLine("Formatting selection")
            let textStylePolicy = policyParent.Get<TextStylePolicy>(mimeTypeInheritanceChain)
            let formattingPolicy = policyParent.Get<FSharpFormattingPolicy>(mimeTypeInheritanceChain)
            format textStylePolicy formattingPolicy input (Selection(fromOffset, toOffset))

