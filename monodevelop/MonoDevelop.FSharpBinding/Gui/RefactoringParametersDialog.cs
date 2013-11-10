using System;
using System.Linq;
using System.Timers;
using Gtk;
using MonoDevelop.Refactoring;
using System.Collections.Generic;
using MonoDevelop.Refactoring.Rename;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;

namespace MonoDevelop.FSharp.Gui
{
	public partial class RefactoringParametersDialog : Gtk.Dialog
	{
		RefactoringOperation refactoring;
		RefactoringOptions options;
		Entry[] entries;
		Func<String[], bool> validateParameters;
		Func<String[], String> getErrorMessage;
		private Timer timer = new Timer (500);

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
			for (int i = 0; i<parameterNames.Length; i++)
			{
				parameters [i].Show ();
				entries [i].Text = defaultValues [i];
				labels [i].LabelProp = parameterNames [i];
			}
			timer.Stop ();
			timer.Elapsed += checkValidity;
			this.DefaultResponse = ResponseType.Ok;
			entry1.SelectRegion (0, -1);
			entry1.GrabFocus ();
		}
		
		private String[] Parameters
		{
			get { return new String[2] { entry1.Text, entry2.Text }; }
		}

		private void saveChanges(List<Change> changes) {
			var changedFiles =
				(from change in changes
				 select ((TextReplaceChange)change).FileName);
			foreach (Document d in IdeApp.Workbench.Documents) {
				if (changedFiles.Contains (d.FileName.ToString ())) {
					d.Save ();
				}
			}
		}

		protected void OnOkClicked (object sender, EventArgs e)
		{
			var properties = Parameters;
			((Widget)this).Destroy ();
			List<Change> changes = this.refactoring.PerformChanges (options, properties);
			IProgressMonitor monitor = IdeApp.Workbench.ProgressMonitors.GetBackgroundProgressMonitor (this.Title, null);
			RefactoringService.AcceptChanges (monitor, changes);
			saveChanges (changes);
		}

		protected void OnPreviewClicked (object sender, EventArgs e)
		{
			var properties = Parameters;
			((Widget)this).Destroy ();
			List<Change> changes = refactoring.PerformChanges (options, properties);
			MessageService.ShowCustomDialog (new RefactoringPreviewDialog (changes));
			saveChanges (changes);
		}

		private void checkValidity (object o, ElapsedEventArgs e) {
			timer.Stop ();
			labelErrorMessage.Text = "Checking validity...";
			bool isValid = validateParameters (Parameters);
			buttonOk.Sensitive = isValid;
			buttonPreview.Sensitive = isValid;
			imageError.Visible = !isValid;
			imageValid.Visible = isValid;
			labelErrorMessage.Text = isValid ? "" : getErrorMessage (Parameters);
		}

		protected void OnEntryChanged (object sender, EventArgs e)
		{
			timer.Stop ();
			buttonOk.Sensitive = false;
			buttonPreview.Sensitive = false;
			timer.Start ();
		}
	}
}