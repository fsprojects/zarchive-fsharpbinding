;;; Test registration of auto modes.

(defmacro check-automode (extension)
  `(check ,(concat "uses fsharp-mode for " extension " files")
     (using-temp-file ,extension
       (should (eq major-mode 'fsharp-mode)))))

(check-automode ".fs")
(check-automode ".fsx")
(check-automode ".fsi")
(check-automode ".fsl")

;;; Test our ability to find SLN files and projects.
;;; This is tricky to test comprehensively because there is a sln at the
;;; root of this repo.

(check "should not find fsharp project if none present"
  (should-not (fsharp-mode/find-sln-or-fsproj "/bin/")))

(check "should find fsproj in test project directory"
  (should-match "Test1.fsproj"
                (fsharp-mode/find-sln-or-fsproj fs-file-dir)))

(check "should prefer sln to fsproj"
  (should-match "bar.sln"
                (fsharp-mode/find-sln-or-fsproj (concat test-dir
                                                        "FindSlnData/"))))

(check "should find closest sln"
  (should-match "foo.sln"
                (fsharp-mode/find-sln-or-fsproj (concat test-dir
                                                        "FindSlnData/"
                                                        "sln/"))))

(check "should find sln in parent dir"
  (should-match "bar.sln"
                (fsharp-mode/find-sln-or-fsproj (concat test-dir
                                                        "FindSlnData/"
                                                        "noproj/"
                                                        "test.fs"))))

;;; Test correct construction of compile commands

(check "Should use make if Makefile present"
       (should-match "make -k"
        (fsharp-mode-choose-compile-command (concat test-dir
                                                    "CompileCommandData/"
                                                    "proj/"
                                                    "test.fs"))))

(check "Should use xbuild if fsproj present"
       (should-match "\\(x\\|ms\\)build.* /nologo .*Test1.fsproj"
        (fsharp-mode-choose-compile-command (concat test-dir
                                                    "Test1/"
                                                    "Program.fs"))))

(check "Should use fsc if no fsproj present"
       (should-match "fs\\(harp\\)?c.* --nologo .*test.fs"
        (fsharp-mode-choose-compile-command (concat test-dir
                                                    "CompileCommandData/"
                                                    "noproj/"
                                                    "test.fs"))))

(check "Should quote filenames in xbuild mode"
       (should-match "\\(x\\|ms\\)build.* /nologo .*\".*Directory With Spaces/proj/test.fsproj\""
        (fsharp-mode-choose-compile-command (concat test-dir
                                                    "CompileCommandData/"
                                                    "Directory With Spaces/"
                                                    "proj/"
                                                    "empty.fs"))))

(check "Should quote filenames in fsc mode"
       (should-match "fs\\(harp\\)?c.* --nologo .*\".*Directory With Spaces/noproj/test.fs\""
        (fsharp-mode-choose-compile-command (concat test-dir
                                                    "CompileCommandData/"
                                                    "Directory With Spaces/"
                                                    "noproj/"
                                                    "test.fs"))))
(check "Should quote builder in xbuild mode"
       (let ((fsharp-build-command "/path with spaces/xbuild"))
         (should-match "\"/path with spaces/xbuild\""
                       (fsharp-mode-choose-compile-command (concat test-dir
                                                                   "CompileCommandData/"
                                                                   "Directory With Spaces/"
                                                                   "proj/"
                                                                   "test.fs")))))
