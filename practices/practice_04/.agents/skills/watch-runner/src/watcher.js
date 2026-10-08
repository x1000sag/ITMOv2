#!/usr/bin/env node
'use strict';

// Простая реализация watcher без внешних зависимостей.
// Следит за изменениями в docs/ и kanban-cli/ и по событию выполняет проверки.

const fs = require('fs');
const path = require('path');
const { spawn } = require('child_process');

function parseArgs(argv) {
  const args = { docs: './docs', src: './kanban-cli', interval: 1500, duration: null, mode: 'watch', pidFile: '.agents/skills/watch-runner/.watcher.pid', logFile: '.agents/skills/watch-runner/watch.log' };
  for (let i = 2; i < argv.length; i++) {
    const k = argv[i];
    if (k === '--docs') args.docs = argv[++i] || args.docs;
    else if (k === '--src') args.src = argv[++i] || args.src;
    else if (k === '--interval') args.interval = Number(argv[++i]) || args.interval;
    else if (k === '--duration') args.duration = Number(argv[++i]) || null;
    else if (k === '--mode') args.mode = (argv[++i] || 'watch');
    else if (k === '--pidFile') args.pidFile = argv[++i] || args.pidFile;
    else if (k === '--logFile') args.logFile = argv[++i] || args.logFile;
  }
  return args;
}

function* walk(dir) {
  const stack = [dir];
  while (stack.length) {
    const current = stack.pop();
    let entries;
    try {
      entries = fs.readdirSync(current, { withFileTypes: true });
    } catch (e) {
      continue;
    }
    for (const e of entries) {
      const p = path.join(current, e.name);
      if (e.isDirectory()) stack.push(p);
      else yield p;
    }
  }
}

function snapshot(dir, filter) {
  const map = new Map();
  for (const f of walk(dir)) {
    if (filter && !filter(f)) continue;
    try {
      const st = fs.statSync(f);
      map.set(f, st.mtimeMs);
    } catch {}
  }
  return map;
}

function diff(prev, next) {
  const changes = [];
  for (const [file, mtime] of next.entries()) {
    const old = prev.get(file);
    if (!old || old !== mtime) changes.push(file);
  }
  for (const file of prev.keys()) {
    if (!next.has(file)) changes.push(file);
  }
  return changes;
}

function hasDotnetProject(dir) {
  try {
    const entries = fs.readdirSync(dir);
    return entries.some(x => x.endsWith('.sln')) || entries.some(x => x.endsWith('.csproj'));
  } catch { return false; }
}

function runDotnetTest(cwd) {
  return new Promise((resolve) => {
    const cmd = 'dotnet';
    const args = ['test'];
    const proc = spawn(cmd, args, { cwd, stdio: 'pipe' });
    let out = '', err = '';
    proc.stdout.on('data', d => { const s = d.toString(); out += s; process.stdout.write(s); });
    proc.stderr.on('data', d => { const s = d.toString(); err += s; process.stderr.write(s); });
    proc.on('close', code => {
      resolve({ code, out, err });
    });
    proc.on('error', e => {
      console.error('[watch-runner] Не удалось запустить dotnet:', e.message);
      resolve({ code: -1, out, err: e.message });
    });
  });
}

async function main() {
  const args = parseArgs(process.argv);
  // Лог-файл
  try {
    fs.mkdirSync(path.dirname(args.logFile), { recursive: true });
    fs.appendFileSync(args.logFile, `[${new Date().toISOString()}] watcher start docs=${args.docs} src=${args.src} interval=${args.interval} mode=${args.mode} duration=${args.duration ?? '∞'}\n`);
  } catch {}
  console.log('[watch-runner] Запуск. docs:', args.docs, 'src:', args.src, 'interval:', args.interval, 'mode:', args.mode, 'duration:', args.duration ?? '∞');

  const docsFilter = f => f.endsWith('.md');
  const srcFilter = f => /\.cs$|\.csproj$|\.sln$/.test(f);

  let docsSnap = snapshot(args.docs, docsFilter);
  let srcSnap = snapshot(args.src, srcFilter);

  const haveDotnet = hasDotnetProject(args.src);
  if (!haveDotnet) {
    const msg = `[watch-runner] Внимание: не найдены .sln или .csproj в ${args.src} - тесты не будут запускаться.`;
    console.warn(msg);
    try { fs.appendFileSync(args.logFile, `[${new Date().toISOString()}] ${msg}\n`); } catch {}
  }

  // PID-файл
  try {
    fs.mkdirSync(path.dirname(args.pidFile), { recursive: true });
    fs.writeFileSync(args.pidFile, String(process.pid), 'utf8');
  } catch {}

  const startedAt = Date.now();
  const timer = setInterval(async () => {
    const newDocs = snapshot(args.docs, docsFilter);
    const docsChanges = diff(docsSnap, newDocs);
    if (docsChanges.length) {
      console.log('[watch-runner] Изменения в документации:', docsChanges.length);
      try { fs.appendFileSync(args.logFile, `[${new Date().toISOString()}] docs changed: ${docsChanges.length}\n`); } catch {}
      docsSnap = newDocs;
    }

    const newSrc = snapshot(args.src, srcFilter);
    const srcChanges = diff(srcSnap, newSrc);
    if (srcChanges.length) {
      console.log('[watch-runner] Изменения в исходниках:', srcChanges.length);
      try { fs.appendFileSync(args.logFile, `[${new Date().toISOString()}] src changed: ${srcChanges.length}\n`); } catch {}
      srcSnap = newSrc;
      if (haveDotnet) {
        console.log('[watch-runner] Запуск dotnet test...');
        try { fs.appendFileSync(args.logFile, `[${new Date().toISOString()}] dotnet test start\n`); } catch {}
        const res = await runDotnetTest(args.src);
        console.log('[watch-runner] dotnet test завершился с кодом', res.code);
        try { fs.appendFileSync(args.logFile, `[${new Date().toISOString()}] dotnet test exit code=${res.code}\n`); } catch {}
      }
    }
    if (args.duration && Date.now() - startedAt >= args.duration) {
      console.log('[watch-runner] Время работы истекло, завершаем.');
      try { fs.appendFileSync(args.logFile, `[${new Date().toISOString()}] watcher stop by duration\n`); } catch {}
      clearInterval(timer);
      process.exit(0);
    }
  }, args.interval);
}

main().catch(err => {
  console.error('[watch-runner] Ошибка:', err);
  process.exit(1);
});
