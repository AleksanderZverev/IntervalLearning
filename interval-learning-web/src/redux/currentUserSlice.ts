import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { HYDRATE } from 'next-redux-wrapper';
import { removeAuthToken, setAuthToken } from '../api/axiosInstance';
import { User } from '../types/user';
import { RootState } from './store';

const isLoggedOutKey = 'isLoggedOut';

export const setIsLoggedOut = (isLoggedOut: boolean) => {
    if (typeof window !== 'undefined' && window?.localStorage) {
        window.localStorage.setItem(isLoggedOutKey, String(isLoggedOut));
    }
};

export const checkIsLoggedOut = (): boolean => {
    if (typeof window !== 'undefined' && window?.localStorage) {
        return window.localStorage.getItem(isLoggedOutKey) === 'true';
    }

    return false;
};

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
            setIsLoggedOut(false);
        },
        signOutUser: (state, action: PayloadAction<void>) => {
            state.currentUser = null;
            removeAuthToken();
            setIsLoggedOut(true);
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
