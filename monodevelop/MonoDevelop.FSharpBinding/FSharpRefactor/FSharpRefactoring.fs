namespace MonoDevelop.FSharpRefactor

open System
open MonoDevelop.Projects
open MonoDevelop.Components.Commands
open MonoDevelop.Refactoring
open MonoDevelop.Refactoring.Rename
open MonoDevelop.Core
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

    let makeProject options =
        let fsFiles = getFsFiles options
        let openDocuments = IdeApp.Workbench.Documents
        let getOffset (source:string) (line, col) =
            source.Split('\n')
            |> Seq.take (line-1) 
            |> Seq.fold (fun s l -> s + (String.length l)) 0
            |> (+) (col + line - 1)
        let applyIfOpen f file =
            let document = Seq.tryFind (fun (d:Document) -> d.FileName.ToString() = file) openDocuments
            Option.map f document
        let lazyTypedInfo (document:Document) =
            lazy
                let source = document.GetContent<ITextFile>().Text
                let typedInfo =
                    LanguageService.Service.GetTypedParseResult(document.FileName, source, document.Project, IdeApp.Workspace.ActiveConfiguration, timeout = 1000000)
                let getDeclarationLocation position names =
                    let result = typedInfo.GetDeclarationLocation(getOffset source position, new TextDocument(source))
                    match result with
                        | Microsoft.FSharp.Compiler.SourceCodeServices.DeclFound(line, col, filename) -> Some ((line+1, col), filename)
                        | Microsoft.FSharp.Compiler.SourceCodeServices.DeclNotFound -> None
                let iTypedInfo =
                    { new ITypedInfo with member self.GetDeclarationLocation (position, names) = getDeclarationLocation position names }
                { typedInfo = iTypedInfo
                  contents = source }
            
        let filesAndContents =
            Seq.map (applyIfOpen (fun (d:Document) -> (d.GetContent<ITextFile>().Text))) fsFiles
            |> Seq.zip fsFiles
            |> Seq.toArray
        let lazyTypedInfos = 
            Seq.map (applyIfOpen lazyTypedInfo) fsFiles |> Seq.toArray
        new Project(options.Document.FileName.ToString(), filesAndContents, Set.empty, lazyTypedInfos)
        
    let GetProject (options:RefactoringOptions) =
        if Option.isNone project
        then
            let updateProject fileEventArgs =
                project <- Some (makeProject options)

            FileService.FileChanged.Add(updateProject)
            FileService.FileCreated.Add(updateProject)
            FileService.FileRenamed.Add(updateProject)
            FileService.FileRemoved.Add(updateProject)
            updateProject ()
        
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