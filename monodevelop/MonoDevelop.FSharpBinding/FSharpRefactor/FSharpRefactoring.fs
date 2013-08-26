namespace MonoDevelop.FSharpRefactor

open System
open MonoDevelop.Projects
open MonoDevelop.Components.Commands
open MonoDevelop.Refactoring
open MonoDevelop.Refactoring.Rename
open MonoDevelop.Ide
open MonoDevelop.Ide.Gui
open MonoDevelop.Ide.Gui.Content
open MonoDevelop.Projects.Text
open MonoDevelop.Core.ProgressMonitoring
open Mono.TextEditor

open FSharp.CompilerBinding
open MonoDevelop.FSharp
open Microsoft.FSharp.Compiler.SourceCodeServices

open FSharpRefactor.Engine
open FSharpRefactor.Engine.Ast
open FSharpRefactor.Engine.RangeAnalysis
open FSharpRefactor.Engine.ScopeAnalysis
open FSharpRefactor.Refactorings

module FSharpRefactoring =
    let mutable project = None
    
    let getFsFiles (options:RefactoringOptions) =
        options.Document.Project.Files.GetAll()
        |> Seq.map (fun f -> f.FilePath)
        |> Seq.filter (fun f -> f.Extension = ".fs")
        |> Seq.map (fun f -> f.ToString())

    let shouldUpdate project options =
        false //FIXME
                
    let GetProject (options:RefactoringOptions) =
        if Option.isNone project || (shouldUpdate project.Value options)
        then
            let doc = options.Document
            let fsFiles = getFsFiles options
            let filename = doc.FileName.ToString()
            let source = doc.GetContent<ITextFile>().Text
            let getOffset (line, col) =
                source.Split('\n')
                |> Seq.take (line-1) 
                |> Seq.fold (fun s l -> s + (String.length l)) 0
                |> (+) (col + line - 1)
            let lazyTypedInfo =
                lazy
                    let typedInfo =
                        LanguageService.Service.GetTypedParseResult(doc.FileName, source, doc.Project, IdeApp.Workspace.ActiveConfiguration, timeout = ServiceSettings.blockingTimeout)
                    let getDeclarationLocation position names =
                        let result = typedInfo.GetDeclarationLocation(getOffset position, new TextDocument(source))
                        match result with
                            | Microsoft.FSharp.Compiler.SourceCodeServices.DeclFound(line, col, filename) -> Some ((line+1, col), filename)
                            | Microsoft.FSharp.Compiler.SourceCodeServices.DeclNotFound -> None
                    let iTypedInfo =
                        { new ITypedInfo with member self.GetDeclarationLocation (position, names) = getDeclarationLocation position names }
                    { typedInfo = iTypedInfo
                      contents = source }
                    
            let lazyTypedInfos = 
                Seq.map (fun f -> if f = filename then Some lazyTypedInfo else None) fsFiles |> Seq.toArray
            project <- Some (new Project(filename, Seq.map (fun f -> f, if f = filename then Some source else None) fsFiles |> Seq.toArray, Set.empty, lazyTypedInfos))
        
        project.Value

    let IsValid (options:RefactoringOptions) (isSourceValid:Project -> bool) =
        (options.MimeType = "text/x-fsharp") && (isSourceValid (GetProject options))
        
    let GetErrorMessage options (getErrorMessage:Project -> string option) =
        let project = GetProject options
        match getErrorMessage project with
            | None -> ""
            | Some message -> message

    let PerformChanges(options:RefactoringOptions, properties) (refactor:Project -> Project) =
        let project = GetProject options
        let updatedProject = refactor project
        let wholeFileChange filename =
            let wholeFileChange = new TextReplaceChange()
            wholeFileChange.FileName <- filename
            wholeFileChange.Offset <- 0
            wholeFileChange.RemovedChars <- String.length (project.GetContents filename)
            wholeFileChange.InsertedText <-
                updatedProject.GetContents filename
            wholeFileChange :> Change
        Collections.Generic.List<Change>(Seq.map wholeFileChange updatedProject.UpdatedFiles)

    let GetPosition (options:RefactoringOptions) =
        options.Location.Line, options.Location.Column

    let GetSelectionRange (options:RefactoringOptions) =
        let source = options.Document.GetContent<ITextFile>().Text
        let textDocument = new TextDocument(source)
        let region = options.Document.Editor.SelectionRange.GetRegion(textDocument)
        (region.BeginLine, region.BeginColumn), (region.EndLine, region.EndColumn)