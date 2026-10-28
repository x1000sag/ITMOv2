# Tasks: Разработка фронтенда (Vue)

**Input**: Design documents from `/specs/002-frontend-dev/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Tests are OPTIONAL - not explicitly requested in the feature specification.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Backend API**: `kanban-cli/src/Kanban.Api/`
- **Frontend**: `frontend/src/`
- **Data**: `data/board.json` (shared with CLI)
- **Shared types/contracts**: follow existing project conventions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [ ] T001 Create frontend project structure with Vite + Vue 3 + TypeScript in `frontend/` per implementation plan
- [ ] T002 [P] Initialize frontend dependencies (vue, vite, typescript) and configure `frontend/package.json`, `frontend/vite.config.ts`, `frontend/tsconfig.json`
- [ ] T003 Create backend API project structure `kanban-cli/src/Kanban.Api/` and add to solution `kanban-cli/Kanban.sln`
- [ ] T004 [P] Configure API project dependencies (ASP.NET Core) in `kanban-cli/src/Kanban.Api/Kanban.Api.csproj`
- [ ] T005 [P] Create base frontend directory structure (`frontend/src/components/`, `frontend/src/views/`, `frontend/src/stores/`, `frontend/src/services/`, `frontend/src/types/`, `frontend/src/utils/`)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T006 Define TypeScript types for Board/Column/Card matching C# models in `frontend/src/types/board.ts` (id as string UUID, tags string[], deadline string|null YYYY-MM-DD, createdAt string ISO8601 UTC)
- [ ] T007 [P] Create API client service for board operations in `frontend/src/services/boardApi.ts` (GET/PUT /api/board)
- [ ] T008 Set up ASP.NET Core API project in `kanban-cli/src/Kanban.Api/Program.cs` with CORS for frontend origin
- [ ] T009 Implement Board API controller/endpoints (GET /api/board, PUT /api/board) in `kanban-cli/src/Kanban.Api/Controllers/BoardController.cs`
- [ ] T010 Implement board service/repository access in API using existing `Kanban.Infrastructure.BoardRepository` logic to ensure atomic writes (temp file + move) as in CLI
- [ ] T011 [P] Configure API to read/write `data/board.json` relative to solution root with same format as CLI
- [ ] T012 Add API project reference to `Kanban.Infrastructure` and `Kanban.Domain` in `kanban-cli/src/Kanban.Api/Kanban.Api.csproj`

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Просмотр доски Kanban (Priority: P1) 🎯 MVP

**Goal**: Пользователь открывает веб-интерфейс и видит доску с колонками и карточками в том же формате, что и в CLI

**Independent Test**: Открыть фронтенд и убедиться, что отображаются колонки ToDo/InProgress/Done и карточки из data/board.json

### Implementation for User Story 1

- [ ] T013 [P] [US1] Create Board view/component structure in `frontend/src/views/BoardView.vue`
- [ ] T014 [P] [US1] Create Column component to display column name and cards in `frontend/src/components/Column.vue`
- [ ] T015 [P] [US1] Create Card component to display card info (title, description, tags, deadline) in `frontend/src/components/Card.vue`
- [ ] T016 [US1] Implement board state management (load board from API) in `frontend/src/stores/boardStore.ts` using Pinia or Composition API
- [ ] T017 [US1] Wire BoardView to load and display board from API on mount in `frontend/src/views/BoardView.vue`
- [ ] T018 [US1] Style components minimally in Russian, matching project conventions
- [ ] T019 [US1] Handle loading and error states when fetching board in `frontend/src/views/BoardView.vue`

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 3 - Сохранение данных (Priority: P1)

**Goal**: Изменения, внесенные через веб-интерфейс, должны сохраняться в том же формате data/board.json, что использует CLI

**Independent Test**: Внести изменения через фронтенд и проверить, что CLI видит эти изменения и корректно с ними работает

### Implementation for User Story 3

- [ ] T020 [P] [US3] Add saveBoard method to board store calling API PUT /api/board in `frontend/src/stores/boardStore.ts`
- [ ] T021 [US3] Ensure API PUT endpoint validates and atomically writes board to `data/board.json` (reuse BoardRepository atomic write logic) in `kanban-cli/src/Kanban.Api/Controllers/BoardController.cs`
- [ ] T022 [US3] Add error handling for save failures in API and frontend in `frontend/src/stores/boardStore.ts` and `kanban-cli/src/Kanban.Api/Controllers/BoardController.cs`
- [ ] T023 [US3] Verify data format compatibility (JSON serialization matches CLI format) between frontend types and API models

**Checkpoint**: At this point, User Stories 1 AND 3 should work together (view and save)

---

## Phase 5: User Story 2 - Взаимодействие с карточками (Priority: P2)

**Goal**: Пользователь может просматривать детали карточки, создавать, редактировать и перемещать карточки между колонками через веб-интерфейс

**Independent Test**: Создать карточку, отредактировать её и переместить между колонками

### Implementation for User Story 2

- [ ] T024 [P] [US2] Create CardModal/Edit component for creating/editing cards in `frontend/src/components/CardModal.vue` (fields: title, description, tags, deadline)
- [ ] T025 [P] [US2] Add card creation functionality - button to add card per column in `frontend/src/components/Column.vue`
- [ ] T026 [US2] Implement card creation logic in board store (generate UUID, set createdAt UTC) in `frontend/src/stores/boardStore.ts`
- [ ] T027 [US2] Implement card editing logic in board store in `frontend/src/stores/boardStore.ts`
- [ ] T028 [US2] Implement drag and drop for moving cards between columns using HTML5 Drag and Drop API in `frontend/src/components/Column.vue` and `frontend/src/components/Card.vue`
- [ ] T029 [US2] Update board state on card move and trigger save in `frontend/src/stores/boardStore.ts`
- [ ] T030 [US2] Add validation (unique tags per card, required title) in frontend in `frontend/src/components/CardModal.vue`
- [ ] T031 [US2] Ensure moved cards maintain all data and save atomically via API

**Checkpoint**: All user stories should now be independently functional

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T032 [P] Add basic styling and ensure Russian UI language throughout frontend
- [ ] T033 [P] Update frontend README with setup/run instructions in `frontend/README.md`
- [ ] T034 Add API documentation/README in `kanban-cli/src/Kanban.Api/README.md`
- [ ] T035 [P] Verify quickstart scenarios work end-to-end (view, create, edit, move, CLI compatibility)
- [ ] T036 Code cleanup and ensure TypeScript types are consistent across frontend
- [ ] T037 Run build checks: `npm run build` in frontend and `dotnet build kanban-cli/Kanban.sln`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - US1 (P1) and US3 (P1) are both high priority; US3 depends on save mechanism which needs API ready (foundational) - can be done after foundational, US1 first then US3 or in parallel if careful
  - US2 (P2) depends on US1/US3 infrastructure being ready
- **Polish (Final Phase)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 3 (P1)**: Can start after Foundational (Phase 2) - Needs API save endpoint (T009/T010) ready; integrates with board state
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - Needs board display and save mechanism working

### Within Each User Story

- Models/types before components
- Services/stores before UI components
- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- Once Foundational is complete, UI components for US1 marked [P] (T013, T014, T015) can run in parallel
- CardModal and column add button for US2 marked [P] (T024, T025) can run in parallel
- Polish tasks marked [P] can run in parallel

---

## Parallel Example: User Story 1

```bash
# Launch all UI components for User Story 1 together:
Task: "Create Board view/component structure in frontend/src/views/BoardView.vue"
Task: "Create Column component in frontend/src/components/Column.vue"
Task: "Create Card component in frontend/src/components/Card.vue"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Test User Story 1 independently - board displays from data/board.json
5. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → Foundation ready (API + types + frontend structure)
2. Add User Story 1 → Test independently → Display works (MVP foundation)
3. Add User Story 3 → Test independently → Saving works, CLI compatibility verified
4. Add User Story 2 → Test independently → Full CRUD + drag & drop
5. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:
1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (components + store loading)
   - Developer B: User Story 3 (API save + store save)
   - Developer C: User Story 2 (modals + drag & drop)
3. Stories integrate and test independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Must maintain 100% compatibility with CLI data format (data/board.json)
- API must use atomic writes like CLI (temp file + move)
- Frontend in Russian, follow existing project conventions
- All file paths are exact as specified in plan
- Avoid cross-story dependencies that break independence
