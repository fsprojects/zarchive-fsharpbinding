namespace MonoDevelop.FSharpRefactor

open System
open Mono.TextEditor
open MonoDevelop.Refactoring
open MonoDevelop.Projects.Text
open Microsoft.FSharp.Compiler.Range

open MonoDevelop.FSharpRefactor.FSharpRefactoring
open FSharpRefactor.Engine.Ast
open FSharpRefactor.Engine.CodeAnalysis.RangeAnalysis
open FSharpRefactor.Refactorings

type ExtractFunctionRefactoring() as self =
    inherit RefactoringOperation()

    let extractFunction source ((startLine, startColumn), (endLine, endColumn)) functionName =
        let tree = (Ast.Parse source).Value
        let expressionRange = mkRange "test.fs" (mkPos startLine (startColumn-1)) (mkPos endLine (endColumn-1))
        let inScopeTree = ExtractFunction.DefaultInScopeTree tree expressionRange
        ExtractFunction.DoExtractFunction source tree (Ast.AstNode.Expression inScopeTree.Value) expressionRange functionName

    do
        self.Name <- "Extract expression into a function"

    override self.IsValid options =
        let range = GetSelectionRange options
        IsValid options
                (ExtractFunction.IsValid (Some range, None))

    override self.PerformChanges (options, properties) =
        let range = GetSelectionRange options
        let refactorSource =
            fun source _ ->
                extractFunction source range "testName"
        PerformChanges (options, properties) refactorSource
