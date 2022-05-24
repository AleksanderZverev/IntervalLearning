import { RootState } from './../store';
import { Language } from './../../types/Dictionary';
import { createSlice, createEntityAdapter, PayloadAction } from '@reduxjs/toolkit';
import { ssrEntries } from 'next/dist/build/webpack/plugins/middleware-plugin';

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

export const { selectAll: selectLanguages } = adapter.getSelectors<RootState>((s) => s.languages);
