import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { QueueCard } from '../api/learningApi';
import { RootState } from '../store';

const initialState: QueueCard[] = [];

export const queueLearnSlice = createSlice({
    name: 'queue/learn',
    initialState,
    reducers: {
        setCards: (state, action: PayloadAction<QueueCard[]>) => {
            const queueCards = action.payload;
            if (queueCards.length > 0) {
                state = queueCards;
            }
        },
    },
});

export const { setCards: setQueueCards } = queueLearnSlice.actions;

export const selectQueueCards = (state: RootState): QueueCard[] => state.queueLearn;
