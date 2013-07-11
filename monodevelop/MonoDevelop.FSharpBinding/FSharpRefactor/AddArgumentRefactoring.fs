namespace MonoDevelop.FSharpRefactor

open MonoDevelop.Refactoring

open MonoDevelop.FSharpRefactor.FSharpRefactoring
open FSharpRefactor.Engine.Ast
open FSharpRefactor.Refactorings

type AddArgumentRefactoring() as self =
    inherit RefactoringOperation()

    do
        self.Name <- "Add Argument"

    override self.IsValid options =
        let position = GetPosition options
        IsValid options
                (AddArgument.IsValid (Some position, None, None))

    override self.PerformChanges (options, properties) =
        let position = GetPosition options
        let refactorSource = AddArgument.Transform (position, "testName", "value")
        PerformChanges (options, properties) refactorSource
