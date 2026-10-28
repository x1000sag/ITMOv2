# Implementation Plan: Разработка фронтенда (Vue)

**Branch**: `002-frontend-dev` | **Date**: 2026-10-28 | **Spec**: [specs/002-frontend-dev/spec.md](spec.md)

**Input**: Feature specification from `/specs/002-frontend-dev/spec.md`

## Summary

Создание веб-интерфейса на Vue 3 + Vite + TypeScript для работы с Kanban-доской. Фронтенд должен отображать доску в формате, совместимом с CLI, работать с тем же файлом `data/board.json` и обеспечивать создание, редактирование, перемещение карточек. Необходимо обеспечить 100% совместимость формата данных между CLI и фронтендом.

## Technical Context

**Language/Version**: TypeScript 5.x, Vue 3

**Primary Dependencies**: Vue 3, Vite, TypeScript

**Storage**: Файл `data/board.json` (как в CLI). Для работы с файловой системой в браузере потребуется API-слой в решении kanban-cli.

**Testing**: Будет определено в процессе (возможно, Vitest)

**Target Platform**: Web (браузер)

**Project Type**: Web application (frontend)

**Performance Goals**: Базовые требования для веб-приложения, без специфичных метрик производительности

**Constraints**: Должна сохраняться совместимость с форматом данных CLI, хранение в том же `data/board.json`

**Scale/Scope**: Однопользовательское приложение, одна доска

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Принцип I (Простота и минимализм)**: Фронтенд реализует только необходимый функционал согласно спецификации - просмотр, создание, редактирование, перемещение карточек. Соответствует.
- **Принцип II (Консистентность доменной модели)**: Необходимо сохранить модель Board/Column/Card, соблюсти все правила (уникальность имен колонок, уникальность тегов, перемещение без позиционирования). Соответствует.
- **Принцип III (Тестируемость)**: Будет рассмотрено при планировании тестирования.
- **Принцип IV (Атомарность хранения)**: Критично - сохранение должно быть атомарным как в CLI. Требуется проработать подход к атомарной записи из веб-приложения.
- **Принцип V (Понятность интерфейса)**: Интерфейс на русском языке, соответствует конвенциям проекта.
- **Принцип VI (Эволюционируемая архитектура)**: Фронтенд в `frontend/` согласно стеку конституции, модель данных совместима. Соответствует.

Все принципы соблюдаются. Особое внимание - принцип IV (атомарность хранения при работе из браузера).

## Project Structure

### Documentation (this feature)

```text
specs/002-frontend-dev/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output
```

### Source Code (repository root)

```text
kanban-cli/
├── src/
│   ├── Kanban.Domain/       # Доменная модель
│   ├── Kanban.Infrastructure/ # Хранение данных
│   ├── Kanban.Cli/           # CLI
│   └── Kanban.Api/          # Web API для фронтенда (новый проект)
└── tests/

frontend/
├── src/
│   ├── components/      # Vue компоненты
│   ├── views/           # Представления/страницы
│   ├── stores/          # Хранилище состояния
│   ├── services/        # Сервисы для работы с API
│   ├── types/           # TypeScript типы (совместимые с доменной моделью)
│   ├── utils/           # Утилиты
│   ├── App.vue
│   └── main.ts
├── public/
├── index.html
├── package.json
├── vite.config.ts
├── tsconfig.json
└── README.md
```

**Structure Decision**: Создать веб-приложение в каталоге `frontend/` как указано в конституции и спецификации. Структура основана на стандартной организации Vue 3 + Vite проекта с разделением на компоненты, сервисы для работы с данными.

## Complexity Tracking

Нет нарушений конституции, требующих обоснования.
