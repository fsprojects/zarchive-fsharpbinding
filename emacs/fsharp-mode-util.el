;;; fsharp-mode-util.el --- Support for F# interactive

;; Copyright (C) 1997 INRIA

;; Author: 2015 Robin Neatherway <robin.neatherway@gmail.com>
;; Maintainer: Robin Neatherway <robin.neatherway@gmail.com>
;; Keywords: languages

;; This file is not part of GNU Emacs.

;; This file is free software; you can redistribute it and/or modify
;; it under the terms of the GNU General Public License as published by
;; the Free Software Foundation; either version 3, or (at your option)
;; any later version.

;; This file is distributed in the hope that it will be useful,
;; but WITHOUT ANY WARRANTY; without even the implied warranty of
;; MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
;; GNU General Public License for more details.

;; You should have received a copy of the GNU General Public License
;; along with GNU Emacs; see the file COPYING.  If not, write to
;; the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
;; Boston, MA 02110-1301, USA.

(with-no-warnings (require 'cl))
(require 'dash)

(defvar fsharp-ac-using-mono
  (case system-type
    ((windows-nt cygwin msdos) nil)
    (otherwise t))
  "Whether the .NET runtime in use is mono. Defaults to `nil' for
  Microsoft platforms (including Cygwin), `t' for all *nix.")

(defun fsharp-mode--program-files-x86 ()
  (file-name-as-directory
   (car (-drop-while 'not
                     (list (getenv "ProgramFiles(x86)")
                           (getenv "ProgramFiles")
                           "C:\\Program Files (x86)")))))

(defun fsharp-mode--msbuild-find (exe)
  (if fsharp-ac-using-mono
      (executable-find exe)
    (let* ((searchdirs (--map (concat (fsharp-mode--program-files-x86)
                                      "MSBuild/" it "/Bin")
                              '("14.0" "13.0" "12.0")))
           (exec-path (append searchdirs exec-path)))
      (executable-find exe))))

(defun fsharp-mode--executable-find (exe)
  (if fsharp-ac-using-mono
      (executable-find exe)
    (let* ((searchdirs (--map (concat (fsharp-mode--program-files-x86)
                                      "Microsoft SDKs/F#/" it "/Framework/v4.0")
                              '("4.0" "3.1" "3.0")))
           (exec-path (append searchdirs exec-path)))
      (executable-find exe))))

(provide 'fsharp-mode-util)

;;; fsharp-mode-util.el ends here
