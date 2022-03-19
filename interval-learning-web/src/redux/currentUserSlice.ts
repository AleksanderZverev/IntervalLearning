import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { HYDRATE } from 'next-redux-wrapper';
import { removeAuthToken, setAuthToken } from '../api/axiosInstance';
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
            setAuthToken(action.payload.jwtToken);
        },
        signOutUser: (state, action: PayloadAction<void>) => {
            state.currentUser = null;
            removeAuthToken();
        },
    },
    extraReducers: {
        [HYDRATE]: (state, action) => {
            console.log('HYDRATE-currentUser', state, 'payload', action.payload);
            return {
                ...state,
                ...action.payload,
            };
        },
    },
});

export const { setCurrentUser, signOutUser } = currentUserSlice.actions;
export const selectCurrentUser = (state: RootState) => state.currentUser.currentUser;
