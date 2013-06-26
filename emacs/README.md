# fsharp-mode

Provides support for the F# language in Emacs. Includes the following features:

- Support for F# Interactive
- Displays type signatures and tooltips
- Provides syntax highlighting and indentation.

The following features are under development:

- Intelligent indentation
- Intellisense support.

Requires Emacs 24+.

## Installation excluding Windows XP

### Package

`fsharp-mode` is available on [MELPA](http://melpa.milkbox.net) and can
be installed using the built-in package manager.

If you're not already using MELPA, add the following to your init.el:

```lisp
;;; Initialize MELPA
(require 'package)
(add-to-list 'package-archives '("melpa" . "http://melpa.milkbox.net/packages/"))
(unless package-archive-contents (package-refresh-contents))
(package-initialize)

;;; Install fsharp-mode
(unless (package-installed-p 'fsharp-mode)
  (package-install 'fsharp-mode))

(require 'fsharp-mode)
```

### Manual installation

1. Clone this repo and run `make install`:
    ```lisp
    git clone git://github.com/fsharp/fsharpbinding.git
    cd fsharpbinding/emacs
    make install
    ```

2. Add the following to your init.el:
    ```lisp
    (add-to-list 'load-path "~/.emacs.d/fsharp-mode/")
    (autoload 'fsharp-mode "fsharp-mode"     "Major mode for editing F# code." t)
    (add-to-list 'auto-mode-alist '("\\.fs[iylx]?$" . fsharp-mode))
    ```

Note that if you do not use `make install`, which attempts to download
the dependencies from MELPA for you, then you must ensure that you have
installed them yourself. A list can be found in `fsharp-mode-pkg.el`.

If you run into any problems with installation, please check that you
have Emacs 24 on your PATH using `emacs --version`.
Note that OSX comes with Emacs 22 by default and installing a .app of
Emacs 24 will not add it to your PATH. One option is:

`alias emacs='/Applications/Emacs.app/Contents/MacOS/Emacs'`

## Usage

fsharp-mode should launch automatically whenever you open an F#
buffer. If the current file is part of an F# project, and the
intellisense process is not running, it will be launched, and
the current project loaded.

Currently intellisense features can be offered for just one project at
a time. To load a new F# project, use <kbd>C-c C-p</kbd>.

While a project is loaded, the following features will be available:

1. Type information for symbol at point will be displayed in the minibuffer
2. Errors and warnings will be automatically highlighted, with mouseover
   text.
3. To display a tooltip, move the cursor to a symbol and press
   <kbd>C-c C-t</kbd> (default).
4. To jump to the definition of a symbol at point, use <kbd>C-c C-d</kbd>.
5. Completion will be invoked automatically on dot, as in Visual Studio.
   It may be invoked manually using `completion-at-point`, often bound to
   <kbd>M-TAB</kbd> and <kbd>C-M-i</kbd>.
6. To stop the intellisense process for any reason, use <kbd>C-c C-q</kbd>.

In the event of any trouble, please open an issue on [Github](https://github.com/fsharp/fsharpbinding/) with the label `Emacs`.

## Installation on Windows XP

The following sections assume you have successfully completed the initial phase of Windows XP installation which
creates the file `bin\fsautocomplete.exe`.

### Installation of Dependencies

Certain emacs package dependencies are required for proper operation, and these are installed as follows:

- Edit your `.emacs` file to contain the following lines:

```lisp
(require 'package)
(add-to-list 'package-archives '("melpa" . "http://melpa.milkbox.net/packages/"))
(unless package-archive-contents (package-refresh-contents))
(package-initialize)

;;; Install fsharp-mode dependencies
(unless (package-installed-p 'popup) (package-install 'popup))
(unless (package-installed-p 'pos-tip) (package-install 'pos-tip))
(unless (package-installed-p 's) (package-install 's))
(unless (package-installed-p 'dash) (package-install 'dash))
(unless (package-installed-p 'auto-complete) (package-install 'auto-complete))

(require 'auto-complete)
(require 'auto-complete-config)
(ac-config-default)

``` 

- Save and restart emacs, when the packages will be downloaded and installed in compiled form under
`EMACS_HOME\.emacs.d\elpa`
- If you encounter an error message about checksums or file sizes during package installation, this is most likely
due to a false positive identification by your virus checker of the `auto-complete.tar` file as malware.
In this event, go to the [melpa website](http://melpa.milkbox.net) and attempt to download the tar
file from there, which will likely trigger a virus checker prompt which will permit you to override
its judgement. Then, if you retry the installation with the override now recorded, it is likely to succeed.
Should it continue to fail, you must then clone that project from its source site shown in the last column
of the melpa page, ensuring Unix line endings (not Windows ones), and then subsitute the following lines for the
offending one above:

```lisp
(unless (package-installed-p 'auto-complete) (package-install-file "?:/path-to/auto-complete/auto-complete.el"))
(unless (package-installed-p 'auto-complete-config) (package-install-file "?:/path-to/auto-complete/auto-complete-config.el"))
```  

- When emacs starts successfully, execute the command `M-x list-packages`, and verify it shows about half-way down
the list all the above packages as installed
- Edit your `.emacs` file in emacs, and verify that it lists `AC` as an operative minor mode for that buffer,
and similarly check that if you then type in `(se` in an empty space it shows a number of completions after a moment,
together with their documentation
- You are now free to comment out the `add-to-list` expression above, as it is not longer required once the packages
are installed, and makes editor initialisation faster by eliminating a network fetch operation 

### Testing Emacs F# Mode

Proceed to verify the Emacs F# mode as follows:

- Edit the file `emacs\run_tests.wxp.cmd` and amend the two lines showing the paths to the GnuWin32 and EmacsW32 
installation directories to reflect the installation locations chosen during the initial phase of installation
- Now exit all emacs instances
- Reselect the existing command prompt window, or create one if necessary, and ensure the working directory is
the root of this project, i.e. one level above the `emacs` folder
- In that window, invoke the command file `emacs\run_tests.wxp.cmd` 
- View the resulting output to verify that around 40 unit tests are executed successfully, and then that an
emacs instance is launched which runs around six further integration tests successfully, then exit that emacs

### Completion of emacs configuration 
   
Complete the configuration of your emacs environment as follows:

- Edit your `.emacs` file and add the following lines below the `(ac-config-default)` line shown above:
    
```lisp
(push "C:\\Program Files\\FSharp-2.0.0.0\\v4.0\\bin" exec-path)
(push "C:\\WINDOWS\\Microsoft.NET\\Framework\\v4.0.30319" exec-path)
(push "D:\\libraries\\fs\\fsharpbinding\\bin" exec-path)
(push "D:\\software\\gnuwin32\\bin" exec-path)

(add-to-list 'load-path "D:/libraries/fs/fsharpbinding/emacs")
(autoload 'fsharp-mode "fsharp-mode" "Major mode for editing F# code." t)
(add-to-list 'auto-mode-alist '("\\.fs[iylx]?$" . fsharp-mode))
(require 'fsharp-mode)

(setq inferior-fsharp-program "\"C:\\Program Files\\FSharp-2.0.0.0\\v4.0\\bin\\Fsi.exe\"")

;; (setq fsharp-ac-debug t)
;; (setq fsharp-ac-verbose t)
```

- Amend the third, fourth, and fifth code lines above to correspond to your actual installation directories
- You can leave the last two lines above commented - uncomment them if required for diagnostic purposes
- Save your `.emacs` file and restart emacs
- You should now be able to do F# development with full autocomplete capabilities
- Development of multi-source file projects could be started by cloning the `test/Test1` folder and editing the `Test1.fsproj` XML file as necessary  

## Configuration

### Compiler and REPL paths

The F# compiler and interpreter should be set to good defaults for your
OS as long as the relevant executables can be found on your PATH. If you
have a non-standard setup you may need to configure these paths manually.

On Unix-like systems, you must use the *--readline-* flag to ensure F#
Interactive will work correctly with Emacs. Typically `fsi` and `fsc` are
invoked through the shell scripts `fsharpi` and `fsharpc`.

```lisp
(setq inferior-fsharp-program "path/to/fsharpi --readline-")
(setq fsharp-compiler "path/to/fsharpc")
```

### Behavior

There are a few variables you can adjust to change how fsharp-mode behaves:

- `fsharp-ac-use-popup`: Show tooltips using a popup at the cursor
  position. If set to nil, display the tooltip in a split window.

- `fsharp-doc-idle-delay`: Set the time (in seconds) to wait before
  showing type information in the minibuffer.
  
- `fsharp-ac-intellisense-enabled`: This mode overrides some aspects of
  auto-complete configuration and runs the background process automatically.
  Set to nil to prevent this.

### Key Bindings

If you are new to Emacs, you might want to use the menu (call
menu-bar-mode if you don't see it). However, it's usually faster to learn
a few useful bindings:

- <kbd>C-c C-r</kbd>:       Evaluate region
- <kbd>C-c C-f</kbd>:       Load current buffer into toplevel
- <kbd>C-c C-e</kbd>:       Evaluate current toplevel phrase
- <kbd>C-M-x</kbd>:         Evaluate current toplevel phrase
- <kbd>C-M-h</kbd>:         Mark current toplevel phrase
- <kbd>C-c C-s</kbd>:       Show interactive buffer
- <kbd>C-c C-c</kbd>:       Compile with fsc
- <kbd>C-c x</kbd>:         Run the executable
- <kbd>C-c C-a</kbd>:       Open alternate file (.fsi or .fs)
- <kbd>C-c l</kbd>:         Shift region to left
- <kbd>C-c r</kbd>:         Shift region to right
- <kbd>C-c <up></kbd>:      Move cursor to the beginning of the block
- <kbd>C-c C-p</kbd>:       Load a project for autocompletion and tooltips
- <kbd>C-c C-d</kbd>:       Jump to definition of symbol at point
- <kbd>C-c C-t</kbd>:       Request a tooltip for symbol at point
- <kbd>C-c C-q</kbd>:       Quit current background compiler process
- <kbd>M-n</kbd>:           Go to next error
- <kbd>M-p</kbd>:           Go to previous error

To interrupt the interactive mode, use <kbd>C-c C-c</kbd>. This is useful if your
code does an infinite loop or a very long computation.

If you want to shift the region by 2 spaces, use: <kbd>M-2 C-c r</kbd>

In the interactive buffer, use <kbd>M-RET</kbd> to send the code without
explicitly adding the `;;` thing.

For key bindings that will be more familiar to users of Visual Studio, adding
the following to your `init.el` may be a good start:

```lisp
(add-hook 'fsharp-mode-hook
 (lambda ()
   (define-key fsharp-mode-map (kbd "M-RET") 'fsharp-eval-region)
   (define-key fsharp-mode-map (kbd "C-SPC") 'completion-at-point)))
```

## Contributing

This project is maintained by the
[F# Software Foundation](http://fsharp.org/), with the repository hosted
on [GitHub](https://github.com/fsharp/fsharpbinding).

Pull requests are welcome. Please run the test-suite with `make
test-all` before submitting a pull request.
