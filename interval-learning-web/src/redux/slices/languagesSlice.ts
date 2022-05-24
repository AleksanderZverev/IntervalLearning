import { RootState } from './../store';
import { Language } from './../../types/Dictionary';
import { createSlice, createEntityAdapter, PayloadAction } from '@reduxjs/toolkit';

const adapter = createEntityAdapter<Language>({ selectId: (l) => l.id });

export const languagesSlice = createSlice({
    name: 'dictionary/languages',
    initialState: adapter.getInitialState(),
    reducers: {
        addLanguages: (state, action: PayloadAction<Language[]>) => {
            const languages = action.payload;
            adapter.addMany(state, languages);
        },
    },
});

export const { addLanguages } = languagesSlice.actions;

export const { selectAll: selectLanguages, selectById: selectLanguageById } = adapter.getSelectors<RootState>(
    (s) => s.languages
);
