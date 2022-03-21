import { BaseQueryFn } from '@reduxjs/toolkit/query/react';
import { AxiosError, AxiosRequestConfig } from 'axios';
import axiosInstance from '../api/axiosInstance';
import { accountSlice } from './accountSlice';
import { setCurrentUser } from './currentUserSlice';

interface CustomQueryArgs {
    url: string;
    method: AxiosRequestConfig['method'];
    data?: AxiosRequestConfig['data'];
}

export type CustomBaseQueryType = BaseQueryFn<CustomQueryArgs, AxiosError, unknown>;

export const axiosBaseQuery: CustomBaseQueryType = async (args, { signal, dispatch, getState }, extraOptions) => {
    try {
        const result = await axiosInstance.request(args);
        return { data: result.data };
    } catch (error: unknown) {
        const err = error as AxiosError;

        if (err.response?.status === 401) {
            try {
                const requestRefreshToken = dispatch(accountSlice.endpoints.refreshToken.initiate());

                try {
                    const authenticatedUser = await requestRefreshToken.unwrap();
                    dispatch(setCurrentUser(authenticatedUser));
                } finally {
                    requestRefreshToken.unsubscribe();
                }

                const result = await axiosInstance.request(args);
                return { data: result.data };
            } catch {}
        }

        return {
            error: error, //{ status: err.response?.status, data: err.response?.data },
        };
    }
};
