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
		Entry[] entries;
		Func<String[], bool> validateParameters;
		Func<String[], String> getErrorMessage;

		public RefactoringParametersDialog (RefactoringOperation refactoring,
		                                    RefactoringOptions options,
		                                    String[] parameterNames,
		                                    String[] defaultValues,
		                                    Func<String[], bool> validateParameters,
		                                    Func<String[], String> getErrorMessage)
		{
			this.refactoring = refactoring;
			this.options = options;
			this.validateParameters = validateParameters;
			this.getErrorMessage = getErrorMessage;
			this.Build ();

			this.entries = new Entry[2] { entry1, entry2 };
			var parameters = new HBox[2] { hbox1, hbox2 };
			var labels = new Label[2] { label1, label2 };
			for (int i = 0; i<parameterNames.Length; i++) {
				parameters [i].Show ();
				entries [i].Text = defaultValues [i];
				labels [i].LabelProp = parameterNames [i];
			}
		}

		protected void OnOkClicked (object sender, EventArgs e)
		{
			string newName = entry1.Text;
			((Widget)this).Destroy ();
			List<Change> changes = this.refactoring.PerformChanges (options, newName);
			IProgressMonitor monitor = IdeApp.Workbench.ProgressMonitors.GetBackgroundProgressMonitor (this.Title, null);
			RefactoringService.AcceptChanges (monitor, changes);
		}

		protected void OnPreviewClicked (object sender, EventArgs e)
		{
			string newName = entry1.Text;
			((Widget)this).Destroy ();
			List<Change> changes = refactoring.PerformChanges (options, newName);
			MessageService.ShowCustomDialog (new RefactoringPreviewDialog (changes));
		}

		protected void OnEntryChanged (object sender, EventArgs e)
		{
			bool isValid = validateParameters (new String[1] { entry1.Text });
			buttonOk.Sensitive = isValid;
			buttonPreview.Sensitive = isValid;
			imageError.Visible = !isValid;
			imageValid.Visible = isValid;
			labelErrorMessage.Text = isValid ? "" : getErrorMessage (new String[1] { entry1.Text });
		}
	}
}

