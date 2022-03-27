import { accountSlice } from './accountSlice';
import { Action, configureStore, ThunkAction } from '@reduxjs/toolkit';
import { createWrapper } from 'next-redux-wrapper';
import { api } from './apiSlice';
import { currentUserSlice } from './currentUserSlice';
import { errorSlice } from './errorSlice';
import { scheduleSlice } from './slices/scheduleSlice';
import { collectionSlice } from './slices/collectionsSlice';

const makeStore = () =>
    configureStore({
        reducer: {
            [api.reducerPath]: api.reducer,
            [accountSlice.reducerPath]: accountSlice.reducer,
            errors: errorSlice.reducer,
            currentUser: currentUserSlice.reducer,
            schedules: scheduleSlice.reducer,
            collections: collectionSlice.reducer,
        },
        middleware: (getDefaultMiddleware) =>
            getDefaultMiddleware({ serializableCheck: false }).concat(api.middleware, accountSlice.middleware),
    });

// optional, but required for refetchOnFocus/refetchOnReconnect behaviors
// see `setupListeners` docs - takes an optional callback as the 2nd arg for customization
// setupListeners(store.dispatch)

// Infer the `RootState` and `AppDispatch` types from the store itself
export type AppStore = ReturnType<typeof makeStore>;
export type RootState = ReturnType<AppStore['getState']>;
export type AppThunk<ReturnType = void> = ThunkAction<ReturnType, RootState, unknown, Action>;

// Inferred type: {posts: PostsState, comments: CommentsState, users: UsersState}
export type AppDispatch = AppStore['dispatch'];

export const wrapper = createWrapper<AppStore>(makeStore);
