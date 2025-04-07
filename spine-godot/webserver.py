from http.server import HTTPServer, SimpleHTTPRequestHandler
import os
import sys
import argparse

class GodotWebServer(SimpleHTTPRequestHandler):
    def end_headers(self):
        self.send_header('Cross-Origin-Opener-Policy', 'same-origin')
        self.send_header('Cross-Origin-Embedder-Policy', 'require-corp')
        super().end_headers()

def run_server(directory='.', port=8000):
    os.chdir(directory)

    server_address = ('', port)
    httpd = HTTPServer(server_address, GodotWebServer)
    print(f'Serving directory: {os.path.abspath(directory)}')
    print(f'Server running at http://localhost:{port}/')
    try:
        httpd.serve_forever()
    except KeyboardInterrupt:
        print('\nShutting down server...')
        httpd.server_close()

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description='Start a web server for Godot exports')
    parser.add_argument('path', nargs='?', default='.',
                      help='Path to the directory to serve (default: current directory)')
    parser.add_argument('--port', '-p', type=int, default=8000,
                      help='Port to run the server on (default: 8000)')

    args = parser.parse_args()

    if not os.path.exists(args.path):
        print(f"Error: Directory '{args.path}' does not exist")
        sys.exit(1)

    if not os.path.isdir(args.path):
        print(f"Error: '{args.path}' is not a directory")
        sys.exit(1)

    run_server(args.path, args.port)