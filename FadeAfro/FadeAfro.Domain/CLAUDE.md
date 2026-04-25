# FadeAfro.Domain

Ядро приложения. Не зависит ни от одного другого проекта. Содержит бизнес-логику, сущности и контракты.

## Структура

```
Entities/           — доменные сущности
Enums/              — перечисления
Exceptions/         — доменные исключения
Repositories/       — интерфейсы репозиториев
Services/           — интерфейсы доменных сервисов
Constants/          — константы (роли)
```

## Сущности

Все сущности наследуют `Entity` — базовый класс с `Id: Guid` (генерируется в конструкторе) и `CreatedAt: DateTime`.

| Сущность               | Ключевые поля                                              |
|------------------------|------------------------------------------------------------|
| `User`                 | TelegramId, FirstName, LastName?, Username?, Roles         |
| `MasterProfile`        | MasterId (→ User), PhotoUrl?, Description?                 |
| `Service`              | MasterProfileId, Name, Description?, Price, Duration       |
| `MasterSchedule`       | MasterProfileId, DayOfWeek, StartTime, EndTime             |
| `MasterUnavailability` | MasterProfileId, Date, StartTime?, EndTime? (null = весь день) |
| `Appointment`          | ClientId, MasterProfileId, ServiceId, StartTime, EndTime, Status, Comment? |

## Бизнес-правила (валидация в конструкторах)

- `User`: TelegramId > 0, FirstName не пустой, Roles не пустые
- `Service`: Name не пустой, Price > 0, Duration > 0
- `MasterSchedule`: EndTime > StartTime
- `MasterUnavailability`: если оба времени указаны — EndTime > StartTime
- `Appointment`: StartTime > DateTime.UtcNow

## Методы сущностей

**Appointment:**
- `CancelByClient()` — из Pending/Confirmed → CancelledByClient
- `CancelByMaster()` — из Pending/Confirmed → CancelledByMaster
- `Complete()` — только из Confirmed → Completed
- Все методы бросают `InvalidAppointmentStatusException` при неверном статусе

**Service:**
- `Update(name, description, price, duration)` — с той же валидацией

**MasterProfile:**
- `Update(photoUrl, description)` — без ограничений

## Исключения

Все наследуют `DomainException : Exception`. Именование: `{Entity}NotFoundException`, `Invalid{Field}Exception`, `{Entity}AlreadyExistsException`.

| Тип исключения                      | HTTP статус |
|-------------------------------------|-------------|
| `*NotFoundException`                | 404         |
| `InvalidInitDataException`          | 401         |
| `*AlreadyExistsException`           | 409         |
| Все остальные `DomainException`     | 400         |

## Интерфейсы репозиториев

Определены здесь, реализованы в `FadeAfro.Infrastructure`. Каждый репозиторий имеет стандартный набор методов: `GetByIdAsync`, `AddAsync`, `UpdateAsync`/`DeleteAsync`, специфичные методы для выборок.

`IAppointmentRepository` дополнительно имеет пагинированные методы:
- `GetByClientIdPagedAsync(clientId, page, pageSize)` → `(Items, TotalCount)`
- `GetByMasterProfileIdPagedAsync(masterProfileId, page, pageSize)` → `(Items, TotalCount)`

## Константы ролей

```csharp
Roles.Client          // "Client"
Roles.Owner           // "Owner"
Roles.MasterOrOwner   // "Master,Owner"
Roles.ClientOrOwner   // "Client,Owner"
```

## Добавление новой сущности

1. Создать класс в `Entities/`, наследовать `Entity`
2. Добавить валидацию в конструкторе, исключения в `Exceptions/{Entity}/`
3. Добавить интерфейс репозитория в `Repositories/`
