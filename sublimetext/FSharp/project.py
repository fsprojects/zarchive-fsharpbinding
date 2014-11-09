import sublime
import sublime_plugin

from threading import Thread
import asyncore
import json
import os
import queue
import time

from FSharp.fsac.reader import AdHocRequest
from FSharp.fsac.reader import CompilerLocationRequest
from FSharp.fsac.reader import DeclarationsRequest
from FSharp.fsac.reader import ProjectRequest
from FSharp.fsac.reader import ParseRequest
from FSharp.fsac.reader import DataRequest
from FSharp.fsac.reader import FsacClient
from FSharp.fsac.server import start, PATH_TO_FSAC
from FSharp.sublime_plugin_lib.subprocess import GenericBinary


class Editor(object):

    def __init__(self):
        server = start()
        self.fsac = Fsac(server)
        self.compilers_path = None
        self.project_file = None
        self.fsac.send_request (CompilerLocationRequest ())

    @property
    def compiler_path(self):
        if self.compilers_path is None:
            return None
        return os.path.join (self.compilers_path, 'fsc.exe')

    @property
    def interpreter_path(self):
        if self.compilers_path is None:
            return None
        return os.path.join (self.compilers_path, 'fsi.exe')


class CompilerLocationResponse (object):
    def __init__(self, content):
        self.content = content

    @property
    def compilers_path(self):
       return self.content ['Data']


class ProjectResponse (object):
    def __init__(self, content):
        self.content = content

    @property
    def files(self):
       return self.content['Data']['Files']

    @property
    def framework(self):
       return self.content ['Data']['Framework']

    @property
    def output(self):
       return self.content ['Data'] ['Output']

    @property
    def output(self):
       return self.content ['Data'] ['References']


def process_resp(data):
    # data = json.loads (data.decode ('utf-8'))

    print ('XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX')
    print (data)
    print ('XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX')

    if data ['Kind'] == 'compilerlocation':
        r = CompilerLocationResponse (data)
        editor_context.compilers_path = r.compilers_path
        print (editor_context.interpreter_path)
        return

    if data['Kind'] == 'project':
        r = ProjectResponse(data)
        print (''.join(r.files))

    if data['Kind'] == 'parse':
        print ('parseparseparse', data)


class Fsac(object):
    def __init__(self, server, *args, **kwargs):
        super().__init__(*args, **kwargs)
        self.client = FsacClient(server, process_resp)

    def send_request(self, request):
        self.client.send_request(request)


class fs_set_project_file(sublime_plugin.WindowCommand):
    def run(self):
        try:
            fname = self.window.active_view ().file_name ()
        except AttributeError as e:
            return
        if not fname:
            return
        editor_context.fsac.send_request (ProjectRequest (fname))


class fs_parse_file(sublime_plugin.WindowCommand):
    def run(self):
        try:
            v = self.window.active_view()
            fname = self.window.active_view ().file_name ()
        except AttributeError as e:
            return
        if not fname:
            return

        content = v.substr(sublime.Region(0, v.size()))
        editor_context.fsac.send_request(ParseRequest(fname, content=content))


class fs_get_declarations(sublime_plugin.WindowCommand):
    def run(self):
        try:
            v = self.window.active_view()
            fname = self.window.active_view ().file_name ()
        except AttributeError as e:
            return
        if not fname:
            return

        editor_context.fsac.send_request(DeclarationsRequest(fname))


class _my_fsac(sublime_plugin.WindowCommand):
    def run(self):
        print ('hello world')
        self.window.show_input_panel ('', '', self.on_done, None, None)

    def on_done(self, s):
        editor_context.fsac.send_request(AdHocRequest(s))


class fs_show_options(sublime_plugin.WindowCommand):
    OPTIONS = {
        'F#: Parse Active File': 'fs_parse_file',
        'F#: Set Active File as Project': 'fs_set_project_file',
        'F#: Show Declarations': 'fs_get_declarations',
        'F#: Get Compiler Location': 'compilerlocation'
    }
    def run(self):
        self.window.show_quick_panel(
            list(sorted(fs_show_options.OPTIONS.keys())),
            self.on_done)

    def on_done(self, idx):
        if idx == -1:
            return
        key = list (sorted (fs_show_options.OPTIONS.keys ())) [idx]
        cmd = fs_show_options.OPTIONS [key]
        if cmd == 'compilerlocation':
            print ("FFFFFFFFFFFOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO")
            editor_context.fsac.send_request (CompilerLocationRequest ())
            return

        self.window.run_command (cmd)


editor_context = Editor()
