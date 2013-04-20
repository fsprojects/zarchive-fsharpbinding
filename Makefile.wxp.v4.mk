# Makefile for compiling and installing F# AutoComplete engine for windows-xp

# Here are a few paths that need to be configured first:
CORPATH = C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319
FSBINF = C:/Program Files/FSharp-2.0.0.0/v4.0/bin
FSBINB = C:\Program Files\FSharp-2.0.0.0\v4.0\bin
FSC = "$(FSBINB)\fsc.exe"

FILES_FCB = \
	FSharp.CompilerBinding/Common.fs \
	FSharp.CompilerBinding/CompilerLocationUtils.fs \
	FSharp.CompilerBinding/FSharpCompiler.fs

FILES_FAC = \
	FSharp.AutoComplete/Debug.fs \
	FSharp.AutoComplete/Options.fs \
	FSharp.AutoComplete/ProjectParser.fs \
	FSharp.AutoComplete/Parser.fs \
	FSharp.AutoComplete/TipFormatter.fs \
	FSharp.AutoComplete/Program.fs

REFERENCES = \
	-r:System.dll \
	-r:Microsoft.Build.dll \
	-r:Microsoft.Build.Engine.dll \
	-r:Microsoft.Build.Framework.dll \
	-r:Microsoft.Build.Tasks.v4.0.dll \
	-r:Microsoft.Build.Utilities.v4.0.dll \
	-r:lib/ndesk-options/NDesk.Options.dll \
	-r:lib/newtonsoft.json-2.0/Newtonsoft.Json.dll \

config=Release

FSC_OPTS = --noframework --nologo

ifeq ($(config),Debug)
FSC_OPTS += --debug --optimize-
else
FSC_OPTS += --debug:pdbonly --optimize
endif

TARGETS = bin/FSharp.Core.dll bin/FSharp.CompilerBinding.dll bin/fsautocomplete.exe

all: $(TARGETS)

bin/fsautocomplete.exe: $(FILES_FAC) bin/FSharp.CompilerBinding.dll
#	@-mkdir -p bin
	$(FSC) $(FSC_OPTS) --out:bin/fsautocomplete.exe -r:bin/FSharp.CompilerBinding.dll $(REFERENCES) $(FILES_FAC) 

bin/FSharp.CompilerBinding.dll: $(FILES_FCB)
#	@-mkdir -p bin
	$(FSC) $(FSC_OPTS) --target:library --out:bin/FSharp.CompilerBinding.dll $(REFERENCES) $(FILES_FCB)

bin/FSharp.Core.dll:
#	@-mkdir -p  bin
	cp -p "$(FSBINF)/FSharp.Core.dll" bin/FSharp.Core.dll
	cp -p lib/ndesk-options/NDesk.Options.dll bin/
	cp -p lib/newtonsoft.json-2.0/Newtonsoft.Json.dll bin/

clean:
	-rm -fr bin

autocomplete: bin/fsautocomplete.exe
