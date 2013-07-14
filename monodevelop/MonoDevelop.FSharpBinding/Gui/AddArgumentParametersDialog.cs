using System;
using MonoDevelop.Refactoring;

namespace MonoDevelop.FSharp.Gui
{
	public class AddArgumentParametersDialog : RefactoringParametersDialog
	{
		public AddArgumentParametersDialog (RefactoringOperation refactoring,
		                                    RefactoringOptions options,
		                                    Func<String, String, bool> validateNameAndValue,
		                                    Func<String, String, String> getErrorMessage)
			: base(refactoring,
			       options,
			       new String[2] { "Argument na_me:", "Default value:" },
			       new String[2] { "argumentName", "value" },
			       (parameters) => validateNameAndValue(parameters[0], parameters[1]),
			       (parameters) => getErrorMessage(parameters[0], parameters[1]))

		{
		}
	}
}

