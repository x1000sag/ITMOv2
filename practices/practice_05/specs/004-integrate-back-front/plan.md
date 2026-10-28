# Implementation Plan: Интеграция веток dev/practice_05-back и dev/practice_05-front

**Branch**: `004-integrate-back-front` | **Date**: 2026-10-28 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/004-integrate-back-front/spec.md`

## Summary

Необходимо проанализировать и выполнить слияние двух независимых фич из веток `dev/practice_05-back` (UserProfile API - управление профилями пользователей) и `dev/practice_05-front` (Vue frontend + Kanban.Api) в текущую ветку `integration/practice_05`. Основной вызов - разрешение конфликта в `Kanban.sln`, который обе ветки модифицируют по-разному, а также обеспечение совместного существования всех проектов (Kanban.Api, UserProfile.* и существующих Kanban.Domain/Infrastructure/Cli/Tests).

## Technical Context

**Language/Version**: C# / .NET (как в проекте), TypeScript/Vue 3 для frontend  
**Primary Dependencies**: ASP.NET Core (Web API), Entity Framework Core (для UserProfile), Vue 3 + Vite + Pinia (для frontend)  
**Storage**: JSON файлы (существующий `data/board.json`), SQLite (для UserProfile - `userprofiles.db`)  
**Testing**: xUnit  
**Target Platform**: Linux/Windows (кроссплатформенный .NET)  
**Project Type**: Web application (backend API + frontend)  
**Performance Goals**: N/A (учебный проект)  
**Constraints**: Минимальные изменения при разрешении конфликтов, сохранить функциональность обеих фич  
**Scale/Scope**: Интеграция существующих реализаций, без новой функциональности

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Согласно конституции проекта Kanban:
- **I. Простота и минимализм** - интеграция существующих фич, без добавления нового функционала ✓
- **II. Консистентность доменной модели** - обе фичи используют существующую модель Board/Column/Card, добавляют новые сущности UserProfile независимо ✓
- **III. Тестируемость** - обе ветки добавляют тесты ✓
- **IV. Атомарность хранения** - не затрагивает механику хранения Kanban ✓
- **V. Понятность интерфейса** - API проекты следуют конвенциям ✓
- **VI. Эволюционируемая архитектура** - соответствует направлению развития (web API + frontend) ✓

Все принципы соблюдаются.

## Project Structure

### Documentation (this feature)

```text
specs/004-integrate-back-front/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output  
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (repository root)

```text
kanban-cli/
├── Kanban.sln           # Конфликт - требуется разрешение
├── src/
│   ├── Kanban.Domain/
│   ├── Kanban.Infrastructure/
│   ├── Kanban.Cli/
│   ├── Kanban.Api/      # из dev/practice_05-front
│   ├── UserProfile.Domain/      # из dev/practice_05-back
│   ├── UserProfile.Infrastructure/ # из dev/practice_05-back
│   └── UserProfile.Api/         # из dev/practice_05-back
└── tests/
    └── Kanban.Tests/    # добавляет тесты из обеих веток

frontend/                 # из dev/practice_05-front (Vue 3 + Vite)
└── src/
    ├── components/
    ├── views/
    ├── stores/
    ├── services/
    └── types/
```

**Structure Decision**: Объединенная структура - все проекты из обеих веток сосуществуют в solution, frontend добавляется в корне репозитория как в ветке front.

## Complexity Tracking

> No constitution violations - integration only

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| N/A | N/A | N/A |
