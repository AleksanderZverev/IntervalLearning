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
    DictionaryController       # Словарь
  Store/
    Collections/               # CRUD коллекций
    Cards/                     # CRUD карточек
    Themes/                    # Темы
    RepeatsSchedules/          # Расписания повторений
    Statistics/                # Статистика коллекций
  Study/
    Statistics/                # Статистика обучения
```

Swagger доступен только в Development по адресу `/api/swagger`.

## Конфигурация

Подключение к БД передаётся через переменную окружения `DB_CONNECTION_STRING` (в production — из docker-compose, генерируется `build.ps1` из `secrets.json`).

JWT-конфиг в `appsettings.json` (секция `JsonWebTokenKeys`):
- `ValidIssuer`: `https://interval-learning.ru`
- `JwtTokenTTLInMinutes`: 15
- `RefreshTokenTTLInDays`: 2
- Signing key передаётся отдельно (не в репозитории)

## Docker

Многоэтапная сборка (SDK 7.0 → aspnet:7.0 runtime). Порты: **80** и **443**. Entrypoint: `dotnet IntervalLearningApi.dll`.
