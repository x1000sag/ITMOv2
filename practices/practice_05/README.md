# Kanban Project

## User Profile API (Feature 003)

Проект содержит API для управления пользовательскими профилями.

### Запуск

```bash
cd kanban-cli
dotnet run --project src/UserProfile.Api
```

API будет доступен по адресу: http://localhost:5000 (или порту, указанному в настройках)

### Endpoints

- POST /profiles - Создание профиля
- GET /profiles/{id} - Получение профиля по ID
- PUT /profiles/{id} - Обновление профиля

### Swagger

Swagger документация доступна по адресу: /swagger при запуске в Development режиме.

### Модель UserProfile

- Id (int)
- FirstName (string, опционально)
- LastName (string, опционально)
- Email (string, обязательный, валидация email)
- BirthDate (DateOnly?, опционально)
- PhotoUrl (string?, опционально, валидация URL)
