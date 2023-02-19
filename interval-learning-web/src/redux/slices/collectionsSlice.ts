import { RootState } from '../store';
import { createEntityAdapter, createSlice, PayloadAction } from '@reduxjs/toolkit';
import { Collection } from '../../types/Collection';

export const getCollectionKey = (userId: string, collectionId: string) => `${userId}-${collectionId}`;

const adapter = createEntityAdapter<Collection>({ selectId: (c) => getCollectionKey(c.userId, c.id) });
const { selectAll, selectById } = adapter.getSelectors();

interface CardChangedItem {
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
        cardAddedToCollection: (state, action: PayloadAction<CardChangedItem>) => {
            const { userId, collectionId } = action.payload;
            const key = getCollectionKey(userId, collectionId);
            const collection = selectById(state, key);

            if (!collection) {
                console.error('REDUX: collection not found');
                return;
            }

            adapter.updateOne(state, {
                id: key,
                changes: {
                    cardsCount: collection.cardsCount + 1,
                    notStartedCards: collection.notStartedCards + 1,
                },
            });
        },
        cardDeletedFromCollection: (state, action: PayloadAction<CardChangedItem>) => {
            const {collectionId, userId} = action.payload;
            const key = getCollectionKey(userId, collectionId);
            const collection = selectById(state, key);

            if (!collection) {
                console.error('REDUX: collection not found');
                return;
            }

            adapter.updateOne(state, {
                id: key,
                changes: {
                    cardsCount: collection.cardsCount - 1,
                },
            });
        },
        addStartedCards: (state, action: PayloadAction<AddStartedCards>) => {
            const { userId, collectionId, startedCards } = action.payload;
            const key = getCollectionKey(userId, collectionId);
            const collection = selectById(state, getCollectionKey(userId, collectionId));

            if (!collection) {
                console.error('REDUX: collection not found');
                return;
            }
            adapter.updateOne(state, {
                id: key,
                changes: {
                    // startedCards: collection.startedCards + startedCards,
                    notStartedCards: collection.notStartedCards - startedCards,
                },
            });
        },
    },
});

export const { setCollections, setOneCollection, cardAddedToCollection, addStartedCards, cardDeletedFromCollection } = collectionSlice.actions;

export const { selectCollections, selectCollectionById } = {
    selectCollections: (state: RootState) => selectAll(state.collections),
    selectCollectionById: (state: RootState, userId: string, collectionId: string) =>
        selectById(state.collections, getCollectionKey(userId, collectionId)),
};
