# FadeAfro — Backend

> ASP.NET Core бэкенд для **FadeAfro** — Telegram Mini App для записи клиентов к мастерам барбершопа.

---

## Содержание

1. [Общее описание](#1-общее-описание)
2. [Требования](#2-требования)
3. [Технологии](#3-технологии)
4. [Архитектура](#4-архитектура)
5. [Контроллеры и эндпоинты](#5-контроллеры-и-эндпоинты)
6. [Авторизация](#6-авторизация)
7. [Роли](#7-роли)
8. [Безопасность](#8-безопасность)
9. [Деплой](#9-деплой)

---

## 1. Общее описание

Бэкенд обеспечивает логику записи клиентов к мастерам, управления расписанием и уведомлениями. Аутентификация построена на валидации Telegram `initData` по HMAC-SHA256 — пользователь не вводит пароль, вход происходит через Telegram.

Реализовано:
- аутентификация через Telegram Mini App initData
- выдача JWT-токена
- управление мастерами, расписанием, недоступностью и услугами
- запись клиентов и отмена записей
- уведомления
- загрузка и раздача фото мастеров
- health check эндпоинт
- rate limiting на ключевые эндпоинты
- структурированное логирование запросов (Serilog)
- автоматическое применение миграций при старте

---

## 2. Требования

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL 17+

**Быстрый старт через Docker:**

```bash
docker run -d --name fadeafro-postgres \
  -e POSTGRES_DB=fadeafro \
  -e POSTGRES_USER=fadeafro \
  -e POSTGRES_PASSWORD=fadeafro \
  -p 5432:5432 postgres:17
```

---

## 3. Технологии

| Компонент        | Технология                         |
|------------------|------------------------------------|
| Фреймворк        | ASP.NET Core 10                    |
| База данных      | PostgreSQL + Entity Framework Core |
| CQRS             | MediatR                            |
| Аутентификация   | JWT Bearer + Telegram initData     |
| Логирование      | Serilog                            |
| Документация API | Swagger / Swashbuckle              |
| Rate limiting    | ASP.NET Core RateLimiter           |

---

## 4. Архитектура

Проект построен по **Clean Architecture + DDD + CQRS** и разбит на 5 слоёв:

```
FadeAfro/
├── FadeAfro/                 # API-слой — контроллеры, middleware, Program.cs
├── FadeAfro.Application/     # Use cases — команды, запросы, хендлеры (MediatR)
├── FadeAfro.Domain/          # Домен — сущности, исключения, интерфейсы репозиториев
├── FadeAfro.Infrastructure/  # Инфраструктура — EF Core, JWT, репозитории, health checks
└── FadeAfro.Tests/           # Unit-тесты (xUnit + NSubstitute + FluentAssertions)
```

Жизненный цикл запроса:

```
Controller → MediatR → Handler → Repository → PostgreSQL
```

### Конфигурация

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=...;Database=fadeafro;Username=...;Password=..."
  },
  "Jwt": {
    "SecretKey": "...",
    "Issuer": "fadeafro",
    "Audience": "fadeafro",
    "ExpirationMinutes": 60
  },
  "Telegram": {
    "BotToken": "..."
  },
  "Time": {
    "TimeZone": "Europe/Moscow"
  },
  "Cors": {
    "AllowedOrigins": []
  }
}
```

Через переменные окружения: `Jwt__SecretKey`, `Telegram__BotToken`, `ConnectionStrings__Postgres`.

> **JWT Key** — произвольная строка, UTF-8-представление которой должно быть не менее 32 байт. Пример генерации:
> ```bash
> openssl rand -hex 32
> ```

### Запуск

```bash
dotnet run --project FadeAfro
```

| | URL |
|-|-----|
| API | `http://localhost:5000` |
| Swagger | `http://localhost:5000/swagger` |

### Миграции

Применяются автоматически при старте приложения.

Ручное создание:
```bash
dotnet ef migrations add <Name> --project FadeAfro.Infrastructure --startup-project FadeAfro
```

---

## 5. Контроллеры и эндпоинты

### AuthController — `/api/auth`

| Метод | Путь | Доступ | Rate limit |
|-------|------|--------|------------|
| `POST` | `/api/auth/login` | Публичный | 10 / мин (по IP) |

---

### UsersController — `/api/users`

| Метод | Путь | Роль |
|-------|------|------|
| `GET` | `/api/users/get/all` | Owner |
| `GET` | `/api/users/get/masters` | Owner |
| `GET` | `/api/users/get/owners` | Owner |
| `GET` | `/api/users/get/me` | Authorized |
| `PUT` | `/api/users/update/me/full-name` | Authorized |

---

### MasterProfilesController — `/api/master-profiles`

| Метод | Путь | Роль | Rate limit |
|-------|------|------|------------|
| `GET` | `/api/master-profiles/get/all` | Authorized | — |
| `GET` | `/api/master-profiles/get/{masterProfileId}` | Authorized | — |
| `GET` | `/api/master-profiles/get/me` | Master | — |
| `GET` | `/api/master-profiles/get/{masterProfileId}/photo` | Публичный | 20 / мин (по IP) |
| `GET` | `/api/master-profiles/get/{masterProfileId}/day-availability` | Публичный | 20 / мин (по IP) |
| `POST` | `/api/master-profiles/upload/me/photo` | Master | 7 / мин (по userId) |
| `POST` | `/api/master-profiles/assign/{userId}` | Owner | 5 / мин (по userId) |
| `DELETE` | `/api/master-profiles/dismiss/{masterId}` | Owner | 5 / мин (по userId) |

---

### MasterServicesController — `/api/master-services`

| Метод | Путь | Роль | Rate limit |
|-------|------|------|------------|
| `GET` | `/api/master-services/get/{masterProfileId}` | Authorized | — |
| `POST` | `/api/master-services/add/me` | Master | 20 / мин (по userId) |
| `PUT` | `/api/master-services/update/me/{serviceId}` | Master | 20 / мин (по userId) |
| `DELETE` | `/api/master-services/delete/me/{serviceId}` | Master | 20 / мин (по userId) |

---

### MasterSchedulesController — `/api/master-schedules`

| Метод | Путь | Роль | Rate limit |
|-------|------|------|------------|
| `GET` | `/api/master-schedules/get/{masterProfileId}` | Authorized | — |
| `POST` | `/api/master-schedules/set/me` | Master | 30 / мин (по userId) |
| `DELETE` | `/api/master-schedules/delete/me/{scheduleId}` | Master | 30 / мин (по userId) |

---

### MasterUnavailabilitiesController — `/api/master-unavailabilities`

| Метод | Путь | Роль | Rate limit |
|-------|------|------|------------|
| `GET` | `/api/master-unavailabilities/get/{masterProfileId}` | Authorized | — |
| `POST` | `/api/master-unavailabilities/add/me` | Master | 20 / мин (по userId) |
| `DELETE` | `/api/master-unavailabilities/delete/me/{unavailabilityId}` | Master | 20 / мин (по userId) |

---

### AppointmentsController — `/api/appointments`

| Метод | Путь | Роль | Rate limit |
|-------|------|------|------------|
| `GET` | `/api/appointments/client/get/me/appointments` | Client | — |
| `GET` | `/api/appointments/master/get/me/appointments` | Master | — |
| `POST` | `/api/appointments/client/me/book` | Client | 5 / мин (по userId) |
| `PATCH` | `/api/appointments/client/cancel/me/{appointmentId}` | Client | 10 / мин (по userId) |
| `PATCH` | `/api/appointments/master/cancel/me/{appointmentId}` | Master | 10 / мин (по userId) |

---

### NotificationsController — `/api/notifications`

| Метод | Путь | Роль |
|-------|------|------|
| `GET` | `/api/notifications/get/me/count/unread` | Authorized |
| `GET` | `/api/notifications/get/me/unread` | Authorized |
| `PUT` | `/api/notifications/read/me/all` | Authorized |
| `PUT` | `/api/notifications/read/me/{notificationId}` | Authorized |

---

### HealthController — `/health`

| Метод | Путь | Доступ |
|-------|------|--------|
| `GET` | `/health` | Публичный |

**200 OK:**
```json
{ "database": "Healthy" }
```

**503 Service Unavailable:**
```json
{ "database": "Unhealthy" }
```

---

## 6. Авторизация

Аутентификация через **Telegram Mini App initData**. При логине клиент передаёт `initData` из Telegram SDK — бэкенд валидирует подпись HMAC-SHA256 с ботовым токеном и выдаёт JWT.

Токен передаётся в заголовке каждого последующего запроса:

```
Authorization: Bearer <token>
```

---

## 7. Роли

| Роль | Описание |
|------|----------|
| `Client` | Клиент — просматривает мастеров, записывается, отменяет записи |
| `Master` | Мастер — управляет расписанием, услугами, недоступностью |
| `Owner` | Владелец — назначает и снимает мастеров, видит всех пользователей |

Роль `Owner` назначается вручную в БД.

---

## 8. Безопасность

### Rate limiting

Ключ партиции — **IP** для анонимных эндпоинтов, **userId** для аутентифицированных. Лимиты указаны в таблицах выше. При превышении — `429 Too Many Requests`.

### Сообщения об ошибках

В **production** глобальный обработчик исключений возвращает обобщённое сообщение без деталей внутренней структуры. В **development** — полный текст исключения.

### Docker

Контейнер бэкенда запускается от имени непривилегированного пользователя `appuser` (не root).

---

## 9. Деплой

```bash
docker compose up -d --build
```

Миграции применяются автоматически при старте. Переменные окружения задаются в `.env`:

```
POSTGRES_DB=
POSTGRES_USER=
POSTGRES_PASSWORD=
JWT_SECRET_KEY=
TELEGRAM_BOT_TOKEN=
```
