/**
 * Minimal MCP server over stdio exposing tools to list/read/search ./docs.
 * No ML, simple filesystem access with basic safety checks.
 */
import * as path from "path";
const sdkServer = require("@modelcontextprotocol/sdk/dist/cjs/server/streamableHttp");
const stdio = require("@modelcontextprotocol/sdk/dist/cjs/server/stdio");
import { createDocsTools } from "./lib";

type ToolResultContent = { type: "text"; text: string };

const DOCS_ROOT = path.resolve(process.cwd(), process.env.DOCS_ROOT || "docs");

async function main() {
  const server = new sdkServer.McpServer({ name: "mcp-docs-ts", version: "0.1.0" });
  const tools = createDocsTools(DOCS_ROOT);

  server.tool(
    {
      name: "docs.list",
      description: "Перечисляет файлы в каталоге DOCS_ROOT (по умолчанию ./docs). Вход: нет. Выход: JSON { root, files }"
    },
    async () => await tools.handleDocsList()
  );

  server.tool(
    {
      name: "docs.read",
      description: "Читает файл по относительному пути от DOCS_ROOT. Вход: { path: string }. Ошибки: неверный путь/нет файла"
    },
    async ({ input }: { input: { path: string } }) => await tools.handleDocsRead(input)
  );

  server.tool(
    {
      name: "docs.search",
      description: "Ищет строки по всем файлам DOCS_ROOT. Вход: { query: string, regex?: boolean, caseSensitive?: boolean }. Выход: совпадения по файлам"
    },
    async ({ input }: { input: { query: string; regex?: boolean; caseSensitive?: boolean } }) => await tools.handleDocsSearch(input)
  );

  const transport = new stdio.StdioServerTransport();
  await server.connect(transport);
}

main().catch((err) => {
  const msg = err instanceof Error ? err.message : String(err);
  // eslint-disable-next-line no-console
  console.error(msg);
  process.exit(1);
});
