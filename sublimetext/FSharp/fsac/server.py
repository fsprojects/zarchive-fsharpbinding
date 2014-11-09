import threading
import queue
import asyncore
import socket
import time
import os

from FSharp.fsac.pipe_server import PipeServer


PATH_TO_FSAC = os.path.join(os.path.dirname(__file__), 'fsac/fsautocomplete.exe')


requests_queue = queue.Queue()
actions_queue = queue.Queue()
responses_queue = queue.Queue()

STOP_SIGNAL = '__STOP'


def make_request(server):
    while True:
        try:
            req = requests_queue.get(block=True, timeout=5)

            try:
                if actions_queue.get(block=False) == STOP_SIGNAL:
                    print('exiting as requested')
                    actions_queue.put(STOP_SIGNAL)
                    break
            except:
                pass

            if req:
                print ('got this', req)
                server.fsac.proc.stdin.write(req)
                server.fsac.proc.stdin.flush ()
        except queue.Empty:
            pass
    print("request processor, I'm out...")


def make_response(server):
    while True:
        try:
            print ("response processor, here I go...")
            data = server.fsac.proc.stdout.readline()
            if not data:
                print ('boo, exiting because not data')
                break

            try:
                if actions_queue.get(block=False) == STOP_SIGNAL:
                    print('exiting as requested')
                    actions_queue.put(STOP_SIGNAL)
                    break
            except:
                pass

            responses_queue.put (data)
        except queue.Empty:
            pass
    print("response processor, I'm out...")


class FsacServer(object):
    def __init__(self, cmd):
        fsac = PipeServer(cmd)
        fsac.start()
        fsac.proc.stdin.write('outputmode json\n'.encode ('ascii'))
        self.fsac = fsac

        threading.Thread (target=make_request, args=(self,)).start ()
        threading.Thread (target=make_response, args=(self,)).start ()


def start():
    return FsacServer([PATH_TO_FSAC])


# if __name__ == '__main__':
#     print("starting fsautocomplete...")
#     start()
