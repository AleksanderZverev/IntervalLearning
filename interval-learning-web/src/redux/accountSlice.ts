import { RegisterRequest } from '../types/Authentication';
import { api } from './apiSlice';

const baseUrl = '/accounts';

export const accountSlice = api.injectEndpoints({
    endpoints: (build) => ({
        register: build.mutation<void, RegisterRequest>({
            query: (req) => ({ url: `${baseUrl}/register`, method: 'POST', data: req }),
        }),
    }),
});

export const { useRegisterMutation } = accountSlice;
