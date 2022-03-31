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
        createCollection: build.mutation<Collection, CreateCollectionItem>({
            query: (item) => ({ method: 'POST', url: baseUrl, data: item }),
            async onQueryStarted(arg, { queryFulfilled, dispatch }) {
                try {
                    const collection = await queryFulfilled;
                    dispatch(setOneCollection(collection.data));
                } catch {}
            },
        }),
    }),
});

export const { useGetCollectionsQuery, useCreateCollectionMutation } = collectionsApi;
