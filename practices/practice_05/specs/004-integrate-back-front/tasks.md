# Tasks: Интеграция веток dev/practice_05-back и dev/practice_05-front

**Input**: Design documents from `/specs/004-integrate-back-front/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, quickstart.md

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Backend projects**: `kanban-cli/src/`, `kanban-cli/tests/`
- **Solution**: `kanban-cli/Kanban.sln`
- **Frontend**: `frontend/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prepare environment for integration analysis

- [ ] T001 [P] Проверить текущую ветку и статус репозитория в `/mnt/d/working/code/ITMOv2/practices/practice_05/`
- [ ] T002 [P] Получить информацию о коммитах в ветках `dev/practice_05-back` и `dev/practice_05-front` относительно общей базы
- [ ] T003 [P] Сохранить текущие состояния файлов для возможного отката при необходимости

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core analysis infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T004 Проанализировать изменения в ветке `dev/practice_05-back` относительно текущей ветки или общей базы
- [ ] T005 Проанализировать изменения в ветке `dev/practice_05-front` относительно текущей ветки или общей базы
- [ ] T006 Сформировать сводный отчет о всех добавленных/измененных/удаленных файлах в обеих ветках

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Проанализировать изменения в ветках (Priority: P1) 🎯 MVP

**Goal**: Понять, какие файлы и функциональность реализованы в каждой ветке, чтобы оценить объем интеграции и потенциальные конфликты

**Independent Test**: Можно выполнить анализ структуры веток и получить отчет о изменениях

### Implementation for User Story 1

- [ ] T007 [US1] Выполнить `git diff --name-status <base>...dev/practice_05-back` для получения списка файлов back-ветки
- [ ] T008 [US1] Выполнить `git diff --name-status <base>...dev/practice_05-front` для получения списка файлов front-ветки
- [ ] T009 [US1] Выявить общие файлы, измененные в обеих ветках (приоритет - `kanban-cli/Kanban.sln`)
- [ ] T010 [US1] Проанализировать содержимое конфликтующих файлов в обеих ветках для понимания различий
- [ ] T011 [US1] Зафиксировать результаты анализа в `specs/004-integrate-back-front/analysis-report.md` (при необходимости)

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - Спланировать стратегию слияния (Priority: P1)

**Goal**: Получить план интеграции двух фич, чтобы выполнить merge безопасным и предсказуемым образом

**Independent Test**: Можно проверить наличие плана с конкретными шагами, списком конфликтов и решением по каждому

### Implementation for User Story 2

- [ ] T012 [US2] Определить порядок слияния на основе анализа (рекомендуется слить back первым, затем front)
- [ ] T013 [US2] Подготовить стратегию разрешения конфликта `kanban-cli/Kanban.sln` путем объединения всех проектов из обеих веток
- [ ] T014 [US2] Определить стратегию разрешения других потенциальных конфликтов (при наличии)
- [ ] T015 [US2] Сформировать пошаговый план слияния с командами git и проверками
- [ ] T016 [US2] Актуализировать `specs/004-integrate-back-front/research.md` при необходимости с финальной стратегией

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 3 - Выполнить интеграцию и проверить работоспособность (Priority: P1)

**Goal**: Слить обе ветки в integration/practice_05 и убедиться, что обе фичи работают вместе

**Independent Test**: Можно собрать решение и запустить тесты, убедившись что нет регрессий и обе фичи доступны

### Implementation for User Story 3

#### Merge dev/practice_05-back
- [ ] T017 [US3] Выполнить слияние `dev/practice_05-back` в текущую ветку `integration/practice_05` (`git merge dev/practice_05-back`)
- [ ] T018 [US3] Разрешить конфликты при слиянии (при наличии). Особое внимание на `kanban-cli/Kanban.sln`
- [ ] T019 [US3] Проверить, что добавлены проекты UserProfile.Domain, UserProfile.Infrastructure, UserProfile.Api
- [ ] T020 [US3] Закоммитить merge после разрешения конфликтов

#### Merge dev/practice_05-front
- [ ] T021 [US3] Выполнить слияние `dev/practice_05-front` в текущую ветку `integration/practice_05` (`git merge dev/practice_05-front`)
- [ ] T022 [US3] Разрешить конфликты в `kanban-cli/Kanban.sln` путем объединения со списком проектов из back-ветки (должны остаться UserProfile.* + добавиться Kanban.Api)
- [ ] T023 [US3] Проверить наличие директории `frontend/` и проекта `Kanban.Api` в solution
- [ ] T024 [US3] Закоммитить merge после разрешения конфликтов

#### Verification
- [ ] T025 [US3] Собрать решение: `cd kanban-cli && dotnet build Kanban.sln`
- [ ] T026 [US3] Запустить тесты: `cd kanban-cli && dotnet test Kanban.sln`
- [ ] T027 [US3] Проверить наличие всех проектов в solution: UserProfile.Domain, UserProfile.Infrastructure, UserProfile.Api, Kanban.Api, Kanban.Domain, Kanban.Infrastructure, Kanban.Cli, Kanban.Tests
- [ ] T028 [US3] Проверить наличие `frontend/` директории с Vue приложением
- [ ] T029 [US3] Зафиксировать успешность интеграции

**Checkpoint**: All user stories should now be independently functional

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T030 [P] Обновить `specs/004-integrate-back-front/quickstart.md` при необходимости для отражения итогового состояния
- [ ] T031 [P] Проверить соответствие всех артефактов требованиям AGENTS.md (язык - русский, минимальные изменения)
- [ ] T032 Выполнить финальную проверку согласно критериям успешности из spec.md
- [ ] T033 Подготовить краткий отчет о результатах интеграции

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - User stories can then proceed in priority order (P1, P1, P1)
- **Polish (Final Phase)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Can start after User Story 1 completion (needs analysis results)
- **User Story 3 (P1)**: Can start after User Story 2 completion (needs integration plan)

### Within Each User Story

- Analysis before planning
- Planning before merging
- Merges in order (back first, then front) with conflict resolution
- Build and test after each merge or after all merges
- Verification before marking complete

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- Some analysis tasks can be parallelized
- Documentation updates in Polish phase marked [P] can run in parallel

---

## Parallel Example: Setup

```bash
# Launch all setup tasks together:
Task: "Проверить текущую ветку и статус репозитория"
Task: "Получить информацию о коммитах в ветках"
Task: "Сохранить текущие состояния файлов"
```

---

## Implementation Strategy

### MVP First (All P1 Stories)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1 (анализ)
4. Complete Phase 4: User Story 2 (планирование)
5. Complete Phase 5: User Story 3 (интеграция и проверка)
6. **STOP and VALIDATE**: Все тесты проходят, обе фичи доступны
7. Complete Phase 6: Polish

### Sequential Strategy (Recommended)

Due to dependencies (analysis -> planning -> merging), this integration should be done sequentially rather than in parallel.

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Конфликт в `kanban-cli/Kanban.sln` - обязательный, требует ручного разрешения
- Слить `dev/practice_05-back` первым, затем `dev/practice_05-front`
- После слияния обязательно проверить сборку и тесты
- Минимальные изменения при разрешении конфликтов
- Следовать конвенциям проекта (русский язык артефактов)
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
