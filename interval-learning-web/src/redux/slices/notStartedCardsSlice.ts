import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { Card } from '../../types/Collection';
import { RootState } from '../store';

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

export const { setNotStartedCards } = notStartedCardsSlice.actions;
