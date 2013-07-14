using System;
using MonoDevelop.Refactoring;

namespace MonoDevelop.FSharp.Gui
{
	public class RenameParametersDialog : RefactoringParametersDialog
	{
		public RenameParametersDialog (RefactoringOperation refactoring,
		                               RefactoringOptions options,
		                               String oldName,
		                               Func<String, bool> validateName,
		                               Func<String, String> getErrorMessage)
			: base (refactoring,
			        options,
			        new String[1] { "New na_me:" },
					new String[1] { oldName },
					(parameters) => validateName(parameters[0]),
					(parameters) => getErrorMessage(parameters[0]))
		{
		}
	}
}

