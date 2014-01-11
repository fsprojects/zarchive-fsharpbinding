namespace MonoDevelop.FSharp.Formatting

open System
open Gtk
open MonoDevelop.Core
open MonoDevelop.Ide
open MonoDevelop.Components.PropertyGrid
open Mono.Unix

// Handwritten GUI, feel free to edit

[<System.ComponentModel.ToolboxItem(true)>]
type FSharpFormattingPolicyPanelWidget() =
    inherit Gtk.Bin()

    let mutable currentFormat = FSharpFormattingSettings()
    let store = new ListStore (typedefof<string>, typedefof<FSharpFormattingSettings>)
    let mutable policy = FSharpFormattingPolicy()

    member val private vbox2 : Gtk.VBox = null with get, set
    member val private hbox1 : Gtk.HBox = null with get, set
    member val private boxScopes : Gtk.VBox = null with get, set
    member val private GtkScrolledWindow : Gtk.ScrolledWindow = null with get, set
    member val private listView : Gtk.TreeView = null with get, set
    member val private hbox2 : Gtk.HBox = null with get, set 
    member val private vbox4 : Gtk.VBox = null with get, set
    member val private tableScopes : Gtk.Table = null with get, set
    member val private propertyGrid : PropertyGrid = null with get, set

    member private this.Build() =
        Stetic.Gui.Initialize(this)
        // Widget MonoDevelop.Xml.Formatting.XmlFormattingPolicyPanelWidget
        Stetic.BinContainer.Attach (this) |> ignore
        this.Name <- "MonoDevelop.FSharp.Formatting.FSharpFormattingPolicyPanelWidget"
        // Container child MonoDevelop.Xml.Formatting.XmlFormattingPolicyPanelWidget.Gtk.Container+ContainerChild
        this.vbox2 <- new Gtk.VBox ()
        this.vbox2.Name <- "vbox2"
        this.vbox2.Spacing <- 6
        // Container child vbox2.Gtk.Box+BoxChild
        this.hbox1 <- new Gtk.HBox ()
        this.hbox1.Name <- "hbox1"
        this.hbox1.Spacing <- 6
        // Container child hbox1.Gtk.Box+BoxChild
        this.boxScopes <- new Gtk.VBox ()
        this.boxScopes.Name <- "boxScopes"
        this.boxScopes.Spacing <- 6
        // Container child boxScopes.Gtk.Box+BoxChild
        this.GtkScrolledWindow <- new Gtk.ScrolledWindow ()
        this.GtkScrolledWindow.Name <- "GtkScrolledWindow"
        this.GtkScrolledWindow.ShadowType <- ShadowType.In
        // Container child GtkScrolledWindow.Gtk.Container+ContainerChild
        this.listView <- new Gtk.TreeView ()
        this.listView.CanFocus <- true
        this.listView.Name <- "listView"
        this.listView.HeadersVisible <- false
        this.GtkScrolledWindow.Add (this.listView)
        this.boxScopes.Add (this.GtkScrolledWindow)
        let w2 = this.boxScopes.[this.GtkScrolledWindow] :?> Gtk.Box.BoxChild
        w2.Position <- 0
        // Container child boxScopes.Gtk.Box+BoxChild
        this.hbox2 <- new Gtk.HBox ()
        this.hbox2.Name <- "hbox2"
        this.hbox2.Spacing <- 6

        this.boxScopes.Add (this.hbox2)
        let w5 = this.boxScopes.[this.hbox2] :?> Gtk.Box.BoxChild
        w5.Position <- 1
        w5.Expand <- false
        w5.Fill <- false
        this.hbox1.Add (this.boxScopes)
        let w6 = this.hbox1.[this.boxScopes] :?> Gtk.Box.BoxChild
        w6.Position <- 0
        w6.Expand <- false
        w6.Fill <- false

        // Container child hbox1.Gtk.Box+BoxChild
        this.vbox4 <- new Gtk.VBox ()
        this.vbox4.Name <- "vbox4"
        this.vbox4.Spacing <- 6

        // Container child vbox4.Gtk.Box+BoxChild
        this.tableScopes <- new Gtk.Table ((uint32 3), (uint32 3), false)
        this.tableScopes.Name <- "tableScopes"
        this.tableScopes.RowSpacing <- (uint32 6)
        this.tableScopes.ColumnSpacing <- (uint32 6)
        this.vbox4.Add (this.tableScopes)
        let w8 = this.vbox4.[this.tableScopes] :?> Gtk.Box.BoxChild
        w8.Position <- 1
        w8.Expand <- false
        w8.Fill <- false
        // Container child vbox4.Gtk.Box+BoxChild
        this.propertyGrid <- new PropertyGrid ()
        this.propertyGrid.Name <- "propertyGrid"
        this.propertyGrid.ShowToolbar <- false
        this.propertyGrid.ShowHelp <- false
        this.vbox4.Add (this.propertyGrid)
        let w9 = this.vbox4.[this.propertyGrid] :?> Gtk.Box.BoxChild
        w9.Position <- 2
        this.hbox1.Add (this.vbox4)
        let w10 = this.hbox1.[this.vbox4] :?> Gtk.Box.BoxChild
        w10.Position <- 1

        this.vbox2.Add (this.hbox1)
        let w11 = this.vbox2.[this.hbox1] :?> Gtk.Box.BoxChild
        w11.Position <- 0

        this.Add (this.vbox2)
        if this.Child <> null then
           this.Child.ShowAll()

    member this.FillFormat(sf) =
        match sf with
        | Some format ->
            currentFormat <- format
            this.propertyGrid.CurrentObject <- format
            this.propertyGrid.Sensitive <- true
        | None ->
            this.propertyGrid.Sensitive <- false    

    member private this.HandleListViewSelectionChanged _ =
        let it : TreeIter ref = ref Unchecked.defaultof<_>
        match this.listView.Selection.GetSelected(it) with
        | true -> 
            let format = store.GetValue(!it, 1) :?> FSharpFormattingSettings
            this.FillFormat(Some format) 
        | _ -> 
            this.FillFormat(None)

    member this.Initialize() =
        this.Build()

        this.propertyGrid.ShowToolbar <- false
        this.propertyGrid.ShadowType <- ShadowType.In

        this.listView.Model <- store
        this.listView.Selection.Changed.Add(this.HandleListViewSelectionChanged)

    member this.CommitPendingChanges() = 
        this.propertyGrid.CommitPendingChanges()

    member this.GetName(format) =
        if format = policy.DefaultFormat then
            GettextCatalog.GetString ("Default")
        else
            let i = policy.Formats.IndexOf (format) + 1
            String.Format(GettextCatalog.GetString ("Format #{0}"), i)

    member this.AppendSettings format =
        store.AppendValues (this.GetName format, format) |> ignore

    member private this.Update() =
        store.Clear()
        this.AppendSettings (policy.DefaultFormat)
        for s in policy.Formats do
            this.AppendSettings s

    member this.SetFormat(p : FSharpFormattingPolicy) =
        policy <- p
        this.Update()
        match store.GetIterFirst() with
        | true, it ->
            this.listView.Selection.SelectIter(it)
        | _ -> ()




