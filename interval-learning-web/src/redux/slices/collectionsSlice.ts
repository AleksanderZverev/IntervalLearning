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
        setOneCollection: (state, action: PayloadAction<Collection>) => {
            adapter.setOne(state, action.payload);
        },
    },
});

export const { setCollections, setOneCollection } = collectionSlice.actions;

export const { selectAll: selectCollections, selectById: selectCollectionById } = adapter.getSelectors(
    (state: RootState) => state.collections
);
