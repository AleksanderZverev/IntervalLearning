import { Collection } from '../types/Collection';
import { api, tagTypes } from './apiSlice';
import { setCollections } from './slices/collectionsSlice';

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
    }),
});

export const { useGetCollectionsQuery } = collectionsApi;
