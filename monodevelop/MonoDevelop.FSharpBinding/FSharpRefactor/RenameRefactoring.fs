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

open MonoDevelop.FSharp.Gui
open MonoDevelop.FSharpRefactor.FSharpRefactoring
open FSharpRefactor.Engine
open FSharpRefactor.Engine.Ast
open FSharpRefactor.Engine.RangeAnalysis
open FSharpRefactor.Engine.ScopeAnalysis
open FSharpRefactor.Refactorings

type RenameRefactoring() as self =
    inherit RefactoringOperation()
    do
        self.Name <- "Rename (F#)"

    override self.IsValid(options) =
        let position = GetPosition options
        IsValid options (Rename.IsValid (Some position, None))

    override self.PerformChanges(options, properties) =
        let parameters = properties :?> string array
        let position = GetPosition options
        let refactorSource =
            Rename.Transform (position, parameters.[0])
        PerformChanges (options, properties) refactorSource

    override self.Run(options) =
        let position = GetPosition options
        let project = GetProject options
        let filename = GetFilename options
        let oldName = FindIdentifierName project filename position
        let renameIsValid name = 
            IsValid options (Rename.IsValid (Some position, Some name))
        let getErrorMessage name =
            GetErrorMessage options (Rename.GetErrorMessage (Some position, Some name))
        let itemDialog =
            new RenameParametersDialog(
                self, 
                options, 
                oldName,
                new Func<string, bool>(renameIsValid), 
                new Func<string, string>(getErrorMessage))
        MessageService.ShowCustomDialog(itemDialog) |> ignore

    override self.GetMenuDescription(options) =
        self.Name