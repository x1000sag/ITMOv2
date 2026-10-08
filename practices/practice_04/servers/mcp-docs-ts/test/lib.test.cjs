// Тесты для createDocsTools (node:test)
const assert = require('node:assert');
const test = require('node:test');
const path = require('node:path');
const fs = require('node:fs/promises');

const { createDocsTools } = require('../dist/lib.js');

async function setupTmpDocs() {
  const root = path.resolve('/tmp/opencode/mcp-docs-e2e');
  const docs = path.join(root, 'docs');
  await fs.rm(root, { recursive: true, force: true });
  await fs.mkdir(docs, { recursive: true });
  await fs.writeFile(path.join(docs, 'a.md'), '# Hello\nKanban CLI\n');
  await fs.writeFile(path.join(docs, 'b.txt'), 'deadline: 2024-01-01\n');
  return docs;
}

test('docs.list returns files', async () => {
  const docs = await setupTmpDocs();
  const tools = createDocsTools(docs);
  const res = await tools.handleDocsList();
  const payload = JSON.parse(res.content[0].text);
  assert.equal(payload.root, docs);
  assert.deepEqual(payload.files.sort(), ['a.md', 'b.txt']);
});

test('docs.read ok and error cases', async () => {
  const docs = await setupTmpDocs();
  const tools = createDocsTools(docs);
  const ok = await tools.handleDocsRead({ path: 'a.md' });
  const payload = JSON.parse(ok.content[0].text);
  assert.equal(payload.path, 'a.md');
  assert.match(payload.content, /Kanban CLI/);

  await assert.rejects(() => tools.handleDocsRead({ path: 'nope.md' }), /Файл не найден/);
  await assert.rejects(() => tools.handleDocsRead({ path: '../etc/passwd' }), /Запрошенный путь вне DOCS_ROOT|Некорректный путь/);
});

test('docs.search finds text (case-insensitive) and supports regex', async () => {
  const docs = await setupTmpDocs();
  const tools = createDocsTools(docs);
  const res1 = await tools.handleDocsSearch({ query: 'kanban', regex: false, caseSensitive: false });
  const payload1 = JSON.parse(res1.content[0].text);
  assert.equal(payload1.query, 'kanban');
  // Должен найти совпадения в a.md
  const hitA = payload1.results.find(r => r.file === 'a.md');
  assert.ok(hitA && hitA.hits.length > 0, 'ожидались совпадения в a.md');

  // regex по слову deadline в b.txt
  const res2 = await tools.handleDocsSearch({ query: 'deadline:\\s+\\d{4}-\\d{2}-\\d{2}', regex: true, caseSensitive: false });
  const payload2 = JSON.parse(res2.content[0].text);
  const hitB = payload2.results.find(r => r.file === 'b.txt');
  assert.ok(hitB && hitB.hits.length > 0, 'ожидались совпадения в b.txt');
});
