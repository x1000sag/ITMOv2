import path from "node:path";
import fs from "node:fs/promises";

export type ToolResultContent = { type: "text"; text: string };

async function pathExists(p: string): Promise<boolean> {
  try {
    await fs.access(p);
    return true;
  } catch {
    return false;
  }
}

function ensureInsideRoot(root: string, rel: string): string {
  if (!rel || rel.includes("\\") || rel.includes("..")) {
    throw new Error("Некорректный путь: запрещены '..' и обратные слеши");
  }
  const abs = path.resolve(root, rel);
  if (!abs.startsWith(root)) {
    throw new Error("Запрошенный путь вне DOCS_ROOT");
  }
  return abs;
}

async function listFiles(dir: string): Promise<string[]> {
  const out: string[] = [];
  async function walk(current: string, base: string) {
    const entries = await fs.readdir(current, { withFileTypes: true });
    for (const e of entries) {
      const abs = path.join(current, e.name);
      const rel = path.relative(base, abs);
      if (e.isDirectory()) {
        await walk(abs, base);
      } else if (e.isFile()) {
        out.push(rel);
      }
    }
  }
  await walk(dir, dir);
  return out.sort();
}

async function readTextFile(file: string): Promise<string> {
  const buf = await fs.readFile(file);
  return buf.toString("utf8");
}

function searchInText(content: string, query: string, opts: { regex?: boolean; caseSensitive?: boolean }) {
  const lines = content.split(/\r?\n/);
  const results: { line: number; match: string }[] = [];
  let matcher: (line: string) => RegExpMatchArray | null;
  if (opts.regex) {
    const flags = opts.caseSensitive ? "g" : "gi";
    const re = new RegExp(query, flags);
    matcher = (line) => line.match(re);
  } else {
    const q = opts.caseSensitive ? query : query.toLowerCase();
    matcher = (line) => {
      const hay = opts.caseSensitive ? line : line.toLowerCase();
      return hay.includes(q) ? ([''] as unknown as RegExpMatchArray) : null;
    };
  }
  lines.forEach((line, idx) => {
    const m = matcher(line);
    if (m) results.push({ line: idx + 1, match: line });
  });
  return results;
}

export function createDocsTools(root: string) {
  const DOCS_ROOT = root;

  return {
    async handleDocsList(): Promise<{ content: ToolResultContent[] }> {
      if (!(await pathExists(DOCS_ROOT))) {
        throw new Error(`Каталог docs не найден: ${DOCS_ROOT}`);
      }
      const files = await listFiles(DOCS_ROOT);
      const payload = { root: DOCS_ROOT, files };
      return { content: [{ type: "text", text: JSON.stringify(payload, null, 2) }] };
    },

    async handleDocsRead(input: { path: string }): Promise<{ content: ToolResultContent[] }> {
      if (!input?.path) throw new Error("Требуется параметр 'path'");
      const abs = ensureInsideRoot(DOCS_ROOT, input.path);
      const exists = await pathExists(abs);
      if (!exists) throw new Error(`Файл не найден: ${input.path}`);
      const text = await readTextFile(abs);
      const payload = { path: input.path, content: text };
      return { content: [{ type: "text", text: JSON.stringify(payload) }] };
    },

    async handleDocsSearch(input: { query: string; regex?: boolean; caseSensitive?: boolean }): Promise<{ content: ToolResultContent[] }> {
      if (!input?.query) throw new Error("Требуется параметр 'query'");
      if (!(await pathExists(DOCS_ROOT))) throw new Error(`Каталог docs не найден: ${DOCS_ROOT}`);
      const files = await listFiles(DOCS_ROOT);
      const results: Array<{ file: string; hits: Array<{ line: number; text: string }> }> = [];
      for (const rel of files) {
        const abs = ensureInsideRoot(DOCS_ROOT, rel);
        const content = await readTextFile(abs);
        const hits = searchInText(content, input.query, { regex: !!input.regex, caseSensitive: !!input.caseSensitive })
          .map(h => ({ line: h.line, text: h.match }));
        if (hits.length) results.push({ file: rel, hits });
      }
      const payload = { query: input.query, results };
      return { content: [{ type: "text", text: JSON.stringify(payload, null, 2) }] };
    }
  };
}
