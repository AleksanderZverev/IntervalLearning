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

Next.js pages — это только точки входа. Внутри каждого catch-all маршрута — React Router `<Routes>`.

```
pages/
  index.tsx
  accounts/authorize/
  accounts/register/
  collections/[[...collections]]      # → Routes: /collections, /collections/:userId-:id, ...
  dictionary/
  learning/[[...learning]]
  schedules/[[...schedules]]          # → Routes: /schedules, /schedules/new, /schedules/:id/edit
  settings/[[...settings]]            # → Routes: /settings/themes, /settings/languages
  store/[[...store]]
```

Шаблон страницы-роутера:
```tsx
// pages/feature/[[...feature]].tsx
const FeatureRouter: FC = () => (
    <Routes>
        <Route path="/feature" element={<FeaturePage />} />
        <Route path="/feature/:id" element={<FeatureDetailPage />} />
    </Routes>
);
export default FeatureRouter;
```

## Структура src/

```
src/
  api/           # axiosInstance (baseURL: http://localhost:5249/api в dev, /api в prod)
  controls/      # Переиспользуемые UI-компоненты
    Form/        # Form, FormField, FormFiledLabel, TextAreaFormField
    Modals/      # AssertionModal, CreateCollectionModal
    PageContainer/   # <Container> с белым фоном
    PageHeader/      # Заголовок страницы с title, subMenu, опциональное редактирование
    Table/           # Table, TableHead, TableBody, TableRow, TableCell, TableHeaderCell
    WebHeader/       # Шапка с логотипом, навигацией, меню пользователя
  hoc/
    withQueryResolver.tsx    # HOC для RTK Query: loading/error/render
    # withMutationResolver   # HOC для мутаций: retry modal + ModalLoader
  hooks/
    useTypedSelector, useTypedDispatch, useOnMount
  pages/         # Компоненты страниц
    collection/
    learning/
    schedules/
    settings/
      ThemesPage/    # /settings/themes
      LanguagesPage/ # /settings/languages
    store/
  redux/
    apiSlice.ts          # Базовый RTK Query api + tagTypes
    axiosBaseQuery.ts    # axiosBaseQuery: axios + onSuccess callback + 401 retry
    themeSlice.ts        # themesApi (getThemes, createTheme, updateTheme, deleteTheme)
    collectionApi.ts     # collectionsApi (полный CRUD)
    schedulesSlice.ts    # schedulesApi
    currentUserSlice.ts  # currentUser + signOutUser
    slices/
      themeSlice.ts      # Entity adapter: setThemes, addTheme, updateTheme, removeTheme
      languagesSlice.ts  # Entity adapter: addLanguages, upsertLanguage, removeLanguage
      collectionsSlice.ts
      cardsSlice.ts
      scheduleSlice.ts
      queueLearnSlice.ts
    api/
      dictionaryApi.ts   # getLanguages, createLanguage, updateLanguage, deleteLanguage, ...
    store.ts             # Конфигурация Redux Store — регистрация слайсов
  types/
    global.ts     # Theme, RememberAnswer
    Dictionary.ts # Language, Word, Translation
    Collection.ts # Collection
```

## Redux Store — зарегистрированные редьюсеры

```
api           # RTK Query cache
accountSlice  # auth / refresh-токен
errors        # глобальные ошибки
currentUser   # залогиненный пользователь
schedules
themes        # entity adapter для Theme
collections   # entity adapter для Collection
cards
queueLearn
languages     # entity adapter для Language
```

При добавлении нового слайса — зарегистрировать в `src/redux/store.ts`.

## RTK Query — паттерны

**Базовый API**: `src/redux/apiSlice.ts` создаёт один `createApi`. Все feature-апишки инжектируют endpoints через `api.injectEndpoints(...)`.

**axiosBaseQuery** (`src/redux/axiosBaseQuery.ts`):
- Принимает `{ url, method, data?, params?, onSuccess? }`
- `onSuccess(dispatch, data)` — колбэк после успешного ответа (используется для dispatch в entity adapter)
- Автоматически вызывает `refreshToken` при 401

**Query (чтение):**
```typescript
getThemes: build.query<Theme[], void>({
    query: () => ({ method: 'GET', url: 'themes',
        onSuccess: async (dispatch, data) => { dispatch(setThemes(data as Theme[])); },
    }),
    providesTags: [tagTypes.themes],
}),
```

**Mutation (запись):**
```typescript
createTheme: build.mutation<Theme, ThemeRequest>({
    query: (data) => ({ method: 'POST', url: 'themes', data }),
    onQueryStarted: async (_, { dispatch, queryFulfilled }) => {
        const { data: created } = await queryFulfilled;
        dispatch(addTheme(created));  // добавляем в entity adapter без рефетча
    },
}),
updateTheme: build.mutation<Theme, { id: number; data: ThemeRequest }>({
    query: ({ id, data }) => ({ method: 'PUT', url: `themes/${id}`, data }),
    onQueryStarted: async (_, { dispatch, queryFulfilled }) => {
        const { data: updated } = await queryFulfilled;
        dispatch(updateTheme(updated));  // обновляем entity adapter
    },
}),
deleteTheme: build.mutation<void, number>({
    query: (id) => ({ method: 'DELETE', url: `themes/${id}` }),
    onQueryStarted: async (id, { dispatch, queryFulfilled }) => {
        await queryFulfilled;
        dispatch(removeTheme(id));
    },
}),
```

**Правило:** не используй `invalidatesTags` вместе с `onQueryStarted` на одной мутации — будет двойное обновление (dispatch + рефетч). Выбирай одно. Для оптимистичного/немедленного обновления — `onQueryStarted`.

**tagTypes** (для invalidation): `collection`, `themes`, `card`, `collectionCards`, `queueCollectionsList`, `notFinishedCollectionsList`, `learningStatistic`.

## withQueryResolver HOC

```tsx
const ConnectedPage = withQueryResolver(useGetThemesQuery)(PageContent);
// <ConnectedPage queryArg={undefined} />
// Показывает Loader пока fetching, ErrorPage при ошибке.
// PageContent получает queryData prop (можно не использовать).
```

`withMutationResolver` — оборачивает компонент, добавляет `mutationProps: { mutate, showRetryModal, isLoading, isSuccess, data, reset }`.

## Страница — типичная структура

```tsx
// src/pages/feature/FeaturePage/FeaturePage.tsx
const FeaturePageContent: FC = () => {
    const items = useTypedSelector(selectItems);
    // ... useState, мутации
    return (
        <PageContainer>
            <PageHeader title="Заголовок" subMenu={<Button>Создать</Button>} />
            <Table>...</Table>
            {dialogOpen && <FeatureDialog open onClose={() => setDialogOpen(false)} />}
        </PageContainer>
    );
};

const ConnectedPage = withQueryResolver(useGetItemsQuery)(FeaturePageContent);

export const FeaturePage: FC = () => (
    <>
        <Head><title>Заголовок</title></Head>
        <ConnectedPage queryArg={undefined} />
    </>
);
```

## Меню пользователя (WebHeader)

Файл: `src/controls/WebHeader/WebHeader.tsx`.

Пункты меню добавляются как `<MenuItem>` внутри `<Menu>`. Навигация — двойная (Next.js router.push + React Router navigate) для совместимости SSR/SPA:

```tsx
<MenuItem onClick={onMenuClick(() => {
    router.push('/settings/themes');
    !isServerSide && navigate('/settings/themes');
})}>
    <ListItemIcon><Style /></ListItemIcon>
    <ListItemText>Темы</ListItemText>
</MenuItem>
```

## Форма — типичная структура

```tsx
const schema = yup.object({ name: yup.string().max(100).required() }).required();

const formMethods = useForm<IForm>({ resolver: yupResolver(schema) });
const { handleSubmit, register, formState: { errors } } = formMethods;

return (
    <FormProvider {...formMethods}>
        <Form>
            <FormField label="Название" error={!!errors.name}
                errorMessage={errors.name?.message} {...register('name')} />
        </Form>
    </FormProvider>
);
```

## Связь с бэкендом

В development: бэкенд доступен на `localhost:5249`, proxy-rewrite в `next.config.js` закомментирован — запросы идут напрямую.

В production (Docker): запросы проходят через nginx-proxy.

## i18n

Настроены две локали: `en` и `ru` (через встроенный Next.js i18n в `next.config.js`).

## Docker

Многоэтапная сборка (Node 16 Alpine). Финальный образ использует `next output: standalone`. Порт: **3000**.

## Entity adapter — правила

`createEntityAdapter` хранит порядок элементов. Используй правильный метод:

| Операция | Метод | Примечание |
|----------|-------|------------|
| Начальная загрузка | `setMany` / `setAll` | `setAll` сбрасывает порядок полностью |
| Создание | `addOne` | Добавляет в конец |
| Обновление | `updateOne({ id, changes })` | **Не меняет порядок** |
| Удаление | `removeOne(id)` | |

**Не используй `upsertOne` для обновлений** — если элемент уже есть в сторе, он переместится в конец (изменение порядка в UI).

Разделяй экшены `addItem` и `updateItem` — не пытайся покрыть оба случая одним `upsertOne`.

```typescript
addTheme: (state, action: PayloadAction<Theme>) => { adapter.addOne(state, action.payload); },
updateTheme: (state, action: PayloadAction<Theme>) => {
    adapter.updateOne(state, { id: action.payload.id, changes: action.payload });
},
```

## Таблицы — ширина колонок

Колонки с кнопками-иконками должны иметь фиксированную ширину, иначе ячейки растягиваются:

```tsx
<TableHeaderCell width={90}></TableHeaderCell>
// ...
<TableCell width={90} align="right">
    <Stack direction="row" justifyContent="flex-end">
        <IconButton size="small">...</IconButton>
    </Stack>
</TableCell>
```

Ширину подбирай по количеству иконок: ~45px на иконку.

## Чеклист добавления новой страницы

1. Тип в `src/types/` (если нет)
2. Entity adapter slice в `src/redux/slices/featureSlice.ts` — разделять `addItem`/`updateItem`, использовать `updateOne` для обновлений
3. Зарегистрировать слайс в `src/redux/store.ts`
4. API endpoints в `src/redux/featureApi.ts` — все мутации через `onQueryStarted`, не через `invalidatesTags`
5. Компонент страницы в `src/pages/feature/FeaturePage/FeaturePage.tsx`
6. Next.js роутер в `pages/feature/[[...feature]].tsx`
7. Пункт меню в `src/controls/WebHeader/WebHeader.tsx` (если нужна навигация из шапки)
