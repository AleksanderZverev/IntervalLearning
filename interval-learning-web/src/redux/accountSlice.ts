import { RegisterRequest, AuthenticateRequest, AuthenticateResponse } from '../types/Authentication';
import { api } from './apiSlice';

const baseUrl = '/accounts';

export const accountSlice = api.injectEndpoints({
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
