import { Collection, StoreCollection } from '../types/Collection';
import { Language, Word } from '../types/Dictionary';
import { api, tagTypes } from './apiSlice';
import { setOneCollection, setCollections, deleteOneCollection } from './slices/collectionsSlice';

export interface CreateCollectionItem {
    collectionId: string | undefined;
    themeId: number;
    title: string;
    isDefaultBackSide: boolean;
}

export interface GetNotFinishedResponse {
    totalCollections: number;
    canStartCollections: Collection[];
    canRelearnCollections: Collection[];
}

export interface RepeatingCollectionResponse {
    dateToRepeatingPhases: Record<string, RepeatingPhaseDto[]>;
}

export interface GetRepeatCollectionsResponseV2 {
    parentUserId: string;
    scheduleId: string;
    lateCollections: RepeatingCollectionInfoDto[];
    repeatingForgottenWordsCollections: RepeatingCollectionInfoDto[];
    repeatingInfosByDate: RepeatingInfoByDateDto[];
}

export interface RepeatingInfoByDateDto {
    date: string;
    repeatingCollections: RepeatingCollectionInfoDto[];
}

export interface RepeatingCollectionInfoDto {
    collectionId: string;
    collectionUserId: string;
    collectionTitle: string;
    themeId: number;
    isRepeatingForgottenWords: boolean;
    isRepeatable: boolean;
    cardsCount: number;
    earliestDateToRepeat: string;
    oldestDateToRepeat: string;
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
    isRepeatable: boolean;
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

export interface MakePublicRequest {
    collectionId: string;
}

export interface GetRandomWordsRequest {
    refetchToggle: number;
    collectionId: string;
}

export interface SearchPublicCollectionRequest {
    themeId: number;
    searchName: string;
    page: number;
    count: number;
}

interface GetRandomWordsResponse {
    words: Word[];
    language: Language;
}

interface GetQueueCollectionsRequest {
    untilDate?: string;
}

interface GetQueueCollectionsRequestV2 {
    untilDate?: string;
    scheduleUserId: string;
    scheduleId: string;
    userCurrentDateTime: string;
}

export interface AddCardsToMyCollectionRequest {
    publicCollectionUserId: string;
    publicCollectionId: string;
    data: {
        checkUnique: boolean;
        myCollectionId: string | null | undefined;
        newCollectionName: string | null | undefined;
    };
}

export interface GetCollectionStatisticResponse {
    todayAddedCards: number;
    startedLearningCards: number;
}

export interface GetCollectionStatisticRequest {
    collectionId: string;
    userCurrentDateTime: string;
}

interface DeleteCollectionRequest {
    userId: string;
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
        getQueueCollections: build.query<RepeatingCollectionResponse, GetQueueCollectionsRequest>({
            query: ({ untilDate }) => ({
                url: `${baseUrl}/repeat`,
                method: 'GET',
                params: { untilDate: untilDate ?? null },
            }),
            providesTags: [tagTypes.queueCollectionsList],
        }),
        getQueueCollectionsV2: build.query<GetRepeatCollectionsResponseV2, GetQueueCollectionsRequestV2>({
            query: ({ untilDate, scheduleUserId, scheduleId, userCurrentDateTime }) => ({
                url: `${baseUrl}/repeat-v2`,
                method: 'GET',
                params: { scheduleUserId, scheduleId, untilDate: untilDate ?? null, userCurrentDateTime },
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
        makeCollectionPublic: build.mutation<Collection, MakePublicRequest>({
            query: (req) => ({
                url: `${baseUrl}/${req.collectionId}/public`,
                method: 'POST',
                onSuccess: async (dispatch, data) => {
                    dispatch(setOneCollection(data as Collection));
                },
            }),
        }),
        addCardsToMyCollection: build.mutation<Collection, AddCardsToMyCollectionRequest>({
            query: (req) => ({
                url: `${baseUrl}/${req.publicCollectionUserId}-${req.publicCollectionId}/add`,
                method: 'POST',
                data: req.data,
                onSuccess: async (dispatch, data) => {
                    dispatch(setOneCollection(data as Collection));
                },
            }),
        }),
        searchPublicCollection: build.query<StoreCollection[], SearchPublicCollectionRequest>({
            query: (req) => ({
                url: `${baseUrl}/search`,
                method: 'GET',
                params: req,
            }),
            keepUnusedDataFor: 0,
        }),
        deleteCollection: build.mutation<void, DeleteCollectionRequest>({
            query: ({ userId, collectionId }) => ({
                url: `${baseUrl}/${collectionId}`,
                method: 'DELETE',
            }),
            async onQueryStarted(arg, { queryFulfilled, dispatch }) {
                try {
                    await queryFulfilled;
                    dispatch(deleteOneCollection({ userId: arg.userId, collectionId: arg.collectionId }));
                } catch {}
            },
        }),
        getCollectionStatistic: build.query<GetCollectionStatisticResponse, GetCollectionStatisticRequest>({
            query: ({ collectionId, userCurrentDateTime }) => ({
                url: `${baseUrl}/${collectionId}/statistic`,
                method: 'GET',
                params: { userCurrentDateTime },
            }),
        }),
    }),
});

export const {
    useGetCollectionsQuery,
    useCreateCollectionMutation,
    useGetCollectionQuery,
    useGetNotFinishedQuery,
    useGetQueueCollectionsQuery,
    useGetQueueCollectionsV2Query,
    useGetRandomWordsQuery,
    useMakeCollectionPublicMutation,
    useAddCardsToMyCollectionMutation,
    useSearchPublicCollectionQuery,
    useDeleteCollectionMutation,
    useGetCollectionStatisticQuery,
} = collectionsApi;
