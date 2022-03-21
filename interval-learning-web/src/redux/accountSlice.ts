import { RegisterRequest, AuthenticateRequest, AuthenticateResponse } from '../types/Authentication';
import { api } from './apiSlice';
import { createApi } from '@reduxjs/toolkit/query/react';
import { axiosBaseQuery } from './axiosBaseQuery';

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
        }),
        refreshToken: build.query<AuthenticateResponse, void>({
            query: () => ({ url: `${baseUrl}/refresh-token`, method: 'POST' }),
        }),
    }),
});

export const { useRegisterMutation, useAuthenticateMutation, useRefreshTokenQuery } = accountSlice;
