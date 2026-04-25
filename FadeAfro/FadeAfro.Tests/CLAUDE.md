# FadeAfro.Tests

Unit тесты. Покрывают Domain сущности и Application хендлеры. Без интеграционных тестов.

## Стек

- xUnit — тест-фреймворк
- NSubstitute — мокирование зависимостей
- FluentAssertions — читаемые assertions

## Структура

```
Domain/
  UserTests.cs                    — 10 тестов
  ServiceTests.cs                 — 15 тестов
  MasterScheduleTests.cs          — 6 тестов
  MasterUnavailabilityTests.cs    — 4 теста
  AppointmentTests.cs             — 19 тестов
Application/
  Users/
    GetUserHandlerTests.cs        — 2 теста
    RegisterUserHandlerTests.cs   — 2 теста
  MasterProfiles/
    CreateMasterProfileHandlerTests.cs  — 4 теста
    UpdateMasterProfileHandlerTests.cs  — 3 теста
    GetMasterProfileHandlerTests.cs     — 2 теста
    GetAllMastersHandlerTests.cs        — 3 теста
    GetAvailableSlotsHandlerTests.cs    — 9 тестов
  Services/
    AddServiceHandlerTests.cs           — 3 теста
    UpdateServiceHandlerTests.cs        — 2 теста
    DeleteServiceHandlerTests.cs        — 2 теста
    GetMasterServicesHandlerTests.cs    — 4 теста
  MasterSchedules/
    SetScheduleHandlerTests.cs          — 2 теста
    GetMasterScheduleHandlerTests.cs    — 3 теста
    DeleteScheduleHandlerTests.cs       — 2 теста
  MasterUnavailabilities/
    AddUnavailabilityHandlerTests.cs    — 3 теста
    DeleteUnavailabilityHandlerTests.cs — 2 теста
    GetMasterUnavailabilitiesHandlerTests.cs — 3 теста
  Appointments/
    CreateAppointmentHandlerTests.cs    — 5 тестов
    CancelAppointmentHandlerTests.cs    — 3 теста
    CompleteAppointmentHandlerTests.cs  — 2 теста
    GetClientAppointmentsHandlerTests.cs — 4 теста
    GetMasterAppointmentsHandlerTests.cs — 4 теста
  Auth/
    AuthenticateTelegramUserHandlerTests.cs — 6 тестов
```

**Итого: 130 тестов**

## Запуск

```bash
dotnet test                                             # все тесты
dotnet test --filter "FullyQualifiedName~Domain"        # только Domain
dotnet test --filter "FullyQualifiedName~Application"   # только Application
dotnet test --filter "FullyQualifiedName~Appointment"   # по имени класса
dotnet test --logger "console;verbosity=detailed"       # с именами тестов
```

## Соглашения

**Именование тестов:** `{Метод}_{Условие}_{ОжидаемыйРезультат}`
```
Handle_NewUser_CreatesUserAndReturnsToken
Handle_InvalidHash_ThrowsInvalidInitDataException
Constructor_StartTimeInPast_ThrowsInvalidAppointmentTimeException
```

**Структура теста (AAA):**
```csharp
// Arrange
var command = new ...;
_repository.GetByIdAsync(...).Returns(...);

// Act
var result = await _handler.Handle(command, CancellationToken.None);

// Assert
result.Id.Should().NotBeEmpty();
await _repository.Received(1).AddAsync(Arg.Is<Entity>(...));
```

**ForceStatus** — для тестов с конкретным статусом Appointment используется рефлексия (private setter):
```csharp
typeof(Appointment).GetProperty(nameof(Appointment.Status))!.SetValue(appointment, status);
```

**Auth тесты** — генерация корректного HMAC-SHA256 initData прямо в тесте через `BuildValidInitData()`.

## Добавление теста для нового хендлера

1. Создать файл `Application/{Domain}/{HandlerName}Tests.cs`
2. Замокировать все зависимости через `Substitute.For<IRepository>()`
3. Создать экземпляр хендлера в конструкторе
4. Покрыть: happy path, все `NotFoundException`, все `AlreadyExistsException`, edge cases
