import type { Plugin } from "@opencode-ai/plugin";
import { spawn } from "node:child_process";
import * as fs from "node:fs";
import * as path from "node:path";

// Простая эвристика: реагируем на изменения, внесённые через apply_patch.
// Если изменены файлы в kanban-cli/ (.cs/.csproj/.sln) — запускаем dotnet test.
// Если изменены файлы в docs/ — просто фиксируем событие (без ML/линта по умолчанию).

function logLine(message: string) {
  try {
    const logDir = path.resolve('.opencode/plugin');
    const logFile = path.join(logDir, 'agent-watch.log');
    const ts = new Date().toISOString();
    const line = `[${ts}] ${message}\n`;
    fs.mkdirSync(logDir, { recursive: true });
    fs.appendFileSync(logFile, line, 'utf8');
    // Дублируем в stdout для живых сессий
    // console.log оставляем — он полезен агентам
    console.log('[agent-watch]', message);
  } catch {
    // Игнор лог-файла при ошибках файловой системы, но сохраняем консольный вывод
    console.log('[agent-watch]', message);
  }
}

function parsePatchedFiles(patchText: string): string[] {
  const files: string[] = [];
  const re = /\*\*\*\s+(?:Update|Add|Delete)\s+File:\s+(.+?)\n/g;
  let m: RegExpExecArray | null;
  while ((m = re.exec(patchText)) !== null) {
    files.push(m[1].trim());
  }
  return files;
}

function shouldRunDotnet(files: string[]): boolean {
  return files.some(f => (
    f.startsWith("kanban-cli/") && (f.endsWith('.cs') || f.endsWith('.csproj') || f.endsWith('.sln'))
  ));
}

function changedDocs(files: string[]): boolean {
  return files.some(f => f.startsWith("docs/"));
}

let lastRunAt = 0;
let running = false;

function findDotnetCwd(): string | null {
  const candidate = path.resolve('kanban-cli');
  try {
    const entries = fs.readdirSync(candidate);
    const hasProj = entries.some(e => e.endsWith('.sln') || e.endsWith('.csproj'));
    if (hasProj) return candidate;
  } catch {}
  return null;
}

function runDotnetTestOnce(cwd: string): Promise<{ code: number }>{
  return new Promise(resolve => {
    const child = spawn('dotnet', ['test'], { cwd, stdio: 'inherit' });
    child.on('close', code => resolve({ code: code ?? -1 }));
    child.on('error', () => resolve({ code: -1 }));
  });
}

export const AgentWatch: Plugin = async ({ client }) => {
  logLine('AgentWatch plugin loaded');
  return {
    tool: {
      execute: {
        // Выполняется ПОСЛЕ выполнения инструмента (если поддерживается).
        after: async (input: any, output: any) => {
          try {
            logLine(`after hook invoked: tool=${String(input?.tool)} keys=[${Object.keys(input || {}).join(',')}]`);
            if (input?.tool !== 'apply_patch') return;
            const patchText: string | undefined = input?.args?.patchText;
            if (!patchText) {
              logLine('after hook: apply_patch без patchText');
              return;
            }
            const files = parsePatchedFiles(patchText);
            if (!files.length) return;
            logLine(`after hook: apply_patch files => ${files.join(', ')}`);

            if (changedDocs(files)) {
              const docsCount = files.filter(f => f.startsWith('docs/')).length;
              logLine(`Docs changed: ${docsCount} file(s)`);
            }

            if (!shouldRunDotnet(files)) return;

            // Дебаунс: не чаще одного запуска в ~2с и без параллельных запусков.
            const now = Date.now();
            if (running || (now - lastRunAt) < 2000) return;
            running = true;
            const dotnetCwd = findDotnetCwd();
            if (!dotnetCwd) {
              logLine('Code changed under kanban-cli, но .sln/.csproj не найдены — пропускаем dotnet test');
              lastRunAt = Date.now();
              running = false;
              return;
            }
            logLine(`Изменения в kanban-cli — запуск dotnet test (cwd=${dotnetCwd})...`);
            const res = await runDotnetTestOnce(dotnetCwd);
            lastRunAt = Date.now();
            running = false;
            logLine(`dotnet test завершился с кодом ${res.code}`);
          } catch (e) {
            logLine(`Ошибка хука: ${(e as Error)?.message ?? e}`);
          }
        },
        // Фоллбек: если after недоступен — проверяем до выполнения инструмента по данным патча.
        before: async (input: any) => {
          try {
            logLine(`before hook invoked: tool=${String(input?.tool)} keys=[${Object.keys(input || {}).join(',')}]`);
            if (input?.tool !== 'apply_patch') return;
            const patchText: string | undefined = input?.args?.patchText;
            if (!patchText) {
              logLine('before hook: apply_patch без patchText');
              return;
            }
            const files = parsePatchedFiles(patchText);
            if (!files.length) return;
            logLine(`before hook: apply_patch files => ${files.join(', ')}`);
            if (changedDocs(files)) {
              logLine('(pre) Изменения в документации обнаружены');
            }
            // Не запускаем тесты до применения патча; оставим эту часть в after.
          } catch (e) {
            logLine(`Ошибка pre-хука: ${(e as Error)?.message ?? e}`);
          }
        }
      }
    }
  };
};

export default AgentWatch;
