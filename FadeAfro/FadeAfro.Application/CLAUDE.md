# FadeAfro.Application

Use cases приложения. Зависит только от `FadeAfro.Domain`. Реализует CQRS через MediatR.

## Структура

```
Features/
  Auth/
    AuthenticateTelegramUser/   — валидация Telegram initData, upsert пользователя, выдача JWT
  Users/
    GetUser/                    — получить пользователя по TelegramId
    RegisterUser/               — зарегистрировать пользователя вручную
  MasterProfiles/
    CreateMasterProfile/
    UpdateMasterProfile/
    GetMasterProfile/
    GetAllMasters/
    GetAvailableSlots/          — расчёт свободных слотов для записи
  Services/
    AddService/
    UpdateService/
    DeleteService/
    GetMasterServices/
  MasterSchedules/
    SetSchedule/
    GetMasterSchedule/
    DeleteSchedule/
  MasterUnavailabilities/
    AddUnavailability/
    DeleteUnavailability/
    GetMasterUnavailabilities/
  Appointments/
    CreateAppointment/
    CancelAppointment/          — флаг CancelledByMaster: bool
    CompleteAppointment/
    GetClientAppointments/      — пагинация
    GetMasterAppointments/      — пагинация
Common/
  PaginationParams.cs           — record(Page = 1, PageSize = 20)
  PagedResponse.cs              — record<T>(Items, TotalCount, Page, PageSize) + TotalPages
Settings/
  ITelegramSettings.cs          — интерфейс для BotToken
```

## Соглашения

Каждая фича — отдельная папка с тремя файлами:
- `{Name}Command.cs` / `{Name}Query.cs` — входные данные (`IRequest<TResponse>`)
- `{Name}Handler.cs` — логика (`IRequestHandler<TRequest, TResponse>`)
- `{Name}Response.cs` — выходные данные

**Query** — только чтение данных, не меняет состояние.  
**Command** — меняет состояние, может вернуть данные или `Unit`.

## Регистрация

```csharp
services.AddApplication(); // регистрирует все хендлеры через MediatR
```

MediatR сканирует сборку `FadeAfro.Application` автоматически.

## GetAvailableSlots — алгоритм

Самый сложный хендлер. Логика:
1. Проверить существование MasterProfile и Service
2. Найти расписание на нужный день недели — если нет, вернуть `[]`
3. Проверить наличие полного блокирования (`StartTime == null && EndTime == null`) — вернуть `[]`
4. Получить активные записи на дату (не отменённые)
5. Генерировать слоты от `schedule.StartTime` до `schedule.EndTime` с шагом `service.Duration`
6. Исключить слоты, конфликтующие с частичными unavailabilities или активными записями

Конфликт: `slotStart < blockEnd && slotEnd > blockStart`.

## Пагинация

```csharp
// Запрос
new GetClientAppointmentsQuery(clientId, new PaginationParams(page: 1, pageSize: 20))

// Ответ
PagedResponse<AppointmentResponse> {
    Items, TotalCount, Page, PageSize,
    TotalPages  // вычисляемое свойство
}
```

Пагинация выполняется на уровне БД через `Skip/Take` в репозитории.

## Добавление новой фичи

1. Создать папку `Features/{Domain}/{FeatureName}/`
2. Добавить `{FeatureName}Command.cs` или `{FeatureName}Query.cs`
3. Добавить `{FeatureName}Handler.cs` с реализацией `IRequestHandler`
4. Добавить `{FeatureName}Response.cs` (если нужен кастомный ответ)
5. Написать unit тест в `FadeAfro.Tests/Application/{Domain}/`
