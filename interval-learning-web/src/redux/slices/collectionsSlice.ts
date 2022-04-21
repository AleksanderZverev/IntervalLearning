import { RootState } from './../store';
import { createEntityAdapter, createSlice, PayloadAction } from '@reduxjs/toolkit';
import { Collection } from '../../types/Collection';

export const getCollectionKey = (userId: string, collectionId: string) => `${userId}-${collectionId}`;

const adapter = createEntityAdapter<Collection>({ selectId: (c) => getCollectionKey(c.userId, c.id) });
const { selectAll, selectById } = adapter.getSelectors();

interface CardAddedItem {
    userId: string;
    collectionId: string;
}

export interface AddStartedCards {
    userId: string;
    collectionId: string;
    startedCards: number;
}

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
        cardAddedToCollection: (state, action: PayloadAction<CardAddedItem>) => {
            const { userId, collectionId } = action.payload;
            const key = getCollectionKey(userId, collectionId);
            const collection = selectById(state, key);
            if (collection === undefined) {
                throw new Error('collection not found');
            }
            collection.cardsCount++;
        },
        addStartedCards: (state, action: PayloadAction<AddStartedCards>) => {
            const { userId, collectionId, startedCards } = action.payload;

            const collection = selectById(state, getCollectionKey(userId, collectionId));

            if (collection) {
                collection.startedCards += startedCards;
                collection.notStartedCards -= startedCards;
            }
        },
    },
});

export const { setCollections, setOneCollection, cardAddedToCollection, addStartedCards } = collectionSlice.actions;

export const { selectCollections, selectCollectionById } = {
    selectCollections: (state: RootState) => selectAll(state.collections),
    selectCollectionById: (state: RootState, userId: string, collectionId: string) =>
        selectById(state.collections, getCollectionKey(userId, collectionId)),
};
