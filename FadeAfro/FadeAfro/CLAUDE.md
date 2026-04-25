# FadeAfro (API)

ASP.NET Core Web API проект. Точка входа приложения. Зависит от `FadeAfro.Application` и `FadeAfro.Infrastructure`.

## Структура

```
Controllers/                      — HTTP контроллеры
Middleware/
  ExceptionHandlingMiddleware.cs  — глобальная обработка ошибок
Extensions/
  ServiceCollectionExtensions.cs  — AddSwagger()
  WebApplicationExtensions.cs     — UseExceptionHandling(), UseSwaggerWithUi()
Program.cs                        — точка входа, DI регистрация
```

## Program.cs — порядок регистрации

```csharp
builder.Services.AddApplication();          // MediatR хендлеры
builder.Services.AddPostgres(configuration); // EF Core + репозитории
builder.Services.AddJwt();                   // JWT аутентификация
builder.Services.AddTelegram();              // Telegram settings
builder.Services.AddControllers();
builder.Services.AddAuthorization();
builder.Services.AddSwagger();

app.UseExceptionHandling();   // первым в pipeline
app.UseSwaggerWithUi();
app.UseAuthentication();      // до UseAuthorization
app.UseAuthorization();
app.MapControllers();
```

## Контроллеры

Все контроллеры: `[ApiController]`, `[Route("api/...")]`, `[Tags("...")]`, `[Authorize]` на уровне класса.

| Контроллер                        | Базовый маршрут                    |
|-----------------------------------|------------------------------------|
| `AuthController`                  | api/auth                           |
| `UsersController`                 | api/users                          |
| `MasterProfilesController`        | api/master-profiles                |
| `ServicesController`              | api/services                       |
| `MasterSchedulesController`       | api/master-schedules               |
| `MasterUnavailabilitiesController`| api/master-unavailabilities        |
| `AppointmentsController`          | api/appointments                   |

Контроллеры только делегируют в MediatR — никакой бизнес-логики:
```csharp
var result = await _mediator.Send(new SomeCommand(...));
return Ok(result);
```

## Авторизация

На уровне роутов используются константы из `FadeAfro.Domain.Constants.Roles`:
```csharp
[Authorize(Roles = Roles.Owner)]
[Authorize(Roles = Roles.MasterOrOwner)]
[Authorize(Roles = Roles.ClientOrOwner)]
[AllowAnonymous]  // для auth/login и users/register
```

## ExceptionHandlingMiddleware

Перехватывает все исключения и возвращает `{ "error": "..." }`:

| Исключение                                | HTTP  |
|-------------------------------------------|-------|
| `*NotFoundException`                      | 404   |
| `InvalidInitDataException`                | 401   |
| `*AlreadyExistsException`                 | 409   |
| Validation (`Invalid*Exception`)          | 400   |
| Прочие `DomainException`                  | 400   |
| Неожиданные (Development: с деталями)     | 500   |

## Swagger

Доступен на `/swagger`. JWT поддержка через `options.InferSecuritySchemes()` (Swashbuckle 10.x) — автоматически читает зарегистрированную схему JWT Bearer.

## Добавление нового эндпоинта

1. Создать фичу в `FadeAfro.Application`
2. Добавить метод в соответствующий контроллер
3. Указать `[Http*]`, `[SwaggerOperation]`, `[Authorize]` / `[AllowAnonymous]`
4. Делегировать через `_mediator.Send(...)`
