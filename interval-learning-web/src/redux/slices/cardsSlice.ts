import { createEntityAdapter, createSlice, PayloadAction } from '@reduxjs/toolkit';
import { Card } from '../../types/Collection';
import { cardsApi } from '../cardsApi';
import { RootState } from '../store';

export const getCardKey = (userId: string, collectionId: string) => `${userId}-${collectionId}`;

export const getCardUniqueKey = (card: Card) => getCardUniqueId(card.userId, card.collectionId, card.id);

const getCardUniqueId = (userId: string, collectionId: string, cardId: string) => `${userId}-${collectionId}-${cardId}`;

interface State {
    userToCollectionToCard: { [userId: string]: { [collectionId: string]: { [cardId: string]: Card } } };
}

const initialState: State = { userToCollectionToCard: {} };

const setCard = (state: State, card: Card) => {
    const root = state.userToCollectionToCard;

    if (!Object.hasOwn(root, card.userId)) {
        root[card.userId] = {};
    }

    const collectionsIndex = root[card.userId];

    if (!Object.hasOwn(collectionsIndex, card.collectionId)) {
        collectionsIndex[card.collectionId] = {};
    }

    const collection = collectionsIndex[card.collectionId];
    collection[card.id] = card;
};

export const cardsSlice = createSlice({
    name: 'cards',
    initialState,
    reducers: {
        addCard: (state, action: PayloadAction<Card>) => {
            setCard(state, action.payload);
        },
        addManyCards: (state, action: PayloadAction<Card[]>) => {
            for (const card of action.payload) {
                setCard(state, card);
            }
        },
    },
});

export const selectCards = (state: RootState, userId?: string, collectionId?: string) => {
    if (userId === undefined || collectionId === undefined) {
        return [];
    }

    const collectionsIndex = state.cards.userToCollectionToCard[userId] ?? {};
    const collection = collectionsIndex[collectionId];

    return collection ? Object.values(collection) : [];
};

export const selectCardsByIds = (state: RootState, userId?: string, collectionId?: string, cardIds?: string[]) => {
    if (userId === undefined || collectionId === undefined || cardIds == undefined) {
        return [];
    }

    const cards = selectCards(state, userId, collectionId);
    return cards.filter((c) => cardIds.includes(c.id));
};

export const { addCard, addManyCards } = cardsSlice.actions;
