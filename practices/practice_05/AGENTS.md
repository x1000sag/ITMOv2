# AGENTS.md: Kanban (C# CLI + Vue)

Этот репозиторий использует OpenCode и Spec Kit. Агенты обязаны прочитать этот файл перед началом работы. Проект: Kanban — C# CLI с хранением в `data/board.json`; планируется frontend на Vue. Язык артефактов — русский.

## Правила Проекта

- Минимальные изменения: точечные правки и небольшие коммиты.
- Все артефакты (docs, specs, ADR, код) — на русском языке.
- Не добавляйте обратную совместимость без явной причины. Если неясно — задайте один уточняющий вопрос.
- Не коммитьте изменения без явной просьбы.
- Комментарии в коде — только там, где это экономит время чтения.
- Фиксируйте результаты (изменённые файлы, команды, вывод) в сессии.

## Spec-Driven Workflow (Spec Kit)

Источник истины по процессу — `.specify/memory/constitution.md`. Команды Spec Kit — в `.opencode/commands/speckit.*.md` (спец-команды OpenCode).

Основной цикл для новой функциональности:

1. `/speckit.constitution` — зафиксировать/обновить принципы проекта.
2. `/speckit.specify` — создать спецификацию в `specs/NNN-feature-name/spec.md`.
3. `/speckit.clarify` — устранить неоднозначности до планирования.
4. `/speckit.plan` — получить `specs/NNN-feature-name/plan.md`.
5. `/speckit.tasks` — получить `specs/NNN-feature-name/tasks.md`.
6. `/speckit.implement` — реализовать задачи по порядку.
7. `/speckit.analyze`, `/speckit.checklist` — проверить согласованность артефактов.

Правила:

- Не начинайте реализацию фичи без `spec.md` и `tasks.md`; мелкие исправления и документацию можно делать напрямую.
- Изменения кода должны соответствовать `plan.md` и задачам из `tasks.md`.
- При изменении поведения обновляйте `docs/` и ADR, чтобы артефакты не расходились.

## Skills как инструменты

Skills лежат в `.agents/skills/<name>/SKILL.md` и доступны агенту через skill tool. Не загружайте все skills сразу: определите, что для задачи нужен соответствующий skill, и прочитайте его перед работой.

| Skill | Когда использовать |
| --- | --- |
| `csharp-docs` | Добавление/правка XML-документации публичного C# API. |
| `dotnet-best-practices` | Изменение .NET/C# кода, ревью структуры решения. |
| `frontend-design` | Создание или переработка Vue UI: палитра, типографика, layout. |
| `find-skills` | Нужной возможности нет в проекте — поиск и установка внешнего skill. |

Если для запуска тестов доступен skill `run-tests` — используйте его для подбора точных команд `dotnet test`. Если skill недоступен, запускайте `dotnet test` напрямую.

## MCP

- `context7` — внешняя документация по C#/.NET, Vue и используемым библиотекам. Используйте для проверки актуального синтаксиса и API.
- `mcp-docs-ts` — локальный RAG по `./docs` (`docs.list`, `docs.read`, `docs.search`), если подключён. Если MCP недоступен, читайте файлы `./docs` напрямую; блокировать задачу из-за этого не нужно.

MCP — вспомогательный источник. Локальные файлы репозитория и официальная документация приоритетнее предположений.

## Стек и Команды

- Backend: C#/.NET, solution `kanban-cli/Kanban.sln` (проекты `src/Kanban.Cli`, `src/Kanban.Domain`, `src/Kanban.Infrastructure`, `tests/Kanban.Tests`).
- Хранение: один файл `data/board.json`, запись атомарная.
- Frontend (планируется): Vue 3 + Vite + TypeScript в каталоге `frontend/`; для UI используйте `frontend-design`, для новых зависимостей — существующие конвенции `frontend/package.json`.
- Сборка: `dotnet build kanban-cli/Kanban.sln`.
- Тесты: `dotnet test kanban-cli/Kanban.sln` (или точная команда из skill `run-tests`).
- Запуск CLI: `dotnet run --project kanban-cli/src/Kanban.Cli -- <команда>`.

Для frontend используйте скрипты из `frontend/package.json` (`npm run dev`, `npm run build`, `npm run test`), если каталог уже существует. Не создавайте frontend-инфраструктуру вне рамок соответствующей спецификации.

## Навигация

- Конституция проекта: `.specify/memory/constitution.md`
- Спецификации фич: `specs/NNN-feature-name/` (`spec.md`, `plan.md`, `tasks.md`)
- Продуктовые требования: `docs/product-requirements.md`
- Пользовательские сценарии: `docs/user-flows.md`
- Термины: `docs/terms.md`
- ADR: `docs/adr/` (`adr-0001-domain-classes.md`, `adr-0002-cli-ui.md`)

## Минимальный Контекст

- Основные сущности: Board, Column, Card; карточки с тегами и дедлайнами; перемещение между колонками.
- Один локальный пользователь, без синхронизации и множественных досок.
- CLI возвращает ненулевой код при ошибке и понятное сообщение.
- Frontend на Vue должен работать с тем же доменом и форматом данных, что и CLI.
