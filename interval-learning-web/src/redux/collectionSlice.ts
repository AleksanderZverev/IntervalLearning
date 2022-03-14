import { Collection } from '../types/Collection';
import { api, tagTypes } from './apiSlice';

const baseUrl = '/collections';

export const collectionsApi = api.injectEndpoints({
    endpoints: (build) => ({
        getCollections: build.query<Collection[], void>({
            query: () => ({ method: 'GET', url: baseUrl }),
            providesTags: (result, error, arg) =>
                result ? [...result.map((c) => ({ type: tagTypes.collection, id: c.id }))] : [],
        }),
    }),
});

export const { useGetCollectionsQuery } = collectionsApi;
