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

open MonoDevelop.FSharpRefactor.FSharpRefactoring
open FSharpRefactor.Engine.Ast
open FSharpRefactor.Engine.CodeAnalysis.RangeAnalysis
open FSharpRefactor.Engine.CodeAnalysis.ScopeAnalysis
open FSharpRefactor.Refactorings

type FSharpRenameItemDialog(options:RefactoringOptions, rename:RenameRefactoring) as self =
    inherit RenameItemDialog(options, rename)
    do
        self.Title <- "Rename (F#)"

type RenameRefactoring() as self =
    inherit MonoDevelop.Refactoring.Rename.RenameRefactoring()

    let rename (source:string) (position:pos) (newName:string) =
        let tree = (Ast.Parse source).Value
        let identifier = FindIdentifier source position
        let declarationIdentifier =
            TryFindIdentifierDeclaration (makeScopeTrees tree) identifier.Value
        Rename.DoRename source tree declarationIdentifier.Value newName

    do
        self.Name <- "Rename (F#)"

    override self.IsValid(options) =
        let position = GetPosition options
        IsValid options (Rename.IsValid (Some position, None))

    override self.PerformChanges(options, properties) =
        let renameProperties =
            properties :?> MonoDevelop.Refactoring.Rename.RenameRefactoring.RenameProperties
        let position =
            mkPos options.Location.Line (options.Location.Column-1)
        let refactorSource =
            fun source _ ->
                rename source position renameProperties.NewName
        PerformChanges (options, properties) refactorSource

    override self.Run(options) =
        let itemDialog = new FSharpRenameItemDialog(options, self)
        MessageService.ShowCustomDialog(itemDialog) |> ignore

    override self.GetMenuDescription(options) =
        self.Name