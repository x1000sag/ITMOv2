#!/usr/bin/env node
'use strict';

const { spawn } = require('child_process');
const fs = require('fs');
const path = require('path');

const PID_DEFAULT = '.agents/skills/watch-runner/.watcher.pid';
const LOG_DEFAULT = '.agents/skills/watch-runner/watch.log';

function argsToObject(argv){
  const res = { pidFile: PID_DEFAULT, docs: './docs', src: './kanban-cli', interval: 1500, duration: null };
  for (let i = 0; i < argv.length; i++){
    const k = argv[i];
    if (k === '--pidFile') res.pidFile = argv[++i] || res.pidFile;
    else if (k === '--docs') res.docs = argv[++i] || res.docs;
    else if (k === '--src') res.src = argv[++i] || res.src;
    else if (k === '--interval') res.interval = Number(argv[++i]) || res.interval;
    else if (k === '--duration') res.duration = Number(argv[++i]) || null;
  }
  return res;
}

function start(opts){
  const logFile = LOG_DEFAULT;
  const args = ['.agents/skills/watch-runner/src/watcher.js', '--docs', opts.docs, '--src', opts.src, '--interval', String(opts.interval), '--pidFile', opts.pidFile, '--logFile', logFile];
  if (opts.duration) { args.push('--duration', String(opts.duration)); }
  const watcher = spawn('node', args, { stdio: 'inherit' });
  watcher.on('spawn', () => {
    console.log('[watch-ctl] watcher started pid', watcher.pid);
  });
}

function stop(opts){
  try{
    const pid = Number(fs.readFileSync(opts.pidFile, 'utf8'));
    if (!pid) throw new Error('pid not found');
    process.kill(pid);
    console.log('[watch-ctl] watcher stopped pid', pid);
  } catch (e){
    console.error('[watch-ctl] stop error:', e.message);
  }
}

function status(opts){
  try{
    const pid = Number(fs.readFileSync(opts.pidFile, 'utf8'));
    if (!pid) throw new Error('pid not found');
    try{
      process.kill(pid, 0);
      console.log('[watch-ctl] running pid', pid);
    } catch {
      console.log('[watch-ctl] pid file exists but process not running', pid);
    }
  } catch {
    console.log('[watch-ctl] not running');
  }
}

function main(){
  const [cmd, ...rest] = process.argv.slice(2);
  const opts = argsToObject(rest);
  if (cmd === 'start') return start(opts);
  if (cmd === 'stop') return stop(opts);
  if (cmd === 'status') return status(opts);
  console.log('usage: node ctl.js <start|stop|status> [--docs <dir>] [--src <dir>] [--interval <ms>] [--pidFile <file>]');
}

main();
