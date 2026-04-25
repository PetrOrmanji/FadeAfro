# FadeAfro Backend

Бэкенд барбершопа FadeAfro. Telegram Mini App — клиенты записываются к мастерам, мастера управляют расписанием и записями.

## Архитектура

Clean Architecture + DDD + CQRS (MediatR). Четыре слоя:

```
FadeAfro.Domain          — сущности, исключения, интерфейсы репозиториев
FadeAfro.Application     — use cases (команды/запросы через MediatR)
FadeAfro.Infrastructure  — EF Core + PostgreSQL, JWT, репозитории
FadeAfro          — ASP.NET Web API (контроллеры, middleware, DI)
FadeAfro.Tests           — unit тесты (xUnit + NSubstitute + FluentAssertions)
```

## Стек

- .NET 10, ASP.NET Core Web API
- PostgreSQL + EF Core 10 (Npgsql)
- MediatR (CQRS)
- JWT Bearer аутентификация
- Telegram initData HMAC-SHA256 валидация
- Swashbuckle 10.x (Swagger + JWT)
- xUnit, NSubstitute, FluentAssertions

## Запуск

```bash
# Тесты
dotnet test

# API
dotnet run --project FadeAfro/FadeAfro.csproj
```

## Конфигурация (appsettings.json / переменные окружения)

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
  }
}
```

Через env: `Jwt__SecretKey`, `Telegram__BotToken`, `ConnectionStrings__Postgres`.

## Docker

```bash
docker build -t fadeafro .
docker run -p 8080:8080 -e Jwt__SecretKey=... -e Telegram__BotToken=... fadeafro
```

## Роли и авторизация

| Роль    | Описание                          |
|---------|-----------------------------------|
| Client  | Клиент — записывается к мастеру   |
| Master  | Мастер — управляет расписанием    |
| Owner   | Владелец — полный доступ          |

Константы: `FadeAfro.Domain.Constants.Roles`.

## API endpoints

| Метод  | Маршрут                                    | Роль            |
|--------|--------------------------------------------|-----------------|
| POST   | /api/auth/login                            | Anonymous       |
| POST   | /api/users/register                        | Anonymous       |
| GET    | /api/users/get/{telegramId}               | Owner           |
| POST   | /api/master-profiles/create               | Owner           |
| GET    | /api/master-profiles/get/{id}             | Authorized      |
| GET    | /api/master-profiles/all                  | Authorized      |
| PUT    | /api/master-profiles/update/{id}          | Master, Owner   |
| GET    | /api/master-profiles/available-slots/{id} | Authorized      |
| POST   | /api/services/add                         | Master, Owner   |
| GET    | /api/services/get/{masterProfileId}       | Authorized      |
| PUT    | /api/services/update/{serviceId}          | Master, Owner   |
| DELETE | /api/services/delete/{serviceId}          | Master, Owner   |
| POST   | /api/master-schedules/set                 | Master, Owner   |
| GET    | /api/master-schedules/get/{masterProfileId}| Authorized     |
| DELETE | /api/master-schedules/delete/{scheduleId} | Master, Owner   |
| POST   | /api/master-unavailabilities/add          | Master, Owner   |
| GET    | /api/master-unavailabilities/get/{masterProfileId}| Authorized|
| DELETE | /api/master-unavailabilities/delete/{id}  | Master, Owner   |
| POST   | /api/appointments/create                  | Client          |
| GET    | /api/appointments/get/client/{clientId}   | Client, Owner   |
| GET    | /api/appointments/get/master/{profileId}  | Master, Owner   |
| PATCH  | /api/appointments/cancel-by-client/{id}   | Client          |
| PATCH  | /api/appointments/cancel-by-master/{id}   | Master, Owner   |
| PATCH  | /api/appointments/complete/{id}           | Master, Owner   |

## Обработка ошибок

`ExceptionHandlingMiddleware` — перехватывает все `DomainException` и возвращает JSON `{ "error": "..." }` с соответствующим HTTP статусом. В Development режиме неожиданные ошибки возвращают полное сообщение.

## Миграции

```bash
dotnet ef migrations add <Name> --project FadeAfro.Infrastructure --startup-project FadeAfro
dotnet ef database update --project FadeAfro.Infrastructure --startup-project FadeAfro
```
