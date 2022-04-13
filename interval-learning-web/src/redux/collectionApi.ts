import { Collection } from '../types/Collection';
import { api, tagTypes } from './apiSlice';
import { setOneCollection, setCollections } from './slices/collectionsSlice';

export interface CreateCollectionItem {
    scheduleUserId: string;
    scheduleId: number;
    themeId: number;
    title: string;
    isDefaultBackSide: boolean;
}

export interface GetNotFinishedResponse {
    startedCollections: Collection[];
    notStartedCollections: Collection[];
}

export interface GetNotFinishedRequest {
    page: number;
    count: number;
}

const baseUrl = '/collections';

export const collectionsApi = api.injectEndpoints({
    endpoints: (build) => ({
        getCollections: build.query<Collection[], void>({
            query: () => ({ method: 'GET', url: baseUrl }),
            providesTags: (result, error, arg) =>
                result ? [...result.map((c) => ({ type: tagTypes.collection, id: c.id }))] : [],
            async onQueryStarted(arg, { queryFulfilled, dispatch }) {
                try {
                    const result = await queryFulfilled;
                    dispatch(setCollections(result.data));
                } catch {}
            },
        }),
        getCollection: build.query<Collection, string>({
            query: (collectionId) => ({
                url: `${baseUrl}/${collectionId}`,
                method: 'GET',
            }),
            providesTags: (result, error, arg) => (result ? [{ type: tagTypes.collection, id: result.id }] : []),
            async onQueryStarted(arg, { queryFulfilled, dispatch }) {
                try {
                    const result = await queryFulfilled;
                    dispatch(setOneCollection(result.data));
                } catch {}
            },
        }),
        createCollection: build.mutation<Collection, CreateCollectionItem>({
            query: (item) => ({ method: 'POST', url: baseUrl, data: item }),
            async onQueryStarted(arg, { queryFulfilled, dispatch }) {
                try {
                    const collection = await queryFulfilled;
                    dispatch(setOneCollection(collection.data));
                } catch {}
            },
        }),
        getNotFinished: build.query<GetNotFinishedResponse, GetNotFinishedRequest>({
            query: (req) => ({ url: `${baseUrl}/not-finished?page=${req.page}&count=${req.count}`, method: 'GET' }),
            async onQueryStarted(arg, { queryFulfilled, dispatch }) {
                try {
                    const response = await queryFulfilled;
                    const { startedCollections, notStartedCollections } = response.data;

                    dispatch(setCollections(startedCollections));
                    dispatch(setCollections(notStartedCollections));
                } catch {}
            },
        }),
    }),
});

export const { useGetCollectionsQuery, useCreateCollectionMutation, useGetCollectionQuery, useGetNotFinishedQuery } =
    collectionsApi;
