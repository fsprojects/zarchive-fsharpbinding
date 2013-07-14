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
		
		private String[] Parameters
		{
			get { return new String[2] { entry1.Text, entry2.Text }; }
		}

		protected void OnOkClicked (object sender, EventArgs e)
		{
			var properties = Parameters;
			((Widget)this).Destroy ();
			List<Change> changes = this.refactoring.PerformChanges (options, properties);
			IProgressMonitor monitor = IdeApp.Workbench.ProgressMonitors.GetBackgroundProgressMonitor (this.Title, null);
			RefactoringService.AcceptChanges (monitor, changes);
		}

		protected void OnPreviewClicked (object sender, EventArgs e)
		{
			var properties = Parameters;
			((Widget)this).Destroy ();
			List<Change> changes = refactoring.PerformChanges (options, properties);
			MessageService.ShowCustomDialog (new RefactoringPreviewDialog (changes));
		}

		protected void OnEntryChanged (object sender, EventArgs e)
		{
			bool isValid = validateParameters (Parameters);
			buttonOk.Sensitive = isValid;
			buttonPreview.Sensitive = isValid;
			imageError.Visible = !isValid;
			imageValid.Visible = isValid;
			labelErrorMessage.Text = isValid ? "" : getErrorMessage (Parameters);
		}
	}
}