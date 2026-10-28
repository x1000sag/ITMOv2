# Kanban Frontend (Vue 3 + Vite)

Веб-интерфейс для работы с Kanban-доской, совместимый с CLI версией.

## Установка

```bash
npm install
```

## Запуск в режиме разработки

```bash
npm run dev
```

Фронтенд будет доступен по адресу http://localhost:5173

## Сборка

```bash
npm run build
```

## Настройка API

По умолчанию фронтенд обращается к API по адресу `http://localhost:5000`. 
Можно настроить через переменную окружения `VITE_API_BASE_URL` в файле `.env`.
