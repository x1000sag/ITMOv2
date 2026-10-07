---
name: watch-runner
description: >-
  Агентный вотчер/раннер для локальной разработки. Следит за изменениями
  в каталогах `docs/` и `kanban-cli/` и по событию запускает проверки.
  Для C# проектов выполняет `dotnet test` (если обнаружены *.csproj или *.sln).
  Работает как skill для агентов, не является git hook.
license: MIT
metadata:
  portability: portable
  binding: optional
---

# Watch Runner (агентный хук)

Этот skill реализует «агентный хук» (watcher/runner), который реагирует на изменения файлов и
запускает проверки. Он предназначен для использования агентами OpenCode и не зависит от git hooks.

## Когда использовать

- При работе над Kanban CLI: автоматически прогонять `dotnet test` при изменении кода/тестов.
- При обновлении документации: получать быстрые сигналы, что документация изменена (минимальная проверка).

## Что делает

- Следит за директориями:
  - `docs/` — сигнализирует об изменениях (без ML/линтеров по умолчанию).
  - `kanban-cli/` — при изменении `.cs`, `.csproj`, `.sln` пытается выполнить `dotnet test`.
- Возвращает результат в stdout/stderr, доступный агенту.

## Инструменты

```tool
name: watch.start
description: Запускает watcher как фоновый процесс, пишет PID и лог. Возвращает путь к PID и LOG.
input:
  type: object
  properties:
    docs:
      type: string
      description: Путь к каталогу документации (default: ./docs)
    src:
      type: string
      description: Путь к каталогу исходников .NET (default: ./kanban-cli)
    interval:
      type: number
      description: Интервал опроса в мс (default: 1500)
    duration:
      type: number
      description: Максимальная длительность работы в мс; по истечении watcher завершится (default: бесконечно)
    pidFile:
      type: string
      description: Путь к PID-файлу (default: .agents/skills/watch-runner/.watcher.pid)
    logFile:
      type: string
      description: Путь к лог-файлу (default: .agents/skills/watch-runner/watch.log)
  required: []
```

```tool
name: watch.stop
description: Останавливает watcher по PID-файлу.
input:
  type: object
  properties:
    pidFile:
      type: string
      description: Путь к PID-файлу (default: .agents/skills/watch-runner/.watcher.pid)
  required: []
```

```tool
name: watch.status
description: Сообщает статус watcher: запущен/не запущен, PID и пути.
input:
  type: object
  properties:
    pidFile:
      type: string
      description: Путь к PID-файлу (default: .agents/skills/watch-runner/.watcher.pid)
  required: []
```

## Реализация tools

- Реализуйте tools как обёртки над ctl.js:

```js
// pseudo-code for tool binding
tool('watch.start', async ({ docs = './docs', src = './kanban-cli', interval = 1500, duration, pidFile, logFile }) => {
  const args = ['.agents/skills/watch-runner/src/ctl.js', 'start', '--docs', docs, '--src', src, '--interval', String(interval)];
  if (duration) args.push('--duration', String(duration));
  if (pidFile) args.push('--pidFile', pidFile);
  return runNode(args);
});

tool('watch.stop', async ({ pidFile }) => runNode(['.agents/skills/watch-runner/src/ctl.js', 'stop', '--pidFile', pidFile].filter(Boolean)));

tool('watch.status', async ({ pidFile }) => runNode(['.agents/skills/watch-runner/src/ctl.js', 'status', '--pidFile', pidFile].filter(Boolean)));
```

## Запуск вручную

```bash
# Старт через контроллер
node .agents/skills/watch-runner/src/ctl.js start --docs ./docs --src ./kanban-cli --interval 1500 --duration 3000

# Проверка статуса
node .agents/skills/watch-runner/src/ctl.js status

# Остановка
node .agents/skills/watch-runner/src/ctl.js stop
```

## Ограничения

- Не устанавливает сторонние зависимости и не использует системные git hooks.
- Для `dotnet test` требуется установленный .NET SDK и наличие проектов/решения.

## Примечания для Оркестратора

- Оркестратор может запускать этот skill в отдельной задаче перед делегированием работ Code/Test агентам.
- Выводы вотчера должны возвращаться в контекст текущей сессии и использоваться для быстрой обратной связи.
