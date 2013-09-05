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
    let mutable projectAndName = None
    
    let getFsFiles () =
        IdeApp.Workbench.ActiveDocument.Project.Files.GetAll()
        |> Seq.map (fun f -> f.FilePath)
        |> Seq.filter (fun f -> f.Extension = ".fs")
        |> Seq.map (fun f -> f.ToString())

    let GetFilename (options:RefactoringOptions) =
        options.Document.FileName.ToString()

    let makeProject () =
        let fsFiles = getFsFiles ()
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
            //Seq.map (fun _ -> None) fsFiles |> Seq.toArray
        new Project(filesAndContents, Set.empty, lazyTypedInfos), IdeApp.Workbench.ActiveDocument.Project.FileName
        
    let GetProject (options:RefactoringOptions) =
        let currentProjectFile = IdeApp.Workbench.ActiveDocument.Project.FileName
        let previousProjectFile = Option.map snd projectAndName
        if Option.isNone projectAndName || (currentProjectFile <> previousProjectFile.Value)
        then
            let updateProject fileEventArgs =
                projectAndName <- Some (makeProject ())

            FileService.FileChanged.Add(updateProject)
            FileService.FileCreated.Add(updateProject)
            FileService.FileRenamed.Add(updateProject)
            FileService.FileRemoved.Add(updateProject)
            updateProject ()
        
        fst projectAndName.Value

    let IsValid (options:RefactoringOptions) (isSourceValid:Project -> string -> bool) =
        let filename = GetFilename options  
        (options.MimeType = "text/x-fsharp") && (isSourceValid (GetProject options) filename)
        
    let GetErrorMessage options (getErrorMessage:Project -> string -> string option) =
        let filename = GetFilename options
        let project = GetProject options
        match getErrorMessage project filename with
            | None -> ""
            | Some message -> message

    let PerformChanges(options:RefactoringOptions, properties) (refactor:Project -> string -> Project) =
        let project = GetProject options
        let updatedProject = refactor project (GetFilename options)
        let wholeFileChange filename =
            let wholeFileChange = new TextReplaceChange()
            wholeFileChange.FileName <- filename
            wholeFileChange.Offset <- 0
            wholeFileChange.RemovedChars <- String.length (project.GetContents filename)
            wholeFileChange.InsertedText <-
                updatedProject.GetContents filename
            wholeFileChange :> Change
        Collections.Generic.List<Change>(Seq.map wholeFileChange updatedProject.UpdatedFiles)        

    let GetSelectionRange (options:RefactoringOptions) =
        let source = options.Document.GetContent<ITextFile>().Text
        let textDocument = new TextDocument(source)
        let region = options.Document.Editor.SelectionRange.GetRegion(textDocument)
        (region.BeginLine, region.BeginColumn), (region.EndLine, region.EndColumn)
        
    let GetPosition (options:RefactoringOptions) =
        let position, _ = GetSelectionRange options
        position
        