# Quickstart: Интегрированное решение

## Предварительные требования
- .NET SDK (версия как в проекте)
- Node.js + npm (для frontend)

## Сборка решения
```bash
cd kanban-cli
dotnet build Kanban.sln
```

## Запуск тестов
```bash
cd kanban-cli
dotnet test Kanban.sln
```

## Запуск компонентов

### Kanban CLI (существующий)
```bash
cd kanban-cli
dotnet run --project src/Kanban.Cli -- <команда>
```

### Kanban.Api (из front-ветки)
```bash
cd kanban-cli
dotnet run --project src/Kanban.Api
```

### UserProfile.Api (из back-ветки)
```bash
cd kanban-cli
dotnet run --project src/UserProfile.Api
```

### Frontend (Vue)
```bash
cd frontend
npm install
npm run dev
```

## Проверка интеграции
После слияния должны присутствовать:
- Все проекты в Kanban.sln: Kanban.Domain, Kanban.Infrastructure, Kanban.Cli, Kanban.Tests, Kanban.Api, UserProfile.Domain, UserProfile.Infrastructure, UserProfile.Api
- Директория frontend/ с Vue приложением
- Все тесты проходят
- Решение собирается без ошибок
