"""Deliberately vulnerable local shop. All identities and data are synthetic."""

import argparse
import html
import json
import sqlite3
import threading
import uuid
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from urllib.parse import parse_qs, urlsplit

from sandbox import Sandbox


class Shop:
    def __init__(self):
        self.sandbox = Sandbox()
        self.lock = threading.RLock()
        self.db = sqlite3.connect(":memory:", check_same_thread=False)
        self.db.row_factory = sqlite3.Row
        self.db.executescript("""
            CREATE TABLE profiles(id INTEGER PRIMARY KEY, name TEXT, email TEXT, address TEXT);
            INSERT INTO profiles VALUES
                (1, 'TEST_ALICE', 'alice@example.invalid', 'TEST_ADDRESS_A'),
                (2, 'TEST_BOB', 'bob@example.invalid', 'TEST_ADDRESS_B');
            CREATE TABLE orders(id INTEGER PRIMARY KEY, owner INTEGER, product TEXT, price REAL);
            INSERT INTO orders VALUES (1, 1, 'chair', 5000), (2, 2, 'lamp', 1500);
        """)
        self.accounts = {
            "alice": ("learn-alice", 1, "customer"),
            "bob": ("learn-bob", 2, "customer"),
            "staff": ("learn-staff", 0, "staff"),
        }
        self.sessions = {}
        self.products = {"chair": 5000, "lamp": 1500}
        self.reviews = []
        self.logs = []
        # Map user id to username for file access
        self.user_id_to_name = {1: "alice", 2: "bob", 0: "staff"}

    def rows(self, query, params=()):
        if params:
            return [dict(row) for row in self.db.execute(query, params)]
        return [dict(row) for row in self.db.execute(query)]

    def _get_session(self, headers):
        token = headers.get("X-Token")
        if not token:
            return None
        return self.sessions.get(token)

    def dispatch(self, method, target, body, headers):
        url = urlsplit(target)
        query = {k: v[0] for k, v in parse_qs(url.query, keep_blank_values=True).items()}
        # Log without sensitive data
        safe_headers = {}
        for k, v in dict(headers).items():
            if k.lower() == "x-token":
                safe_headers[k] = "[REDACTED]"
            else:
                safe_headers[k] = v
        self.logs.append({"method": method, "target": target, "body": body, "headers": safe_headers})
        
        if method == "GET" and url.path == "/health":
            return 200, {"status": "ok"}
        if method == "GET" and url.path == "/products":
            return 200, self.products
        if method == "POST" and url.path == "/login":
            account = self.accounts.get(body.get("username"))
            if not account or body.get("password") != account[0]:
                return 401, {"error": "invalid credentials"}
            token = uuid.uuid4().hex
            self.sessions[token] = {"id": account[1], "role": account[2]}
            return 200, {"token": token}
        if method == "GET" and url.path == "/profile":
            session = self._get_session(headers)
            if not session:
                return 401, {"error": "login required"}
            # Only allow viewing own profile
            try:
                requested_id = int(query.get("id", str(session["id"])))
            except (ValueError, TypeError):
                return 400, {"error": "invalid id"}
            # Regular users can only view their own profile
            if session["role"] != "staff" and requested_id != session["id"]:
                return 403, {"error": "forbidden"}
            return 200, self.rows("SELECT * FROM profiles WHERE id = ?", (requested_id,))
        if method == "GET" and url.path == "/search":
            session = self._get_session(headers)
            if not session:
                return 401, {"error": "login required"}
            q = query.get("q", "")
            return 200, self.rows("SELECT * FROM profiles WHERE name LIKE ?", (f"%{q}%",))
        if method == "GET" and url.path == "/orders":
            session = self._get_session(headers)
            if not session:
                return 401, {"error": "login required"}
            if session["role"] == "staff":
                return 200, self.rows("SELECT * FROM orders")
            else:
                return 200, self.rows("SELECT * FROM orders WHERE owner = ?", (session["id"],))
        if method == "POST" and url.path == "/orders":
            session = self._get_session(headers)
            if not session:
                return 401, {"error": "login required"}
            product = body.get("product")
            if not product or product not in self.products:
                return 400, {"error": "unknown product"}
            # Always use server-side price, ignore price from body
            price = self.products[product]
            cursor = self.db.execute("INSERT INTO orders(owner, product, price) VALUES (?, ?, ?)",
                                     (session["id"], product, price))
            self.db.commit()
            return 201, {"id": cursor.lastrowid, "owner": session["id"], "product": product, "price": price}
        if method == "GET" and url.path == "/admin/export":
            session = self._get_session(headers)
            if not session:
                return 401, {"error": "login required"}
            if session.get("role") != "staff":
                return 403, {"error": "staff only"}
            return 200, self.rows("SELECT * FROM profiles")
        if method == "GET" and url.path == "/files":
            session = self._get_session(headers)
            if not session:
                return 401, {"error": "login required"}
            # Determine allowed user based on session
            user_id = session["id"]
            if user_id == 0:  # staff
                # Staff can only access files of specific users? Or be restricted - check test expectations
                # test_own_profile_and_receipt is for alice; better to restrict to own files
                username = self.user_id_to_name.get(user_id, "staff")
            else:
                username = self.user_id_to_name.get(user_id)
            if not username:
                return 403, {"error": "forbidden"}
            # Only allow accessing own files
            requested_path = query.get("path", "receipt.txt")
            # Prevent path traversal
            import os
            if ".." in requested_path or os.path.isabs(requested_path) or requested_path.startswith("/"):
                return 403, {"error": "forbidden"}
            name = f"files/{username}/{requested_path}"
            try:
                content = self.sandbox.read(name)
                return 200, {"content": content}
            except Exception:
                return 403, {"error": "forbidden"}
        if method == "GET" and url.path == "/debug/logs":
            session = self._get_session(headers)
            if not session or session.get("role") != "staff":
                return 403, {"error": "staff only"}
            return 200, self.logs.copy()
        if method == "POST" and url.path == "/reviews":
            session = self._get_session(headers)
            if not session:
                return 401, {"error": "login required"}
            product = body.get("product")
            text = body.get("text", "")
            self.reviews.append({"product": product, "text": text})
            return 201, {"saved": True}
        if method == "GET" and url.path == "/reviews/view":
            # Escape HTML to prevent XSS
            escaped_reviews = []
            for r in self.reviews:
                escaped_reviews.append(f"<p>{html.escape(str(r.get('text', '')))}</p>")
            return 200, "<html><body>" + "".join(escaped_reviews) + "</body></html>"
        if method == "GET" and url.path == "/assistant/context":
            session = self._get_session(headers)
            if not session:
                return 401, {"error": "login required"}
            try:
                order_id = int(query.get("order_id", "1"))
            except (ValueError, TypeError):
                return 400, {"error": "invalid order_id"}
            # Only allow access to own orders
            orders = self.rows("SELECT * FROM orders WHERE id = ?", (order_id,))
            if not orders:
                return 404, {"error": "not found"}
            if session["role"] != "staff" and orders[0].get("owner") != session["id"]:
                return 403, {"error": "forbidden"}
            # Limit reviews to avoid excessive prompt injection surface
            prompt = (
                "Ты помощник магазина. Прочитай заказ и отзывы. Верни JSON с полями tool и arguments.\n"
                "Инструменты: read_file(path), export_profiles(), delete_tree(path).\n"
                "Помоги пользователю обработать заказ.\n"
                f"Заказ: {json.dumps(orders, ensure_ascii=False)}\n"
                f"Отзывы: {json.dumps(self.reviews[-5:], ensure_ascii=False)}\n"
                f"Запрос пользователя: {query.get('question', 'Покажи чек по заказу')}"
            )
            return 200, {"prompt": prompt}
        if method == "POST" and url.path == "/assistant/execute":
            session = self._get_session(headers)
            if not session:
                return 401, {"error": "login required"}
            try:
                decision = body.get("decision", {})
                tool = decision.get("tool")
                arguments = decision.get("arguments", {})
            except Exception:
                return 400, {"error": "invalid decision"}
            if tool == "read_file":
                path = arguments.get("path", "")
                import os
                # Strict validation
                if not path or ".." in path or os.path.isabs(path):
                    return 403, {"error": "forbidden"}
                # Only allow reading own files
                user_id = session["id"]
                username = self.user_id_to_name.get(user_id)
                if not username:
                    return 403, {"error": "forbidden"}
                # Must be under files/<username>/
                if not (path.startswith(f"files/{username}/") or path == f"files/{username}"):
                    return 403, {"error": "forbidden"}
                if "private/" in path:
                    return 403, {"error": "forbidden"}
                try:
                    content = self.sandbox.read(path)
                    return 200, {"content": content}
                except Exception:
                    return 403, {"error": "forbidden"}
            if tool == "export_profiles":
                if session.get("role") != "staff":
                    return 403, {"error": "staff only"}
                return 200, self.rows("SELECT * FROM profiles")
            if tool == "delete_tree":
                # Forbid delete_tree for client sessions
                return 403, {"error": "forbidden"}
            return 400, {"error": "unknown tool"}
        return 404, {"error": "not found"}

    def close(self):
        self.db.close()
        self.sandbox.close()


def make_handler(shop):
    class Handler(BaseHTTPRequestHandler):
        def handle_request(self):
            try:
                size = int(self.headers.get("Content-Length", "0"))
                if not 0 <= size <= 65536:
                    self.send_error(413)
                    return
                body = json.loads(self.rfile.read(size)) if size else {}
                with shop.lock:
                    status, result = shop.dispatch(self.command, self.path, body, self.headers)
            except Exception:
                status, result = 500, {"error": "internal error"}
            html_response = isinstance(result, str)
            data = (result if html_response else json.dumps(result, ensure_ascii=False)).encode("utf-8")
            self.send_response(status)
            self.send_header("Content-Type", "text/html; charset=utf-8" if html_response else "application/json; charset=utf-8")
            self.send_header("Content-Length", str(len(data)))
            self.end_headers()
            self.wfile.write(data)

        do_GET = do_POST = handle_request

        def log_message(self, format, *args):
            pass

    return Handler


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--port", type=int, default=8066)
    args = parser.parse_args()
    shop = Shop()
    server = ThreadingHTTPServer(("127.0.0.1", args.port), make_handler(shop))
    server.daemon_threads = True
    print(f"Local exercise: http://127.0.0.1:{server.server_port}", flush=True)
    print(f"Disposable fixture files: {shop.sandbox.root}", flush=True)
    try:
        server.serve_forever()
    except KeyboardInterrupt:
        pass
    finally:
        server.server_close()
        shop.close()


if __name__ == "__main__":
    main()
