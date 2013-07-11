namespace MonoDevelop.FSharpRefactor

open System
open Mono.TextEditor
open MonoDevelop.Refactoring
open MonoDevelop.Projects.Text

open MonoDevelop.FSharpRefactor.FSharpRefactoring
open FSharpRefactor.Engine.Ast
open FSharpRefactor.Engine.CodeAnalysis.RangeAnalysis
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
        let range = GetSelectionRange options
        let refactorSource =
            ExtractFunction.Transform (range, "testName")
        PerformChanges (options, properties) refactorSource
