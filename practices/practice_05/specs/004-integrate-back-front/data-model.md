# Data Model: Интегрированное решение

## Сущности Kanban (существующие)
См. оригинальную модель в домене Kanban:
- **Board** - доска с колонками
- **Column** - колонка с карточками  
- **Card** - карточка с id, title, description, tags, deadline, createdAt

## Сущности UserProfile (из dev/practice_05-back)
Из спецификации 003-user-profile-db:
- **UserProfile** - профиль пользователя
  - Id (Guid/long)
  - Имя, контактная информация и др. (согласно реализации)

Хранение: SQLite через Entity Framework Core в `UserProfile.Api/userprofiles.db`

## Взаимодействие
UserProfile и Kanban - независимые домены, не имеют связей в текущей реализации. Интеграция не требует объединения моделей.
