import { createApi, BaseQueryFn } from '@reduxjs/toolkit/query/react';
import { AxiosRequestConfig } from 'axios';
import { HYDRATE } from 'next-redux-wrapper';
import axiosInstance from '../api/axiosInstance';

interface CustomQueryArgs {
    url: string;
    method: AxiosRequestConfig['method'];
    data?: AxiosRequestConfig['data'];
}

export type CustomBaseQueryType = BaseQueryFn<CustomQueryArgs, unknown, unknown>;

const axiosBaseQuery: CustomBaseQueryType = async (args, { signal, dispatch, getState }, extraOptions) => {
    try {
        const result = await axiosInstance.request(args);
        return { data: result.data };
    } catch (error: unknown) {
        return {
            error: error, //{ status: err.response?.status, data: err.response?.data },
        };
    }
};

export const tagTypes = {
    collection: 'Collection',
    theme: 'Theme',
} as const;

export type TagType = typeof tagTypes[keyof typeof tagTypes];

export const api = createApi({
    reducerPath: 'api',
    baseQuery: axiosBaseQuery,
    tagTypes: Array.from(Object.values(tagTypes)),
    endpoints: () => ({}),
});
