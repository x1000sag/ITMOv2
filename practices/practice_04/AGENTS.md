# OpenCode Agents: Правила и Оркестрация

Этот репозиторий использует среду OpenCode. Агентам запрещено делать предположения: они обязаны читать этот файл перед началом работы и следовать описанным процедурам. Проект: Kanban CLI. Язык артефактов — русский.

## Правила Проекта

- Минимальные изменения: делайте точечные правки и небольшие коммиты.
- Все артефакты (docs, ADR, код) — на русском языке.
- Агенты и субагенты обязаны:
  - читать этот AGENTS.md перед началом работы;
  - подключать и использовать доступные skills;
  - использовать MCP как источник истинности для локальной документации (./docs) и внешней справки по C#;
  - фиксировать результаты (изменённые файлы, команды, вывод) в сессии.
- Не добавляйте обратную совместимость без явной причины. Если неясно — задайте один уточняющий вопрос.

## Роли Агента

- Orchestrator: формирует план работ, декомпозирует задачи и делегирует их субагентам. Гарантирует подключение skills и MCP.
- Docs/Spec Agent: поддерживает актуальность документации и ADR (./docs, ./docs/adr).
- Code Agent: реализует функциональность Kanban CLI согласно ADR.
- Test Agent: запускает сборку/тесты и возвращает результат в сессию.

## Навигация по Документации

- Продуктовые требования: `./docs/product-requirements.md`
- Пользовательские сценарии: `./docs/user-flows.md`
- Термины: `./docs/terms.md`
- ADR: `./docs/adr/` (например, `adr-0001-domain-classes.md`, `adr-0002-cli-ui.md`)

## Подключения

- Skills:
  - find-skills: `.agents/skills/find-skills/` — поиск и подключение skills из экосистемы skills.sh.
  - dotnet-runner (опционально): skill для `dotnet build/test/run` (если отсутствует — Orchestrator может задать задачу Code Agent на создание локального skill).
  - watch-runner: `.agents/skills/watch-runner/` — агентный вотчер/раннер, реагирующий на изменения файлов и запускающий проверки. Не является git hook; используется агентами в сессии.
- MCP:
  - Локальный MCP (TypeScript) для RAG по `./docs` — `servers/mcp-docs-ts` (интерфейсы: `docs.list`, `docs.read`, `docs.search`).
  - Внешний MCP по C#: `context7` — официальная документация C#/.NET (https://learn.microsoft.com/ru-ru/dotnet/csharp/).

## Оркестрация: Точные Команды Запуска Субагентов

Управление субагентами выполняется через Task tool. Orchestrator обязан вызывать subagent с правильным типом и чётким prompt.

1. Docs/Spec Agent

Команда (Task tool):

```
task {
  "description": "Актуализация документации и ADR",
  "subagent_type": "docs-spec",
  "prompt": "Перед началом ЗАГРУЗИ skills: find-skills. ПОДКЛЮЧИ MCP: локальный mcp-docs-ts и внешний context7. Для чтения и поиска по ./docs используй ТОЛЬКО инструменты локального MCP: docs.list, docs.read, docs.search. Обнови/создай файлы в ./docs по ADR и требованиям. Следуй правилам: минимальные изменения, русский язык, используй apply_patch."
}
```

Рекомендуемый порядок действий в prompt:
- вызвать tools локального MCP: `docs.list` для инвентаризации;
- через `docs.read`/`docs.search` проверить `./docs/product-requirements.md`, `./docs/user-flows.md`, `./docs/terms.md`;
- обновить/создать ADR в `./docs/adr/`;
- валидировать ссылки между файлами.

2. Code Agent

Команда (Task tool):

```
task {
  "description": "Реализация кода согласно ADR",
  "subagent_type": "code",
  "prompt": "Перед началом ЗАГРУЗИ skills: find-skills, dotnet-runner. ПОДКЛЮЧИ MCP: локальный mcp-docs-ts для ./docs и внешний context7 для справки по C#. Используй локальный MCP для чтения ADR/требований (docs.read/docs.search). Для сборки/тестов используй ТОЛЬКО инструменты skill dotnet-runner (build/test/run), не вызывай dotnet напрямую. Реализуй функциональность Kanban CLI по ADR. Минимальные изменения, используй apply_patch, язык C#. Храни данные в data/board.json."
}
```

Рекомендуемый порядок действий в prompt:
- получить контекст ADR через локальный MCP (docs.read ./docs/adr/...);
- реализовать доменные классы/CLI по ADR;
- обеспечить сериализацию в `data/board.json`;
- запустить `dotnet-runner.build` и `dotnet-runner.test`;
- подготовить минимальные тесты.

3. Test Agent

Команда (Task tool):

```
task {
  "description": "Сборка и тестирование .NET проекта",
  "subagent_type": "test",
  "prompt": "Перед началом ЗАГРУЗИ skills: dotnet-runner, watch-runner. ПОДКЛЮЧИ MCP: локальный mcp-docs-ts (для чтения требований при необходимости). Выполни сборку/тесты через инструменты skill dotnet-runner (build/test). При необходимости запусти watch-runner.start для автопроверок и верни краткий отчёт/статус."
}
```

Примечания:
- При наличии skill `dotnet-runner` используйте его tools (build/test/run) вместо прямых вызовов.
- Если tests отсутствуют — вернуть предупреждение и предложить Code Agent добавить минимальные тесты.

## Обязательные Подключения Перед Запуском

Orchestrator должен:
1. ЯВНО загрузить skills: `find-skills`, `dotnet-runner`, `watch-runner`.
2. Подключить MCP: внешний `context7` (официальная документация C#/.NET) и локальный `mcp-docs-ts` для `./docs`.
3. Проверить доступность `./docs` и наличия ADR. Если локальный MCP недоступен — прервать задачу с сообщением и инструкцией по запуску `servers/mcp-docs-ts`.

### Запуск агентного hook (watch-runner)

- Оркестратор обязан запускать вотчер перед делегированием задач:

```
node .agents/skills/watch-runner/src/watcher.js --docs ./docs --src ./kanban-cli --interval 1500
```

- Вотчер не зависит от git и предназначен для интеграции в агентные сессии. При изменении исходников .NET он запускает `dotnet test` и возвращает вывод в stdout/stderr для агентов.

#### Рекомендуемый сценарий Orchestrator

- Перед делегированием задач запусти watcher контроллером:

```
node .agents/skills/watch-runner/src/ctl.js start --docs ./docs --src ./kanban-cli --interval 1500 --duration 120000
```

- Для проверки статуса:

```
node .agents/skills/watch-runner/src/ctl.js status
```

- Перед завершением сессии останови watcher:

```
node .agents/skills/watch-runner/src/ctl.js stop
```

- Если доступны tools навыка, используй их:
  - `watch.start` (эквивалент стартовой команды)
  - `watch.status`
  - `watch.stop`

Рекомендованные вспомогательные prompt-фрагменты для загрузки источников:

```
В начале сессии: вызови skill tool для загрузки `find-skills`, `dotnet-runner`, `watch-runner`.
Используй MCP `context7` для уточнений по синтаксису C# и .NET CLI.
Используй локальный MCP `mcp-docs-ts` (tools: docs.list, docs.read, docs.search) для чтения ./docs.
```

## Политика Изменений

- Лучшие изменения — наименьшие корректные изменения.
- Не редактировать/не удалять чужие изменения без явного указания.
- Комментарии в коде добавлять только там, где это экономит время чтения.

## Минимальный Контекст

- Агенты — часть среды OpenCode, работают с локальной документацией и кодом.
- Источники истинности: локальные файлы `./docs` (через локальный MCP) и официальная документация C# (через `context7`).
- Основные сущности продукта: Board, Column, Card; минимальный CLI для работы с карточками, тегами и дедлайнами; хранение — `data/board.json`.
