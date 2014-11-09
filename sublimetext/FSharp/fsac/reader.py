import threading
import queue
import asyncore
import socket
import time
import json


from FSharp.fsac.server import requests_queue
from FSharp.fsac.server import responses_queue


class Request (object):
    def __init__(self, timeout=250, add_newline=True):
        self.add_newline = add_newline
        self.timeout = timeout

    def encode(self):
        data = str(self)
        if self.add_newline:
            data += '\n'
        return data.encode ('utf-8')


class CompilerLocationRequest(Request):
    def __init__(self, *args, **kwargs):
        super ().__init__ (*args, **kwargs)

    def __str__(self):
        return 'compilerlocation'


class ProjectRequest(Request):
    def __init__(self, project_file, *args, **kwargs):
        super ().__init__ (*args, **kwargs)
        self.project_file = project_file

    def __str__(self):
        return 'project "{0}"'.format(self.project_file)


class ParseRequest(Request):
    def __init__(self, file_name, content='', full=True, *args, **kwargs):
        super ().__init__ (*args, add_newline=False, **kwargs)
        self.file_name = file_name
        self.content = content
        self.full = full

    def __str__(self):
        cmd = 'parse "{0}"'.format(self.file_name)
        if self.full:
            cmd += ' full'
        cmd += '\n'
        cmd += self.content + '\n<<EOF>>\n'
        return cmd


class DeclarationsRequest(Request):
    def __init__(self, file_name, *args, **kwargs):
        super ().__init__ (*args, **kwargs)
        self.file_name = file_name

    def __str__(self):
        return 'declarations "{0}"'.format(self.file_name)


class DataRequest(Request):
    def __init__(self, *args, content='', add_newline=False, **kwargs):
        super ().__init__ (*args, add_newline=add_newline, **kwargs)
        self.content = content

    def __str__(self):
        return self.content


class AdHocRequest (Request):
    def __init__(self, content, *args, **kwargs):
        super ().__init__ (*args, **kwargs)
        self.content = content

    def __str__(self):
        return self.content


def read_reqs(origin, req_proc):
    while True:
        try:
            data = origin.get(block=True, timeout=5)
            if not data:
                print ('oops, no data to consume')
                break

            try:
                if actions.get(block=False) == STOP_SIGNAL:
                    print ('asked to stop, complying')
                    break
            except:
                pass

            req_proc (json.loads(data.decode ('utf-8')))
        except queue.Empty:
            pass



class FsacClient(object):

    def __init__(self, server, req_proc):
        self.requests = requests_queue
        self.server = server

        threading.Thread(target=read_reqs, args=(responses_queue, req_proc)).start()

    def stop(self):
        self.server.stdin.close()

    def send_request(self, request):
        print ("sending request from client...")
        self.requests.put(request.encode())

