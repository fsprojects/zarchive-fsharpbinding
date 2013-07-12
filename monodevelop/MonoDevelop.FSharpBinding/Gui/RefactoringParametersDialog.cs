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
		Func<String, String> getErrorMessage;

		public RefactoringParametersDialog (RefactoringOperation refactoring, RefactoringOptions options, String oldName, Func<String, bool> validateName, Func<String, String> getErrorMessage)
		{
			this.refactoring = refactoring;
			this.options = options;
			this.validateName = validateName;
			this.getErrorMessage = getErrorMessage;
			this.Build ();
			entry.Text = oldName;
		}

		protected void OnOkClicked (object sender, EventArgs e)
		{
			string newName = entry.Text;
			((Widget)this).Destroy ();
			List<Change> changes = refactoring.PerformChanges (options, newName);
			IProgressMonitor monitor = IdeApp.Workbench.ProgressMonitors.GetBackgroundProgressMonitor (this.Title, null);
			RefactoringService.AcceptChanges (monitor, changes);
		}

		protected void OnPreviewClicked (object sender, EventArgs e)
		{
			string newName = entry.Text;
			((Widget)this).Destroy ();
			List<Change> changes = refactoring.PerformChanges (options, newName);
			MessageService.ShowCustomDialog (new RefactoringPreviewDialog (changes));
		}

		protected void OnEntryChanged (object sender, EventArgs e)
		{
			bool isValid = validateName (entry.Text);
			buttonOk.Sensitive = isValid;
			buttonPreview.Sensitive = isValid;
			imageError.Visible = !isValid;
			imageValid.Visible = isValid;
			labelErrorMessage.Text = isValid ? "" : getErrorMessage (entry.Text);
		}
	}
}

