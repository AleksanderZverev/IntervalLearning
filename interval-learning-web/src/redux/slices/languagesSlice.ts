import { RootState } from './../store';
import { Language } from './../../types/Dictionary';
import { createSlice, createEntityAdapter, PayloadAction } from '@reduxjs/toolkit';

const adapter = createEntityAdapter<Language>({ selectId: (l) => l.id });

export const languagesSlice = createSlice({
    name: 'dictionary/languages',
    initialState: adapter.getInitialState(),
    reducers: {
        addLanguages: (state, action: PayloadAction<Language[]>) => {
            adapter.addMany(state, action.payload);
        },
        addLanguage: (state, action: PayloadAction<Language>) => {
            adapter.addOne(state, action.payload);
        },
        updateLanguage: (state, action: PayloadAction<Language>) => {
            adapter.updateOne(state, { id: action.payload.id, changes: action.payload });
        },
        removeLanguage: (state, action: PayloadAction<string>) => {
            adapter.removeOne(state, action.payload);
        },
    },
});

export const { addLanguages, addLanguage, updateLanguage, removeLanguage } = languagesSlice.actions;

export const { selectAll: selectLanguages, selectById: selectLanguageById } = adapter.getSelectors<RootState>(
    (s) => s.languages
);
