import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { Card } from '../../types/Collection';
import { cardsApi } from '../cardsApi';
import { RootState } from '../store';

export const getCardKey = (userId: string, collectionId: string) => `${userId}-${collectionId}`;

interface State {
    [key: string]: Card[];
}

const initialState: State = {};

export const cardsSlice = createSlice({
    name: 'cards',
    initialState,
    reducers: {
        addCard: (state, action: PayloadAction<Card>) => {
            const card = action.payload;
            const key = getCardKey(card.userId, card.collectionId);
            if (!(key in state)) {
                state[key] = [];
            }
            state[key].push(card);
        },
        addManyCards: (state, action: PayloadAction<Card[]>) => {
            const cards = action.payload;
            if (!cards || cards.length === 0) {
                return;
            }
            const collectionId = cards[0].collectionId;
            const userId = cards[0].userId;
            const key = getCardKey(userId, collectionId);
            if (!(key in state)) {
                state[key] = [];
            }
            state[key].push(...cards);
        },
    },
});

export const selectCards = (state: RootState, userId?: string, collectionId?: string) => {
    if (userId === undefined || collectionId === undefined) {
        return [];
    }

    const cardsIndex = state.cards;
    const key = getCardKey(userId, collectionId);
    return key in cardsIndex ? cardsIndex[key] : [];
};

export const selectCardsByIds = (state: RootState, userId?: string, collectionId?: string, cardIds?: string[]) => {
    if (userId === undefined || collectionId === undefined || cardIds == undefined) {
        return [];
    }

    const cards = selectCards(state, userId, collectionId);
    return cards.filter((c) => cardIds.includes(c.id));
};

export const { addCard, addManyCards } = cardsSlice.actions;
