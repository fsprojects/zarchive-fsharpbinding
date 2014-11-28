if exist "%ProgramFiles(x86)%\Microsoft SDKs\F#\3.1\Framework\v4.0\fsi.exe" (
	"%ProgramFiles(x86)%\Microsoft SDKs\F#\3.1\Framework\v4.0\fsi" configure.fsx "%*"
) else (
	if exist "%ProgramFiles(x86)%\Microsoft SDKs\F#\3.0\Framework\v4.0\fsi.exe" (
		"%ProgramFiles(x86)%\Microsoft SDKs\F#\3.0\Framework\v4.0\fsi" configure.fsx "%*"
	) else (
		echo "F# not installed."
		exit
	)
)

..\lib\nuget\NuGet.exe restore MonoDevelop.FSharpBinding\MonoDevelop.FSharp.Windows.sln
