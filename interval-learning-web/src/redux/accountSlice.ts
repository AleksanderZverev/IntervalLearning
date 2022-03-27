import { RegisterRequest, AuthenticateRequest, AuthenticateResponse } from '../types/Authentication';
import { createApi } from '@reduxjs/toolkit/query/react';
import { axiosBaseQuery } from './axiosBaseQuery';
import axiosInstance from '../api/axiosInstance';
import { checkIsLoggedOut, setCurrentUser, signOutUser } from './currentUserSlice';

const baseUrl = '/accounts';

export const accountSlice = createApi({
    reducerPath: 'api/accounts',
    baseQuery: axiosBaseQuery,
    endpoints: (build) => ({
        register: build.mutation<void, RegisterRequest>({
            query: (req) => ({ url: `${baseUrl}/register`, method: 'POST', data: req }),
        }),
        authenticate: build.mutation<AuthenticateResponse, AuthenticateRequest>({
            query: (req) => ({ url: `${baseUrl}/authenticate`, method: 'POST', data: req }),
            async onQueryStarted(arg, { queryFulfilled, dispatch }) {
                try {
                    const result = await queryFulfilled;
                    dispatch(setCurrentUser(result.data));
                } catch {}
            },
        }),
        refreshToken: build.query<AuthenticateResponse, void>({
            queryFn: async (args, { signal, dispatch, getState }, extraOptions, baseQuery) => {
                const isLoggedOut = checkIsLoggedOut();

                if (isLoggedOut) {
                    return { error: {} };
                }

                try {
                    const result = await axiosInstance.post(`${baseUrl}/refresh-token`);
                    return { data: result.data };
                } catch (err: unknown) {
                    return { error: err };
                }
            },
            async onQueryStarted(arg, { queryFulfilled, dispatch }) {
                try {
                    const result = await queryFulfilled;
                    dispatch(setCurrentUser(result.data));
                } catch {
                    dispatch(signOutUser());
                }
            },
        }),
    }),
});

export const { useRegisterMutation, useAuthenticateMutation, useRefreshTokenQuery } = accountSlice;
