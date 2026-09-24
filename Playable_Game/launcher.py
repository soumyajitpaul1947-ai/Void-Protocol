import http.server
import socketserver
import webbrowser
import os
import sys
import socket

DIRECTORY = os.path.dirname(os.path.abspath(__file__))

class Handler(http.server.SimpleHTTPRequestHandler):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, directory=DIRECTORY, **kwargs)

    def log_message(self, format, *args):
        # Keep launcher terminal clean
        pass

def find_open_port(start_port=8080):
    port = start_port
    while port < start_port + 50:
        try:
            with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
                s.bind(('localhost', port))
                return port
        except OSError:
            port += 1
    return start_port

def main():
    port = find_open_port(8080)
    url = f"http://localhost:{port}/index.html"

    print("============================================================")
    print("              VOID PROTOCOL - GAME LAUNCHER                ")
    print("============================================================")
    print(f" Game Server running at: {url}")
    print(" Launching default browser...")
    print(" [TIP] Keep this window open while playing.")
    print(" [TIP] Press Ctrl+C to close the game server when done.")
    print("============================================================")

    webbrowser.open(url)

    with socketserver.TCPServer(("", port), Handler) as httpd:
        try:
            httpd.serve_forever()
        except KeyboardInterrupt:
            print("\nShutting down Void Protocol game server. Goodbye!")
            sys.exit(0)

if __name__ == '__main__':
    main()
