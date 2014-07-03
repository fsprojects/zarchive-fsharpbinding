namespace MonoDevelop.FSharp.Tests
open System
open NUnit.Framework
open MonoDevelop.FSharp
open MonoDevelop.Core
open MonoDevelop.Ide.Gui
open MonoDevelop.Ide.Gui.Content
open FSharp.CompilerBinding
open MonoDevelop.Projects
open MonoDevelop.Ide.TypeSystem
open FsUnit
open MonoDevelop.Debugger
open ICSharpCode.NRefactory.Semantics
open ICSharpCode.NRefactory.TypeSystem
open MonoDevelop.FSharp.NRefactory

[<TestFixture>]
type ResolverProviderTests() =
    inherit TestBase()
    let mutable doc = Unchecked.defaultof<Document>

    let content = """
    let a = System.String.Concat("a","b")

    let fsharpfunc a b = a b

    let ff = List.filter (fun i -> true) [1;2;3]

    """

    let createDoc (text:string)=
        let workbenchWindow = TestWorkbenchWindow()
        let viewContent = new TestViewContent()

        let project = new DotNetAssemblyProject ("F#", Name="test", FileName = FilePath("test.fsproj"))
        let projectConfig = project.AddNewConfiguration("Debug")

        TypeSystemService.LoadProject (project) |> ignore

        viewContent.Project <- project

        workbenchWindow.SetViewContent(viewContent)
        viewContent.ContentName <- "/users/a.fs"
        viewContent.GetTextEditorData().Document.MimeType <- "text/x-fsharp"
        let doc = Document(workbenchWindow)

        (viewContent :> IEditableTextBuffer).Text <- text
        (viewContent:> IEditableTextBuffer).CursorPosition <- 0

        let pfile = doc.Project.AddFile("/users/a.fs")

        let resolverProvider = new FSharpResolverProvider()
        viewContent.Contents.Add(resolverProvider)

        try doc.UpdateParseDocument() |> ignore
        with exn -> Diagnostics.Debug.WriteLine(exn.ToString())
        doc

    let getBasicOffset expr =
        let startOffset = content.IndexOf (expr, StringComparison.Ordinal)
        startOffset + (expr.Length / 2)

    let resolveExpression (doc:Document, content:string, offset:int) =
        let resolver =
            doc.GetContents<obj>()
            |> Seq.cast<ITextEditorResolverProvider> 
            |> Seq.tryHead
        
        match resolver with
        | Some resolver -> 
            let region = ref Unchecked.defaultof<DomRegion>
            let result = resolver.GetLanguageItem(doc, offset, region)
            result
        | None -> failwith "No debug resolver found"

    let (|Typ|_|) (r: ResolveResult) =
      match r with 
      | :? TypeResolveResult as t -> 
         Some t.Type
      | _ -> None 

    let (|FSharpType|_|) (r: IType) =
      match r with 
      | :? FSharpResolvedTypeDefinition as t -> 
         Some t
      | _ -> None 

    let (|Member|_|) (r: ResolveResult) =
      match r with 
      | :? MemberResolveResult as m -> 
         Some m.Member
      | _ -> None 

    let (|FSharpMethod|_|) (r: IMember) =
      match r with 
      | :? FSharpResolvedMethod as m -> 
         Some m
      | _ -> None 

    [<TestFixtureSetUp>]
    override x.Setup() =
        base.Setup()
        doc <- createDoc(content)

    [<Test>]
    member x.``String.Concat method should return correct overload``() =
        let basicOffset = getBasicOffset (".Concat")
        match resolveExpression (doc, content, basicOffset) with
        | Member (FSharpMethod m) -> 
            m.Name |> should equal "Concat" 
            m.Parameters.Count |> should equal 2
            m.Parameters.[0].Name |> should equal "str0"
            m.Parameters.[0].Type.FullName |> should equal "System.String"
            m.Parameters.[1].Name |> should equal "str1"
            m.Parameters.[1].Type.FullName |> should equal "System.String"
        | _ -> Assert.Fail "Not a method result"

    [<Test>]
    member x.``Local F# function should be found``() =
        let basicOffset = getBasicOffset ("fsharpfunc")
        match resolveExpression (doc, content, basicOffset) with
        | Member (FSharpMethod m) -> 
            m.Name |> should equal "fsharpfunc" 
            m.Parameters.Count |> should equal 2
//            m.Parameters.[0].Name |> should equal "str0"
//            m.Parameters.[0].Type.FullName |> should equal "Func<'b,c'>"
//            m.Parameters.[1].Name |> should equal "str1"
//            m.Parameters.[1].Type.FullName |> should equal "System.String"
        | _ -> Assert.Fail "Not a method result"

    //[<Test>]
    //List.filter can currently not be found. I'm not sure what is expected yet.
    member x.``List filter should be found``() =
        let basicOffset = getBasicOffset ("filter")
        match resolveExpression (doc, content, basicOffset) with
        | Member (FSharpMethod m) -> 
            m.Name |> should equal "filter" 
            m.Parameters.Count |> should equal 2
//            m.Parameters.[0].Name |> should equal "str0"
//            m.Parameters.[0].Type.FullName |> should equal "Func<'b,c'>"
//            m.Parameters.[1].Name |> should equal "str1"
//            m.Parameters.[1].Type.FullName |> should equal "System.String"
        | _ -> Assert.Fail "Not a method result"

    [<Test>]
    member x.``System.String should yield a type``() =
        let basicOffset = getBasicOffset (".String")
        match resolveExpression (doc, content, basicOffset) with
        | Typ (FSharpType typ) -> 
            typ.FullName |> should equal "System.String" 
            typ.ParentAssembly.AssemblyName |> should equal "mscorlib"
            typ.TypeArguments.Count |> should equal 0
        | _ -> Assert.Fail "Not a method result"

   

