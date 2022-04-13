import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { Card } from '../../types/Collection';
import { RootState } from '../store';
import { getCardKey } from './cardsSlice';

interface State {
    notStartedCards: Card[];
}

const initialState: State = {
    notStartedCards: [],
};

export const notStartedCardsSlice = createSlice({
    name: 'cards/not-started',
    initialState,
    reducers: {
        setNotStartedCards: (state, action: PayloadAction<Card[]>) => {
            const cards = action.payload;
            state.notStartedCards = cards;
        },
    },
});

export const selectNotStartedCardsIds = (state: RootState) => state.notStartedCards.notStartedCards;
export const isNotStartedCardsIdsAdded = (state: RootState) => selectNotStartedCardsIds(state).length !== 0;

export const { setNotStartedCards } = notStartedCardsSlice.actions;
