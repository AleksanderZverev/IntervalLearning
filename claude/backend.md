# Backend (back/)

## Стек

| Технология | Версия | Роль |
|------------|--------|------|
| .NET / ASP.NET Core | net9.0, LangVersion 13 | Платформа |
| Entity Framework Core | 7.0.9 | ORM |
| Npgsql | 7.0.4 | PostgreSQL-провайдер для EF |
| FluentValidation | 11.8 | Валидация запросов |
| Mapster | 7.4 | Маппинг объектов (DTO ↔ Domain) |
| JwtBearer | 7.0.9 | JWT-аутентификация |
| BCrypt.Net-Next | 4.0.3 | Хэширование паролей |
| FluentResults | — | Монада Result для ошибок без исключений |
| Swashbuckle (Swagger) | 6.2.3 | Документация API (только в Development) |

> Dockerfile использует образы `dotnet/aspnet:7.0` / `dotnet/sdk:7.0` — расхождение с target framework `net9.0` в .csproj.

## Архитектура

Clean Architecture + DDD. Слои (от внутреннего к внешнему):

```
Domain
  └─ DomainServices
       └─ Application (CQRS — только Commands)
            └─ DB (EF Core, репозитории, миграции)
            └─ Infrastructure (JWT, BCrypt)
                 └─ IntervalLearningApi (ASP.NET host, Controllers)
```

`Program.cs` регистрирует слои через extension-методы: `AddPersistence`, `AddInfrastructure`, `AddDomainServices`, `AddApplication`, `AddWeb`.

Аутентификация реализована через кастомный `JwtMiddleware` (не стандартный `UseAuthentication`).

## Проекты в solution

| Проект | Роль |
|--------|------|
| `IntervalLearningApi` | Хост: Controllers, Middleware, Program.cs |
| `Domain` | Сущности, агрегаты, Value Objects |
| `DomainServices` | Интерфейсы и реализации доменных сервисов |
| `Application` | CQRS Commands + DomainEventHandlers |
| `Infrastructure` | JWT, BCrypt (BoundedContexts) |
| `DB` | EF Core DbContext, репозитории, миграции |
| `GlobalTools` | Общие утилиты |
| `Domain.UnitTests` | Unit-тесты домена |
| `GlobalTools.Tests` | Unit-тесты утилит |
| `IntervalLearningApi.IntegrationTests` | Интеграционные тесты |

## Controllers (API endpoints)

```
Controllers/
  Accounts/
    AuthenticationController   # Регистрация, вход, refresh-токен
  Dictionary/
    DictionaryController       # Языки (GET/POST/PUT/DELETE), переводы, поиск слов
  Study/
    Collections/               # CRUD коллекций
    Cards/                     # CRUD карточек
    Themes/
      ThemeController          # GET/POST/PUT/DELETE тем
    RepeatsSchedules/          # Расписания повторений
    Statistics/                # Статистика обучения
  Store/
    Statistics/                # Статистика коллекций
```

Маршруты централизованы в `IntervalLearningApi/Constants/ApiRoutes.cs`. Добавляя новый endpoint, сначала описывай маршрут там.

Swagger доступен только в Development по адресу `/api/swagger`.

## CQRS — паттерн команды

Интерфейсы:
- `ICommand<TRequest>` — команда без возвращаемого значения → `Task<Result>`
- `ICommand<TRequest, TResult>` — команда с результатом → `Task<Result<TResult>>`

Команды **регистрируются автоматически** в `Application/DI/ServiceCollectionExtensions.cs` через reflection — новые файлы подхватываются без изменения DI.

Вызов из контроллера:
```csharp
var result = await commandManager
    .GetCommand<MyCommand>()
    .Handle(new MyRequest(...));
return result.ToActionResult(x => mapper.Map<MyDto>(x));
```

Типичная реализация команды:
```csharp
// Application/Commands/Feature/DoThing/DoThingCommand.cs
public class DoThingCommand : ICommand<DoThingRequest, MyEntity>
{
    public DoThingCommand(IStudyRepository studyRepository) { ... }

    public async Task<Result<MyEntity>> Handle(DoThingRequest request)
    {
        return await repository.Query.Entities.Find(request.Id)
            .ToResultAsync()
            .ErrorIfNull(new NotFoundError(nameof(MyEntity)))
            .Bind(entity =>
            {
                entity.Name = request.Name;
                return repository.Entities.UpdateAndSave(entity);
            });
    }
}
```

## Result extensions (GlobalTools.Extensions)

Ключевые методы:
- `Task<T>.ToResultAsync()` → `Task<Result<T>>` — оборачивает значение в Result.Ok
- `Task<Result<T?>>.ErrorIfNull(IError)` → `Task<Result<T>>` — возвращает ошибку если null
- `result.HasAnyError()` — работает на ValueTuple из нескольких результатов

Ошибки (`GlobalTools.Errors`): `NotFoundError`, `BadRequestError`, `InternalError`, `ConflictError`, `ForbiddenError`.

`ResultsExtensions.ToActionResult<T, TResponse>(this Result<T>, Func<T, TResponse>)` — маппит Result в ActionResult.

## Репозитории

**Запросы** (read-only) → `IBoundedContextQueryRepository`:
- `IStudyQueryRepository` — Themes, Collections, Cards, Schedules, Queue, CardRemembers, RelearningCards
- `IDictionaryQueryRepository` — Languages, Words

**Мутации** (write) → `IRepository<T>` / `IRepository<T, TId, TIdParams>`:
- `BaseRepository<T>` — базовая реализация: `Add/Update/Delete` + `...AndSave` варианты
- Специализированные репозитории наследуют `BaseRepository<T>` и добавляют `GetUniqueId` через PostgreSQL sequence

Регистрация репозиториев — **ручная** в `DB/DependencyInjection/DependencyInjectionExtensions.cs`.

Паттерн генерации ID через sequence (для Theme, Collection и др.):
```csharp
var id = studyRepository.Themes.GetUniqueId(new ThemeIdParams()).Value;
```

Языки используют `ValueGeneratedOnAdd` (PostgreSQL identity column) — ID генерируется БД при вставке.

## Domain — Value Objects

Все ID и строковые поля — value objects с фабричным методом `Create()`:
- `ThemeId`, `ThemeTitle`, `LanguageId`, `ShortString` (max 50), `CollectionTitle` и др.
- `Create()` возвращает `Result<T>` — проверяй на `IsFailed` перед использованием `.Value`

**Инкапсуляция:** поля сущностей должны быть `private set`. Логику обновления выноси в доменный метод сущности:

```csharp
// Поля — private set
public ThemeTitle Name { get; private set; }

// Конструктор принимает обязательные поля (не использовать object initializer с private set)
public Theme(ThemeId id, ThemeTitle name) : base(id) { Name = name; }

// Мутация через метод
public void Update(ThemeTitle name) { Name = name; }
```

Для `Language` метод `Update` возвращает `Result` (валидирует Value Objects внутри):
```csharp
public Result Update(string name, string nativeLanguageName, ...) { ... }
```

**Не делай:** `theme.Name = value` снаружи. Только через доменный метод.

## DTO и Mapster

Каждый DTO файл содержит класс `XxxRegister : IRegister` с конфигурацией Mapster.  
`TypeAdapterConfig` настроен с `RequireExplicitMapping = true` — все маппинги должны быть явно объявлены.

```csharp
public class ThemeRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Theme, ThemeDto>()
            .Map(d => d.Id, s => s.Id.Value);
    }
}
```

## ApplicationContext (DbSet-ы)

```
Users, UsersPasswords, RefreshTokens
Collections, Cards, Remembers, PhaseRememberEntities
Themes, RepeatsSchedules, Phases
Queue, RelearningCards, UserMetadata
CollectionPublications, PublicCollectionSubscribers
Words, Languages, Translations
```

Миграции: `dotnet ef migrations add {Name} -p DB -s IntervalLearningApi`

## Конфигурация

Подключение к БД передаётся через переменную окружения `DB_CONNECTION_STRING` (в production — из docker-compose, генерируется `build.ps1` из `secrets.json`).

JWT-конфиг в `appsettings.json` (секция `JsonWebTokenKeys`):
- `ValidIssuer`: `https://interval-learning.ru`
- `JwtTokenTTLInMinutes`: 15
- `RefreshTokenTTLInDays`: 2
- Signing key передаётся отдельно (не в репозитории)

## Docker

Многоэтапная сборка (SDK 7.0 → aspnet:7.0 runtime). Порты: **80** и **443**. Entrypoint: `dotnet IntervalLearningApi.dll`.

## Request-классы и валидация

Request-классы контроллеров выносятся в отдельные файлы по паттерну:
```
Controllers/Feature/Requests/FeatureRequest.cs
```

Каждый файл содержит пару: `XxxRequestValidator : AbstractValidator<XxxRequest>` + `XxxRequest`.
Валидаторы регистрируются автоматически через `AddValidatorsFromAssemblyContaining<AuthenticateRequestValidator>()`.

В контроллере вызов: `validatorResolver.Validate(request)` → при ошибке `validation.ToErrorActionResult()`.

```csharp
public class ThemeRequestValidator : AbstractValidator<ThemeRequest>
{
    public ThemeRequestValidator()
    {
        RuleFor(p => p.Name).ShouldBeCreatable(ThemeTitle.Create);
    }
}
public class ThemeRequest { public string Name { get; set; } }
```

Не определяй Request-классы прямо в файле контроллера.

## Чеклист добавления нового CRUD endpoint

1. `Domain/Feature/ValueObjects/` — value objects если нужны новые
2. `Domain/Feature/Feature.cs` — доменные методы (конструктор + Update/и т.д.)
3. `Application/Commands/Feature/DoThing/DoThingRequest.cs` — record с параметрами
4. `Application/Commands/Feature/DoThing/DoThingCommand.cs` — реализация ICommand
5. `IntervalLearningApi/Controllers/Feature/DTOs/FeatureDto.cs` — DTO + Register : IRegister
6. `IntervalLearningApi/Controllers/Feature/Requests/FeatureRequest.cs` — request + FluentValidation validator
7. `IntervalLearningApi/Constants/ApiRoutes.cs` — константы маршрутов
8. `IntervalLearningApi/Controllers/Feature/FeatureController.cs` — action method с вызовом validatorResolver
9. Если нужен новый репозиторий: реализация в `DB/Repository/`, регистрация в `DependencyInjectionExtensions.cs`
