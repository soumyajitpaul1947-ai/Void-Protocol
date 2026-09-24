import os
import sys
import socket
import threading
import http.server
import socketserver
import webview

def get_game_dir():
    if getattr(sys, 'frozen', False):
        # PyInstaller temp folder
        if hasattr(sys, '_MEIPASS'):
            bundled = os.path.join(sys._MEIPASS, 'Playable_Game')
            if os.path.isdir(bundled):
                return bundled
        # Or next to the executable
        exe_dir = os.path.dirname(sys.executable)
        candidate = os.path.join(exe_dir, 'Playable_Game')
        if os.path.isdir(candidate):
            return candidate
        return exe_dir
    else:
        here = os.path.dirname(os.path.abspath(__file__))
        candidate = os.path.join(here, 'Playable_Game')
        if os.path.isdir(candidate):
            return candidate
        return here

def find_open_port(start_port=8100):
    port = start_port
    while port < start_port + 50:
        try:
            with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
                s.bind(('127.0.0.1', port))
                return port
        except OSError:
            port += 1
    return start_port

def start_server(game_dir, port):
    class QuietHandler(http.server.SimpleHTTPRequestHandler):
        def __init__(self, *args, **kwargs):
            super().__init__(*args, directory=game_dir, **kwargs)

        def log_message(self, format, *args):
            pass

    server = socketserver.TCPServer(('127.0.0.1', port), QuietHandler)
    server.serve_forever()

def main():
    game_dir = get_game_dir()
    port = find_open_port(8100)

    server_thread = threading.Thread(target=start_server, args=(game_dir, port), daemon=True)
    server_thread.start()

    url = f"http://127.0.0.1:{port}/index.html"

    # Create native desktop window
    window = webview.create_window(
        title='Void Protocol',
        url=url,
        width=1300,
        height=760,
        resizable=True,
        min_size=(960, 540),
        confirm_close=False,
        background_color='#030712'
    )

    webview.start(debug=False)

if __name__ == '__main__':
    main()
