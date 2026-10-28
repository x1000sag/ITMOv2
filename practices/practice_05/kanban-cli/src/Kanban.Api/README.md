# Kanban API

Web API для работы с Kanban-доской из веб-интерфейса.

## Запуск

```bash
dotnet run --project src/Kanban.Api
```

API будет доступен по адресу http://localhost:5000

## Endpoints

- `GET /api/board` - Получить доску
- `PUT /api/board` - Сохранить доску

Данные сохраняются в файл `data/board.json` с атомарной записью (как в CLI). Формат данных полностью совместим с CLI.
