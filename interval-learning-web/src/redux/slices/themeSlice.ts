import { RootState } from './../store';
import { createEntityAdapter, createSlice, PayloadAction } from '@reduxjs/toolkit';
import { Theme } from '../../types/global';

const adapter = createEntityAdapter<Theme>({ selectId: (t) => t.id });

export const themeSlice = createSlice({
    name: 'themes',
    initialState: adapter.getInitialState(),
    reducers: {
        setThemes: (state, action: PayloadAction<Theme[]>) => {
            adapter.setMany(state, action.payload);
        },
    },
});

export const { setThemes } = themeSlice.actions;

export const { selectAll: selectThemes, selectById: selectTheme } = adapter.getSelectors(
    (state: RootState) => state.themes
);
