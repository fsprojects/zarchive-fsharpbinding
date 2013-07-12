using System;
using Gtk;
using MonoDevelop.Refactoring;
using System.Collections.Generic;
using MonoDevelop.Refactoring.Rename;
using MonoDevelop.Core;
using MonoDevelop.Ide;

namespace MonoDevelop.FSharp.Gui
{
	public partial class RefactoringParametersDialog : Gtk.Dialog
	{
		RefactoringOperation refactoring;
		RefactoringOptions options;
		Func<String, bool> validateName;

		public RefactoringParametersDialog (RefactoringOperation refactoring, RefactoringOptions options, Func<String, bool> validateName)
		{
			this.refactoring = refactoring;
			this.options = options;
			this.validateName = validateName;
			this.Build ();
		}

		//FIXME: make my own properties class
		RenameRefactoring.RenameProperties Properties {
			get {
				return new RenameRefactoring.RenameProperties () {
					NewName = entry.Text,
					RenameFile = false
				};
			}
		}

		protected void OnOkClicked (object sender, EventArgs e)
		{
			((Widget)this).Destroy ();
			List<Change> changes = refactoring.PerformChanges (options, Properties);
			IProgressMonitor monitor = IdeApp.Workbench.ProgressMonitors.GetBackgroundProgressMonitor (this.Title, null);
			RefactoringService.AcceptChanges (monitor, changes);
		}

		protected void OnPreviewClicked (object sender, EventArgs e)
		{
			((Widget)this).Destroy ();
			List<Change> changes = refactoring.PerformChanges (options, Properties);
			MessageService.ShowCustomDialog (new RefactoringPreviewDialog (changes));
		}

		protected void OnEntryChanged (object sender, EventArgs e)
		{
			var isValid = validateName (entry.Text);
			buttonOk.Sensitive = isValid;
			buttonPreview.Sensitive = isValid;
		}
	}
}

