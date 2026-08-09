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
        addTheme: (state, action: PayloadAction<Theme>) => {
            adapter.addOne(state, action.payload);
        },
        updateTheme: (state, action: PayloadAction<Theme>) => {
            adapter.upsertOne(state, action.payload);
        },
        removeTheme: (state, action: PayloadAction<number>) => {
            adapter.removeOne(state, action.payload);
        },
    },
});

export const { setThemes, addTheme, updateTheme, removeTheme } = themeSlice.actions;

export const { selectAll: selectThemes, selectById: selectTheme } = adapter.getSelectors(
    (state: RootState) => state.themes
);
