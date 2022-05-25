import { Collection } from '../types/Collection';
import { Language, Word } from '../types/Dictionary';
import { api, tagTypes } from './apiSlice';
import { setOneCollection, setCollections } from './slices/collectionsSlice';

export interface CreateCollectionItem {
    collectionId: string | undefined;
    themeId: number;
    title: string;
    isDefaultBackSide: boolean;
}

export interface GetNotFinishedResponse {
    totalCollections: number;
    canStartCollections: Collection[];
}

export interface RepeatingCollectionResponse {
    dateToRepeatingPhases: Record<string, RepeatingPhaseDto[]>;
}

export interface RepeatingPhaseDto {
    scheduleUserId: string;
    scheduleId: string;
    phaseIndex: number;
    secondsFromLastPhase: number;
    description: string | null;
    repeatingCollections: RepeatingCollectionDto[];
}

export interface RepeatingCollectionDto {
    collection: Collection;
    cardsToRepeatCount: number;
}

export interface GetNotFinishedRequest {
    scheduleUserId: string;
    scheduleId: string;
    page: number;
    count: number;
}

export interface GetCollectionQuery {
    collectionId: string;
}

export interface GetRandomWordsRequest {
    refetchToggle: number;
    collectionId: string;
}

interface GetRandomWordsResponse {
    words: Word[];
    language: Language;
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
        getNotFinished: build.query<GetNotFinishedResponse, GetNotFinishedRequest>({
            query: (req) => ({ url: `${baseUrl}/not-finished`, method: 'GET', params: req }),
            async onQueryStarted(arg, { queryFulfilled, dispatch }) {
                try {
                    const response = await queryFulfilled;

                    const { canStartCollections } = response.data;
                    dispatch(setCollections(canStartCollections));
                } catch {}
            },
            providesTags: [tagTypes.notFinishedCollectionsList],
        }),
        getQueueCollections: build.query<RepeatingCollectionResponse, void>({
            query: () => ({
                url: `${baseUrl}/repeat`,
                method: 'GET',
            }),
            providesTags: [tagTypes.queueCollectionsList],
        }),
        getRandomWords: build.query<GetRandomWordsResponse, GetRandomWordsRequest>({
            query: (req) => ({
                url: `${baseUrl}/words/random`,
                method: 'GET',
                params: { collectionId: req.collectionId },
            }),
            keepUnusedDataFor: 0,
        }),
    }),
});

export const {
    useGetCollectionsQuery,
    useCreateCollectionMutation,
    useGetCollectionQuery,
    useGetNotFinishedQuery,
    useGetQueueCollectionsQuery,
    useGetRandomWordsQuery,
} = collectionsApi;
