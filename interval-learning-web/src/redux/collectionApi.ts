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

export interface QueueCollectionResponse {
    dateToCollectionsQueue: Record<string, QueueCollection[]>;
}

export interface QueueCollection {
    collection: Collection;
    cardsToRepeatCount: number;
}

export interface GetCollectionQuery {
    collectionId: string;
}

const baseUrl = '/collections';

export const collectionsApi = api.injectEndpoints({
    endpoints: (build) => ({
        getCollections: build.query<Collection[], void>({
            query: () => ({
                method: 'GET',
                url: baseUrl,
                onSuccess: async (dispatch, data) => {
                    dispatch(setCollections(data as Collection[]));
                },
            }),
            providesTags: (result, error, arg) =>
                result ? [...result.map((c) => ({ type: tagTypes.collection, id: c.id }))] : [],
        }),
        getCollection: build.query<Collection, GetCollectionQuery>({
            query: ({ collectionId }) => ({
                url: `${baseUrl}/${collectionId}`,
                method: 'GET',
                onSuccess: async (dispatch, data) => {
                    dispatch(setOneCollection(data as Collection));
                },
            }),
            providesTags: (result, error, arg) => (result ? [{ type: tagTypes.collection, id: result.id }] : []),
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
        getQueueCollections: build.query<QueueCollectionResponse, void>({
            query: () => ({
                url: `${baseUrl}/queue`,
                method: 'GET',
                // onSuccess: async (dispatch, data) => {
                //     const response = data as QueueCollectionResponse;
                //     response.
                // },
            }),
            providesTags: [tagTypes.queueCollectionsList],
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
            providesTags: [tagTypes.notFinishedCollectionsList],
        }),
    }),
});

export const {
    useGetCollectionsQuery,
    useCreateCollectionMutation,
    useGetCollectionQuery,
    useGetNotFinishedQuery,
    useGetQueueCollectionsQuery,
} = collectionsApi;
