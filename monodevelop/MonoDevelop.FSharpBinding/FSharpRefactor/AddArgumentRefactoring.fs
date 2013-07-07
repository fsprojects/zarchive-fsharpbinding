namespace MonoDevelop.FSharpRefactor

open MonoDevelop.Refactoring
open Microsoft.FSharp.Compiler.Range

open MonoDevelop.FSharpRefactor.FSharpRefactoring
open FSharpRefactor.Engine.Ast
open FSharpRefactor.Refactorings

type AddArgumentRefactoring() as self =
    inherit RefactoringOperation()

    let addArgument source position argumentName defaultValue =
        let tree = (Ast.Parse source).Value
        let bindingRange =
            AddArgument.DefaultBindingRange source tree position
        AddArgument.DoAddArgument source tree bindingRange.Value argumentName defaultValue

    do
        self.Name <- "Add Argument"

    override self.IsValid options =
        let position = GetPosition options
        IsValid options
                (AddArgument.IsValid (Some position, None, None))

    override self.PerformChanges (options, properties) =
        let line, col = GetPosition options
        let refactorSource =
            fun source _ ->
                addArgument source (mkPos line (col-1)) "testName" "value"
        PerformChanges (options, properties) refactorSource
