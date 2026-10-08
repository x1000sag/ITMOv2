import path from "node:path";
import fs from "node:fs/promises";
import { createDocsTools } from "./lib.js";

async function main() {
  const tmpRoot = path.resolve("/tmp/opencode/mcp-docs-test");
  const docsDir = path.join(tmpRoot, "docs");
  await fs.rm(tmpRoot, { recursive: true, force: true });
  await fs.mkdir(docsDir, { recursive: true });
  await fs.writeFile(path.join(docsDir, "a.md"), "Hello world\nKanban CLI\n");
  await fs.writeFile(path.join(docsDir, "b.txt"), "deadline: 2024-01-01\n");

  const tools = createDocsTools(docsDir);

  // docs.list
  const list = await tools.handleDocsList();
  console.log("LIST:", list.content[0].text);

  // docs.read success
  const readOk = await tools.handleDocsRead({ path: "a.md" });
  console.log("READ_OK:", readOk.content[0].text.substring(0, 40), "...");

  // docs.read error
  try {
    await tools.handleDocsRead({ path: "../etc/passwd" });
    console.error("ERROR: expected path traversal to fail");
    process.exit(1);
  } catch (e) {
    console.log("READ_ERR:", (e as Error).message);
  }

  // docs.search
  const search = await tools.handleDocsSearch({ query: "kanban", regex: false, caseSensitive: false });
  console.log("SEARCH:", search.content[0].text);
}

main().catch((e) => {
  console.error(e);
  process.exit(1);
});
