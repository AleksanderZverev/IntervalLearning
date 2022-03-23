import { RootState } from './../store';
import { createEntityAdapter, createSlice, PayloadAction } from '@reduxjs/toolkit';
import { Collection } from '../../types/Collection';

const adapter = createEntityAdapter<Collection>({ selectId: (c) => `${c.userId}-${c.id}` });

export const collectionSlice = createSlice({
    name: 'collections',
    initialState: adapter.getInitialState(),
    reducers: {
        setCollections: (state, action: PayloadAction<Collection[]>) => {
            adapter.setMany(state, action.payload);
        },
    },
});

export const { setCollections } = collectionSlice.actions;

export const { selectAll: selectCollections } = adapter.getSelectors((state: RootState) => state.collections);
