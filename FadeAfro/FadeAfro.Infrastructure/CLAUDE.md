# FadeAfro.Infrastructure

Инфраструктурный слой. Реализует интерфейсы из `FadeAfro.Domain`. Зависит от `FadeAfro.Domain` и `FadeAfro.Application`.

## Структура

```
Persistence/
  DatabaseContext.cs              — DbContext (EF Core + Npgsql)
  Configurations/                 — IEntityTypeConfiguration<T> для каждой сущности
Repositories/                     — реализации IRepository интерфейсов
Services/
  JwtTokenService.cs              — генерация JWT токена
Settings/
  TelegramSettings.cs             — реализация ITelegramSettings через IOptions<TelegramOptions>
Options/
  JwtOptions.cs                   — [Required] поля: SecretKey, Issuer, Audience, ExpirationMinutes
  TelegramOptions.cs              — [Required] поле: BotToken
Extensions/
  ServiceCollectionExtensions.cs  — AddJwt(), AddTelegram(), AddPostgres()
Migrations/                       — EF Core миграции
```

## Регистрация сервисов

```csharp
services.AddPostgres(configuration); // DbContext + все репозитории
services.AddJwt();                   // JWT аутентификация + JwtTokenService
services.AddTelegram();              // TelegramSettings
```

## Options (конфигурация)

Все Options используют `.ValidateDataAnnotations().ValidateOnStart()` — приложение не запустится при отсутствии обязательных полей.

**JwtOptions** (`"Jwt"` секция):
- `SecretKey` — ключ подписи токена
- `Issuer` — издатель
- `Audience` — аудитория
- `ExpirationMinutes` — от 1 до 1440 (по умолчанию 60)

**TelegramOptions** (`"Telegram"` секция):
- `BotToken` — токен бота

## DatabaseContext

EF Core конфигурации применяются через `ApplyConfigurationsFromAssembly` — каждая сущность имеет свой `IEntityTypeConfiguration<T>` в `Configurations/`.

DbSets: `Users`, `MasterProfiles`, `Services`, `MasterSchedules`, `MasterUnavailabilities`, `Appointments`.

## Репозитории

Все репозитории принимают `DatabaseContext` через DI. Каждый вызов `AddAsync`/`UpdateAsync`/`DeleteAsync` сразу делает `SaveChangesAsync()`.

`AppointmentRepository` содержит пагинированные методы:
```csharp
// DB-level пагинация через Skip/Take на IQueryable
var query = _context.Appointments.Where(...);
var totalCount = await query.CountAsync();
var items = await query.OrderByDescending(a => a.StartTime)
    .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
```

## JwtTokenService

Claims в токене:
- `NameIdentifier` — `user.Id` (Guid)
- `telegramId` — `user.TelegramId` (long)
- `GivenName` — `user.FirstName`
- `Role` — все роли пользователя

## Миграции

```bash
# Создать миграцию
dotnet ef migrations add <Name> \
  --project FadeAfro.Infrastructure \
  --startup-project FadeAfro

# Применить к БД
dotnet ef database update \
  --project FadeAfro.Infrastructure \
  --startup-project FadeAfro
```

## Добавление новой сущности

1. Добавить `DbSet<T>` в `DatabaseContext`
2. Создать `Configurations/{Entity}Configuration.cs`
3. Создать `Repositories/{Entity}Repository.cs`
4. Зарегистрировать в `AddPostgres()`
5. Создать миграцию
