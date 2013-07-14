namespace MonoDevelop.FSharpRefactor

open System
open MonoDevelop.Ide
open MonoDevelop.Refactoring

open MonoDevelop.FSharp.Gui
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
        let parameters = properties :?> string array
        let position = GetPosition options
        let refactorSource =
            AddArgument.Transform (position, parameters.[0], parameters.[1])
        PerformChanges (options, properties) refactorSource

    override self.Run(options) =
        let position = GetPosition options
        let source, _ = GetSourceAndFilename options
        let addArgumentIsValid name value = 
            IsValid options
                    (AddArgument.IsValid (Some position, Some name, Some value))
        let getErrorMessage name value =
            GetErrorMessage options
                            (AddArgument.GetErrorMessage (Some position, Some name, Some value))
        let itemDialog =
            new AddArgumentParametersDialog(
                self,
                options,
                new Func<String, String, bool>(addArgumentIsValid), 
                new Func<String, String, string>(getErrorMessage))
        MessageService.ShowCustomDialog(itemDialog) |> ignore
