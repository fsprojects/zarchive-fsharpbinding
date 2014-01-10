namespace MonoDevelop.FSharp.Formatting

open System
open Gtk
open MonoDevelop.Core
open MonoDevelop.Ide

// This is just a placeholder until proper GUI is generated

type FSharpFormattingPolicyPanelWidget() = 
    inherit Gtk.Bin()
    member __.CommitPendingChanges() = ()
    member __.SetFormat(policy : FSharpFormattingPolicy) = ()


