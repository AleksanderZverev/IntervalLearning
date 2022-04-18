import { BaseQueryFn } from '@reduxjs/toolkit/query/react';
import { AxiosError, AxiosRequestConfig } from 'axios';
import axiosInstance from '../api/axiosInstance';
import { accountSlice } from './accountSlice';
import { setError } from './errorSlice';
import { AppDispatch } from './store';

interface CustomQueryArgs extends AxiosRequestConfig {
    // url: string;
    // method: AxiosRequestConfig['method'];
    // data?: AxiosRequestConfig['data'];
    // params?: AxiosRequestConfig['params'];
    onSuccess?: (dispatch: AppDispatch, data: unknown) => Promise<void>;
}

export type CustomBaseQueryType = BaseQueryFn<CustomQueryArgs, unknown, unknown>;

export const axiosBaseQuery: CustomBaseQueryType = async (
    { onSuccess, ...args },
    { signal, dispatch, getState },
    extraOptions
) => {
    try {
        const result = await axiosInstance.request(args);
        if (onSuccess) {
            try {
                await onSuccess(dispatch, result.data);
            } catch (e) {
                console.error('Error in onSuccess method', e);
                throw e;
            }
        }

        return { data: result.data };
    } catch (error: unknown) {
        const err = error as AxiosError;

        if (err.response?.status === 401) {
            try {
                const requestRefreshToken = dispatch(accountSlice.endpoints.refreshToken.initiate());
                const refreshTokenResponse = await requestRefreshToken;

                if (refreshTokenResponse.isError) {
                    dispatch(setError({ code: 401, data: refreshTokenResponse.error }));
                }

                requestRefreshToken.unsubscribe();

                if (refreshTokenResponse.isSuccess) {
                    const result = await axiosInstance.request(args);
                    if (onSuccess) {
                        await onSuccess(dispatch, result.data);
                    }
                    return { data: result.data };
                }
            } catch {}
        }

        return {
            error: error, //{ status: err.response?.status, data: err.response?.data },
        };
    }
};
