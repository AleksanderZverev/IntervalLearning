import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { User } from '../types/user';
import { RootState } from './store';

interface State {
    currentUser: User | null;
}
const initialState: State = { currentUser: null };

export const currentUserSlice = createSlice({
    name: 'currentUser',
    initialState,
    reducers: {
        setCurrentUser: (state, action: PayloadAction<User>) => {
            state.currentUser = { ...action.payload };
        },
        signOutUser: (state, action: PayloadAction<void>) => {
            state.currentUser = null;
        },
    },
});

export const { setCurrentUser, signOutUser } = currentUserSlice.actions;
export const selectCurrentUser = (state: RootState) => state.currentUser.currentUser;
