import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { Card } from '../../types/Collection';
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

export const selectCards = (state: RootState, userId: string, collectionId: string) => {
    const cardsIndex = state.cards;
    const key = getCardKey(userId, collectionId);
    return key in cardsIndex ? cardsIndex[key] : [];
};

export const { addCard, addManyCards } = cardsSlice.actions;
