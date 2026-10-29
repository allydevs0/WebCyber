import http.server
import socketserver
import os
import sys

PORT = 8080
DIRECTORY = sys.argv[1] if len(sys.argv) > 1 else os.path.dirname(os.path.abspath(__file__))

class WASMHandler(http.server.SimpleHTTPRequestHandler):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, directory=DIRECTORY, **kwargs)

    def end_headers(self):
        self.send_header("Cross-Origin-Opener-Policy", "same-origin")
        self.send_header("Cross-Origin-Embedder-Policy", "require-corp")
        self.send_header("Access-Control-Allow-Origin", "*")
        super().end_headers()

    def guess_type(self, path):
        if path.endswith('.wasm'):
            return 'application/wasm'
        if path.endswith('.data'):
            return 'application/octet-stream'
        if path.endswith('.js'):
            return 'application/javascript'
        return super().guess_type(path)

print(f"==================================================")
print(f" Servidor de Teste Bomb Rush Cyberfunk WebAssembly")
print(f" Servindo: {DIRECTORY}")
print(f" Acesso:   http://localhost:{PORT}")
print(f"==================================================")

with socketserver.TCPServer(("", PORT), WASMHandler) as httpd:
    try:
        httpd.serve_forever()
    except KeyboardInterrupt:
        print("\nServidor finalizado.")
