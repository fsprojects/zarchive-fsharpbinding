namespace MonoDevelop.FSharpRefactor

open System
open Mono.TextEditor
open MonoDevelop.Ide
open MonoDevelop.Refactoring
open MonoDevelop.Projects.Text

open MonoDevelop.FSharp.Gui
open MonoDevelop.FSharpRefactor.FSharpRefactoring
open FSharpRefactor.Engine.Ast
open FSharpRefactor.Engine.RangeAnalysis
open FSharpRefactor.Refactorings

type ExtractFunctionRefactoring() as self =
    inherit RefactoringOperation()

    do
        self.Name <- "Extract expression into a function"

    override self.IsValid options =
        let range = GetSelectionRange options
        IsValid options
                (ExtractFunction.IsValid (Some range, None))

    override self.PerformChanges (options, properties) =
        let parameters = properties :?> string array
        let range = GetSelectionRange options
        let refactorSource =
            ExtractFunction.Transform (range, parameters.[0])
        PerformChanges (options, properties) refactorSource

    override self.Run(options) =
        let range = GetSelectionRange options
        let extractFunctionIsValid name = 
            IsValid options (ExtractFunction.IsValid (Some range, Some name))
        let getErrorMessage name =
            GetErrorMessage options (ExtractFunction.GetErrorMessage (Some range, Some name))
        let itemDialog =
            new ExtractFunctionParametersDialog(
                self, 
                options, 
                new Func<string, bool>(extractFunctionIsValid), 
                new Func<string, string>(getErrorMessage))
        MessageService.ShowCustomDialog(itemDialog) |> ignore
