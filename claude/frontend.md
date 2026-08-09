# Frontend (interval-learning-web)

## Стек

| Технология | Версия | Роль |
|------------|--------|------|
| Next.js | 12.0.10 | Фреймворк (SSR/SSG + routing) |
| React | 17 | UI |
| TypeScript | 4.5.5 | Язык |
| MUI (Material UI) | v5 | Компонентная библиотека и стили |
| Emotion | — | CSS-in-JS (используется MUI) |
| Redux Toolkit | — | Глобальное состояние |
| RTK Query (apiSlice) | — | Запросы к API и кэширование |
| next-redux-wrapper | — | Интеграция Redux с Next.js SSR |
| react-hook-form + yup | v7 | Формы и валидация |
| axios | — | HTTP-клиент |
| dayjs | — | Работа с датами |

## Запуск

```bash
yarn dev   # dev-сервер на порту 4001
yarn build # production-сборка (Next.js standalone output)
```

Dev-сервер автоматически запускается через `open-dev-env.ps1` → `dev.ps1`.

## Роутинг (pages/)

```
pages/
  index.tsx                        # Главная
  accounts/authorize/              # Вход
  accounts/register/               # Регистрация
  collections/[[...collections]]   # Коллекции (catch-all)
  dictionary/                      # Словарь
  learning/[[...learning]]         # Обучение (catch-all)
  schedules/                       # Расписания
  store/                           # Магазин
```

## Структура src/

```
src/
  api/           # Базовые утилиты для запросов
  controls/      # Переиспользуемые UI-компоненты
  globals/       # Глобальные константы / конфиги
  helpers/       # Вспомогательные функции
  hoc/           # Higher-Order Components
  hooks/         # Кастомные React-хуки
  pages/         # Компоненты страниц (не путать с pages/ в корне)
  redux/
    slices/      # Redux-слайсы состояния
    api/         # RTK Query API-слайсы
    store.ts     # Конфигурация Redux Store
  types/         # TypeScript типы и интерфейсы
  theme.ts       # MUI-тема
  ErrorHandler.tsx
  GlobalErrorBoundary.tsx
```

## Связь с бэкендом

В development: бэкенд доступен на `localhost:5249`, proxy-rewrite в `next.config.js` закомментирован — запросы идут напрямую.

В production (Docker): запросы проходят через nginx-proxy.

## i18n

Настроены две локали: `en` и `ru` (через встроенный Next.js i18n в `next.config.js`).

## Docker

Многоэтапная сборка (Node 16 Alpine). Финальный образ использует `next output: standalone`. Порт: **3000**.
