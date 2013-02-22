@echo off
if %PROCESSOR_ARCHITECTURE%==x86 (
	set MSBuild="%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe"
	set MDROOT="%ProgramFiles%\MonoDevelop"
) else (
	set MSBUILD=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe
	set MDROOT="%ProgramFiles(x86)%\MonoDevelop"
)
%MSBUILD% ..\FSharp.CompilerBinding\FSharp.CompilerBinding.fsproj /p:Configuration=Release
%MSBUILD% MonoDevelop.FSharpBinding\MonoDevelop.FSharp.windows.fsproj /p:Configuration=Release
rmdir /s /q pack
mkdir pack\windows\Release
%MDROOT%\bin\mdtool.exe setup pack bin\windows\Release\FSharpBinding.windows.addin.xml -d:pack\windows\Release
%MDROOT%\bin\mdtool.exe setup install -y pack\windows\Release\MonoDevelop.FSharpBinding_3.2.12.mpack 
