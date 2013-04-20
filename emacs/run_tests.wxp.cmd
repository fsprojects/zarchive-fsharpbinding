rem command file to build fsharp emacs folder for windows-xp
setlocal
pushd test

set PATH=D:\software\gnuwin32\bin;%PATH%
set PATH=D:\software\emacs-24.2\bin;%PATH%
set PATH=C:\Program Files\FSharp-2.0.0.0\v4.0\bin;%PATH%
set PATH=C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319;%PATH%
set PATH=%~dp0..\bin;%PATH%

emacs.exe -Q --batch -l test-common.el -l fsharp-doc-tests.el -l fsharp-mode-completion-tests.el -l fsharp-mode-tests.el -f run-fsharp-tests
rem emacs.exe -Q --batch -l test-common.el -l integration-tests.el -f run-fsharp-tests
emacs.exe -Q -l test-common.el -l integration-tests.el -f run-fsharp-tests

popd
endlocal