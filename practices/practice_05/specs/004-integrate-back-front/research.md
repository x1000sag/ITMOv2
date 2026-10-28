# Research: Интеграция веток dev/practice_05-back и dev/practice_05-front

**Дата**: 2026-10-28  
**Цель**: Проанализировать отличия веток и определить стратегию слияния

## Анализ изменений по веткам

### dev/practice_05-back (коммит 89c3687 "feat: profiles")
**Добавляет функциональность UserProfile API:**
- Новые проекты: `UserProfile.Domain`, `UserProfile.Infrastructure`, `UserProfile.Api`
- Entity Framework Core для работы с SQLite (`userprofiles.db`)
- API endpoints для управления профилями пользователей
- Тесты: `UserProfileServiceTests.cs`, `ProfilesControllerIntegrationTests.cs`
- Спецификации: `specs/003-user-profile-db/`
- Модификация: `Kanban.sln`, `tests/Kanban.Tests/Kanban.Tests.csproj`

### dev/practice_05-front (коммиты fe65606, 8d5be2a)
**Добавляет Vue frontend и Kanban API:**
- Frontend приложение: `frontend/` (Vue 3 + Vite + TypeScript + Pinia)
- Новый проект: `Kanban.Api` (REST API для работы с доской Kanban)
- Модификация: `Kanban.sln`, `src/Kanban.Infrastructure/BoardRepository.cs`
- Спецификации: `specs/002-frontend-dev/`
- API контракт: `specs/002-frontend-dev/contracts/api.yaml`

## Анализ конфликтов

### Критические конфликты
1. **Kanban.sln** - ОБЯЗАТЕЛЬНЫЙ КОНФЛИКТ
   - Обе ветки изменяют файл с разной структурой
   - back: добавляет UserProfile.* проекты, добавляет SolutionFolder "src", меняет формат путей на backslashes, добавляет платформы x64/x86
   - front: добавляет Kanban.Api проект, добавляет SolutionFolder "src", меняет формат путей на backslashes, добавляет платформы x64/x86
   - Решение: объединить все проекты из обеих веток, сохранить единую структуру

2. **tests/Kanban.Tests/Kanban.Tests.csproj** - возможный конфликт
   - back ветка модифицирует (добавляет зависимости для тестов UserProfile)
   - front ветка не трогает этот файл
   - Решение: применить изменения из back ветки

### Потенциальные конфликты
3. **src/Kanban.Infrastructure/BoardRepository.cs** - front ветка модифицирует
   - back ветка не трогает
   - Решение: применить изменения из front ветки

## Стратегия слияния

**Рекомендуемый порядок**: Слить сначала `dev/practice_05-back`, затем `dev/practice_05-front` (или наоборот, но нужно будет разрешить конфликт в Kanban.sln вручную в обоих случаях).

**Альтернативный подход**: Создать merge commit с разрешением конфликта Kanban.sln путем объединения всех проектов.

**План разрешения**:
1. Попробовать merge dev/practice_05-back в текущую ветку integration/practice_05
2. Разрешить конфликты (Kanban.sln, возможно Kanban.Tests.csproj)
3. Закоммитить merge
4. Попробовать merge dev/practice_05-front
5. Разрешить конфликты в Kanban.sln (объединить с уже добавленными UserProfile проектами)
6. Закоммитить merge
7. Собрать решение и запустить тесты

**Решение для Kanban.sln**: Создать объединенную версию со всеми проектами:
- Существующие: Kanban.Domain, Kanban.Infrastructure, Kanban.Cli, Kanban.Tests
- Из front: Kanban.Api
- Из back: UserProfile.Domain, UserProfile.Infrastructure, UserProfile.Api
- SolutionFolder "src" должен содержать Kanban.Api и UserProfile.* проекты
- Сохранить формат из веток (backslashes, платформы x64/x86)

## Результаты анализа

**Общий объем изменений:**
- Back: ~35 файлов (3 проекта + тесты + спецификации)
- Front: ~40 файлов (frontend/ + Kanban.Api + спецификации)
- Конфликтов: минимум 1 критический (Kanban.sln)

**Совместимость:** Обе фичи независимы, не конфликтуют по функциональности. UserProfile и Kanban - разные домены.

**Риски:** Низкие. Конфликт только в файле решения.
