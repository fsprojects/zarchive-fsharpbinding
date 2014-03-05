## FSharp - F# Support for Sublime Text

This package provides support for F# development in Sublime Text.

FSharp is currently a preview and not ready for use. If you want to
contribute to its development, you can read on to learn how to set up a
development environment.


### Developing FSharp

Pull requests to FSharp are welcome.

At the moment, FSharp is only compatible with Sublime Text 3 on Windows.

See also *FSharp_Tests/README.md*.

General steps:

* Clone this repository to any folder outside of Sublime Text's *Data* folder
* Edit files as needed
* Edit tests in FSharp_Tests as needed
* Build *FSharp.sublime-package*
* Publish to *Installed Packages*
* Publish FSharp_Tests to *Packages/FSharp_Tests*
* Restart Sublime Text
* Run the tests

There are scripts to build *FSharp.sublime-package* automatically on Linux,
OS X and Windows, but they may not work on your computer at present.

The file *manifest.json* should contain all the files that need to be
included in *FSharp.sublime-package*.

## Windows development environment

You must set `$STDataPath` in your PowerShell session to Sublime Text's *Data*
path.
