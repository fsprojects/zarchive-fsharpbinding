rem command file to build fsharp .bin folder for windows-xp
setlocal
set PATH=D:\software\gnuwin32\bin;%PATH%
rm -rf bin
mkdir bin
make -B --debug=j -f Makefile.wxp.v4.mk
endlocal