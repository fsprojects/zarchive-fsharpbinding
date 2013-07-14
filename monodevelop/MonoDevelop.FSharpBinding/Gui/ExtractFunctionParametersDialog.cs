using System;
using MonoDevelop.Refactoring;

namespace MonoDevelop.FSharp.Gui
{
	public class ExtractFunctionParametersDialog : RefactoringParametersDialog
	{
		public ExtractFunctionParametersDialog (RefactoringOperation refactoring,
		                                        RefactoringOptions options,
		                                        Func<String, bool> validateName,
		                                        Func<String, String> getErrorMessage)
			: base(refactoring,
			       options,
			       new String[1] { "Function na_me:" },
			       new String[1] { "functionName" },
			       (parameters) => validateName(parameters[0]),
			       (parameters) => getErrorMessage(parameters[0]))
		{
		}
	}
}

