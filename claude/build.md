# Build

## Скрипты

| Скрипт | Описание |
|--------|----------|
| `build.ps1` | Основной скрипт сборки. Запускается из корня проекта. |
| `start-prod.ps1` | Поднимает контейнеры через `docker compose up` (production). |
| `open-dev-env.ps1` | Открывает среду разработки (GitExtensions, VS, VS Code, dev-сервер). |
| `nginx/update-keys.ps1` | Генерирует SSL-сертификат для localhost через mkcert. |
| `scripts/add-initial-db-values.ps1` | Заполняет БД начальными данными. Запускать один раз после первого старта. |
| `interval-learning-web/dev.ps1` | Запускает `yarn dev` для фронтенда. Вызывается из `open-dev-env.ps1`. |

## Что делает build.ps1

1. Запрашивает подтверждение перед стартом
2. Запускает `nginx/update-keys.ps1` — генерирует SSL-сертификаты для nginx
3. Читает секреты из `secrets.json` (если нет — берёт из `secrets.template.json` с предупреждением)
4. Подставляет секреты в template-файлы и генерирует итоговые docker-compose файлы
5. Запускает `docker compose build`

При ошибке билда выводит подсказку для детального лога:
```powershell
docker compose build --no-cache --progress=plain
```

## Секреты

Файл `secrets.json` в корне репозитория — не коммитится в git. Структура описана в `secrets.template.json`.

Секции: `Development` и `Production`, каждая содержит `Database` с полями:
- `Host`, `Port`, `DatabaseName`, `Username`, `Password`

На основе этих полей `build.ps1` генерирует `DB_CONNECTION_STRING` для backend-сервиса.

## Template-файлы и плейсхолдеры

| Файл | Генерирует |
|------|-----------|
| `docker-compose-template.yml` | `docker-compose.yml` (Development) |
| `docker-compose.production-template.yml` | `docker-compose.production.yml` (Production) |

Плейсхолдеры в template-файлах:

| Плейсхолдер | Значение |
|-------------|---------|
| `${IMAGE_VERSION}` | Дата билда в формате `yyyy-MM-dd` |
| `{{DB_NAME}}` | `Database.DatabaseName` из secrets |
| `{{DB_USER}}` | `Database.Username` из secrets |
| `{{DB_PASSWORD}}` | `Database.Password` из secrets |
| `{{DB_CONNECTION_STRING}}` | Строка подключения, собранная из всех полей `Database` |

## Docker-сервисы

| Сервис | Образ | Порты |
|--------|-------|-------|
| `postgres` | `postgres:13` | 5432 |
| `backend` | `intervallearningapi:{version}` | 5249→80, 7249→443 |
| `client` | `client:{version}` | 3000 |
| `proxy` | `nginx:{version}` | 80, 443 |

Production-файл накладывается поверх базового: `docker-compose -f docker-compose.yml -f docker-compose.production.yml up -d`
