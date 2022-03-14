import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { RootState } from './store';

interface Error {
    code: number;
    data: unknown;
}

const initialState = {
    errors: [] as Error[],
};

export const errorSlice = createSlice({
    name: 'errors',
    initialState,
    reducers: {
        setError: (state, action: PayloadAction<Error>) => {
            state.errors.push(action.payload);
        },
        clearErrors: (state, action: PayloadAction<void>) => {
            state.errors = [];
        },
    },
});

export const selectErrors = (state: RootState) => state.errors.errors;

export const { setError, clearErrors } = errorSlice.actions;
