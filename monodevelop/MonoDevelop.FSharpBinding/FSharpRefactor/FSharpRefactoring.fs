namespace MonoDevelop.FSharpRefactor

open System
open MonoDevelop.Components.Commands
open MonoDevelop.Refactoring
open MonoDevelop.Refactoring.Rename
open MonoDevelop.Ide
open MonoDevelop.Ide.Gui
open MonoDevelop.Ide.Gui.Content
open MonoDevelop.Projects.Text
open MonoDevelop.Core.ProgressMonitoring
open Mono.TextEditor

open FSharpRefactor.Engine.Ast
open FSharpRefactor.Engine.CodeAnalysis.RangeAnalysis
open FSharpRefactor.Engine.CodeAnalysis.ScopeAnalysis
open FSharpRefactor.Refactorings

module FSharpRefactoring =
    let GetSourceAndFilename (options:RefactoringOptions) =
        let doc = options.Document
        let filename = doc.FileName.ToString()
        let source = doc.GetContent<ITextFile>().Text
        source, filename

    let IsValid options (isSourceValid:string -> string -> bool) =
        let source, filename = GetSourceAndFilename options
        options.MimeType = "text/x-fsharp"
        |> (&&) (isSourceValid source filename)
        
    let GetErrorMessage options (getErrorMessage:string -> string -> string option) =
        let source, filename = GetSourceAndFilename options
        match getErrorMessage source filename with
            | None -> ""
            | Some message -> message


    let PerformChanges(options:RefactoringOptions, properties) (refactorSource:string -> string -> string) =
        let filename = options.Document.FileName.ToString()
        let textFile = options.Document.GetContent<ITextFile>()

        let wholeFileChange = new TextReplaceChange()
        wholeFileChange.FileName <- filename
        wholeFileChange.Offset <- 0
        wholeFileChange.RemovedChars <- textFile.Length
        wholeFileChange.InsertedText <-
            refactorSource textFile.Text filename

        Collections.Generic.List<Change>([wholeFileChange :> Change])

    let GetPosition (options:RefactoringOptions) =
        options.Location.Line, options.Location.Column

    let GetSelectionRange (options:RefactoringOptions) =
        let source = options.Document.GetContent<ITextFile>().Text
        let textDocument = new TextDocument(source)
        let region = options.Document.Editor.SelectionRange.GetRegion(textDocument)
        (region.BeginLine, region.BeginColumn), (region.EndLine, region.EndColumn)