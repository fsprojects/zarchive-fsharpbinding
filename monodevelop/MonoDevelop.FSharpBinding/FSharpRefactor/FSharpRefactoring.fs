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
open Microsoft.FSharp.Compiler.Range

open FSharpRefactor.Engine.Ast
open FSharpRefactor.Engine.CodeAnalysis.RangeAnalysis
open FSharpRefactor.Engine.CodeAnalysis.ScopeAnalysis
open FSharpRefactor.Refactorings

module FSharpRefactoring =
    let IsValid (options:RefactoringOptions) (isSourceValid:string -> string -> bool) =
        let doc = options.Document
        let wholeFileChange = new TextReplaceChange()
        let filename = options.Document.FileName.ToString()
        let source = doc.GetContent<ITextFile>().Text

        options.MimeType = "text/x-fsharp"
        |> (&&) (isSourceValid source filename)

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
