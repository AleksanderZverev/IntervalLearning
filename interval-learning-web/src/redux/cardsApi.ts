import { PhaseInfo } from '../types/schedule';
import { Card } from '../types/Collection';
import { api, tagTypes } from './apiSlice';
import { addCard, addManyCards, deleteCard, getCardUniqueKey } from './slices/cardsSlice';
import {
    addStartedCards,
    cardAddedToCollection,
    cardDeletedFromCollection,
    getCollectionKey,
} from './slices/collectionsSlice';

interface BaseRequestItem<T> {
    userId: string;
    collectionId: string;
    request: T;
}

export interface CreateCardItem {
    cardId: string | undefined;
    frontText: string;
    promptText: string | null;
    backText: string;
    description: string | null;
    examples: string[] | null;
}

export interface GetCardItem {
    page: number;
    count: number;
}

export enum SearchFieldType {
    RememberingText = 'RememberingText',
    PromptText = 'PromptText',
    MeaningText = 'MeaningText',
}

export interface SearchCardsItem {
    searchValue: string;
    page: number;
    count: number;
    fieldType: SearchFieldType;
}

export interface CardIdsList {
    cardIds: string[];
}

export interface CardsItem {
    scheduleUserId: string;
    scheduleId: string;
    cardIds: string[];
}

interface GetRepeatCardsRequest {
    scheduleUserId: string;
    scheduleId: string;
    phaseIndex: number;
    date: string;
}

export interface RememberRequest {
    rememberItems: RememberItem[];
    scheduleUserId: string;
    scheduleId: string;
    phaseIndex: number;
}

export interface RememberItem {
    cardId: string;
    weight: number;
}

export interface StartCardResponse {
    nextRepeatDate: string | null;
    nextRepeatPhase: PhaseInfo | null;
    nextPhaseIndex: number;
    cardMovementInfos: CardMovementInfo[];
}

export interface CardMovementInfo {
    cardIds: string[];
    nextRepetitionDate: string;
}

export interface RememberCardResponse {
    nextRepeatDate: string | null;
    nextRepeatPhase: PhaseInfo | null;
    nextPhaseIndex: number;
    cardMovementInfos: CardMovementInfo[];
}

export interface GetNotStartedCardsRequest {
    scheduleUserId: string;
    scheduleId: string;
    count: number;
}

export interface MoveCardRequest {
    destinationCollectionId: string;
    cardId: string;
}

export interface DeleteCardRequest {
    cardId: string;
}

export interface GetCardRequest {
    cardId: string;
}

export interface RelearnCardRequest {
    cardId: string;
    scheduleUserId?: string;
    scheduleId?: string;
}

export interface GetRelearningCardsRequest {
    count: number;
}

export interface StopRepeatingCardRequest {
    cardId: string;
    scheduleUserId: string;
    scheduleId: string;
}

export interface PostponeRepeatingCardRequest {
    cardId: string;
    scheduleUserId: string;
    scheduleId: string;
    postponeDays: number;
}

export const cardsApi = api.injectEndpoints({
    endpoints: (build) => ({
        getCard: build.query<Card, BaseRequestItem<GetCardRequest>>({
            query: ({ collectionId, request: { cardId } }) => ({
                url: `/collections/${collectionId}/cards/${cardId}`,
                method: 'GET',
                onSuccess: async (dispatch, data) => {
                    const card = data as Card;
                    dispatch(addCard(card));
                },
            }),
            keepUnusedDataFor: 0,
        }),
        getCards: build.query<Card[], BaseRequestItem<GetCardItem>>({
            query: ({ collectionId, request }) => ({
                url: `/collections/${collectionId}/cards?page=${request.page}&count=${request.count}`,
                method: 'GET',
                onSuccess: async (dispatch, data) => {
                    const cards = data as Card[];
                    dispatch(addManyCards(cards));
                },
            }),
            providesTags: (result, error, arg) =>
                result
                    ? [
                          { type: tagTypes.collectionCards, id: getCollectionKey(arg.userId, arg.collectionId) },
                          ...result.map((c) => ({ type: tagTypes.card, id: getCardUniqueKey(c) })),
                      ]
                    : [],
        }),
        addCard: build.mutation<Card, BaseRequestItem<CreateCardItem>>({
            query: ({ collectionId, request }) => ({
                url: `/collections/${collectionId}/cards`,
                method: 'POST',
                data: request,
            }),
            async onQueryStarted(arg, { queryFulfilled, dispatch }) {
                try {
                    const newCard = await queryFulfilled;

                    dispatch(addCard(newCard.data));
                    if (!arg.request.cardId) {
                        dispatch(cardAddedToCollection({ collectionId: arg.collectionId, userId: arg.userId }));
                    }
                } catch {}
            },
            invalidatesTags: (r, e, a) => [
                tagTypes.notFinishedCollectionsList,
                { type: tagTypes.collectionCards, id: getCollectionKey(a.userId, a.collectionId) },
            ],
        }),
        getNotStartedCards: build.query<string[], BaseRequestItem<GetNotStartedCardsRequest>>({
            query: ({ collectionId, request }) => ({
                url: `/collections/${collectionId}/cards/not-started`,
                method: 'GET',
                params: request,
                onSuccess: async (dispatch, data) => {
                    dispatch(addManyCards(data as Card[]));
                },
            }),
            transformResponse: (result, meta, arg) => {
                const cards = result as Card[];
                return cards.map((c) => c.id);
            },
        }),
        startCards: build.mutation<StartCardResponse, BaseRequestItem<CardsItem>>({
            query: ({ collectionId, request }) => ({
                url: `/collections/${collectionId}/cards/start`,
                method: 'POST',
                data: request,
            }),
            async onQueryStarted(arg, { queryFulfilled, dispatch }) {
                try {
                    await queryFulfilled;
                    const { userId, collectionId, request } = arg;
                    dispatch(addStartedCards({ userId, collectionId, startedCards: request.cardIds.length }));
                } catch {}
            },
            invalidatesTags: (result, arg) =>
                result ? [tagTypes.queueCollectionsList, tagTypes.notFinishedCollectionsList] : [],
        }),
        getRepeatCards: build.query<CardIdsList, BaseRequestItem<GetRepeatCardsRequest>>({
            query: ({ collectionId, request }) => ({
                url: `/collections/${collectionId}/cards/repeat`,
                method: 'GET',
                params: request,
                onSuccess: async (dispatch, data) => {
                    const cards = data as Card[];
                    dispatch(addManyCards(cards));
                },
            }),
            transformResponse: (response: Card[], meta, arg) => {
                const item: CardIdsList = {
                    cardIds: response.map((c) => c.id),
                };
                return item;
            },
            keepUnusedDataFor: 0,
        }),
        patchRememberCards: build.mutation<RememberCardResponse, BaseRequestItem<RememberRequest>>({
            query: ({ collectionId, request }) => ({
                url: `/collections/${collectionId}/cards/remember`,
                method: 'PATCH',
                data: request,
            }),
            invalidatesTags: [tagTypes.queueCollectionsList],
        }),
        deleteCard: build.mutation<Card, BaseRequestItem<DeleteCardRequest>>({
            query: ({ collectionId, request: { cardId } }) => ({
                url: `/collections/${collectionId}/cards/${cardId}`,
                method: 'DELETE',
                onSuccess: async (dispatch, data) => {
                    const card = data as Card;
                    dispatch(deleteCard({ cardId: card.id, userId: card.userId, collectionId: card.collectionId }));
                    dispatch(cardDeletedFromCollection({ collectionId: card.collectionId, userId: card.userId }));
                },
            }),
            invalidatesTags: (r, e, a) => [
                tagTypes.notFinishedCollectionsList,
                tagTypes.queueCollectionsList,
                { type: tagTypes.collectionCards, id: getCollectionKey(a.userId, a.collectionId) },
            ],
        }),
        moveCard: build.mutation<Card, BaseRequestItem<MoveCardRequest>>({
            query: ({ collectionId, request }) => ({
                url: `/collections/${collectionId}/cards/move`,
                method: 'POST',
                data: request,
            }),
            async onQueryStarted(arg, { queryFulfilled, dispatch }) {
                try {
                    const response = await queryFulfilled;
                    dispatch(
                        deleteCard({
                            cardId: arg.request.cardId,
                            userId: arg.userId,
                            collectionId: arg.collectionId,
                        })
                    );
                    dispatch(cardDeletedFromCollection({ collectionId: arg.collectionId, userId: arg.userId }));
                    dispatch(addCard(response.data));
                    dispatch(
                        cardAddedToCollection({
                            collectionId: arg.request.destinationCollectionId,
                            userId: arg.userId,
                        })
                    );
                } catch {}
            },
            invalidatesTags: (r, e, a) => [
                tagTypes.notFinishedCollectionsList,
                tagTypes.queueCollectionsList,
                { type: tagTypes.collectionCards, id: getCollectionKey(a.userId, a.collectionId) },
                { type: tagTypes.collectionCards, id: getCollectionKey(a.userId, a.request.destinationCollectionId) },
            ],
        }),
        searchCards: build.query<Card[], BaseRequestItem<SearchCardsItem>>({
            query: ({ collectionId, request }) => ({
                url: `/collections/${collectionId}/cards/search`,
                method: 'GET',
                params: request,
                onSuccess: async (dispatch, data) => {
                    const cards = data as Card[];
                    dispatch(addManyCards(cards));
                },
            }),
            providesTags: (result, error, arg) =>
                result ? [...result.map((c) => ({ type: tagTypes.card, id: getCardUniqueKey(c) }))] : [],
        }),
        relearnCard: build.mutation<void, BaseRequestItem<RelearnCardRequest>>({
            query: ({ userId, collectionId, request: { cardId, scheduleUserId, scheduleId } }) => ({
                url: `/collections/${collectionId}/cards/relearn`,
                method: 'PATCH',
                params: { cardId: cardId, scheduleUserId: scheduleUserId ?? null, scheduleId: scheduleId ?? null },
            }),
            invalidatesTags: [tagTypes.notFinishedCollectionsList, tagTypes.queueCollectionsList],
        }),
        getRelearningCards: build.query<string[], BaseRequestItem<GetRelearningCardsRequest>>({
            query: ({ collectionId, request: { count } }) => ({
                url: `/collections/${collectionId}/cards/relearn`,
                method: 'GET',
                params: { count: count },
                onSuccess: async (dispatch, data) => {
                    const cards = data as Card[];
                    dispatch(addManyCards(cards));
                },
            }),
            transformResponse: (result, meta, arg) => {
                const cards = result as Card[];
                return cards.map((c) => c.id);
            },
            keepUnusedDataFor: 0,
        }),
        stopRepeatingCard: build.mutation<void, BaseRequestItem<StopRepeatingCardRequest>>({
            query: ({ userId, collectionId, request: { cardId, scheduleUserId, scheduleId } }) => ({
                url: `/collections/${collectionId}/cards/${cardId}/learn`,
                method: 'DELETE',
                params: { scheduleUserId: scheduleUserId, scheduleId: scheduleId },
            }),
            invalidatesTags: [tagTypes.notFinishedCollectionsList, tagTypes.queueCollectionsList],
        }),
        postponeRepeatingCard: build.mutation<void, BaseRequestItem<PostponeRepeatingCardRequest>>({
            query: ({ userId, collectionId, request: { cardId, scheduleUserId, scheduleId, postponeDays } }) => ({
                url: `/collections/${collectionId}/cards/${cardId}/learn/postpone`,
                method: 'PATCH',
                params: { scheduleUserId: scheduleUserId, scheduleId: scheduleId, postponeDays: postponeDays },
            }),
            invalidatesTags: [tagTypes.notFinishedCollectionsList, tagTypes.queueCollectionsList],
        }),
    }),
});

export const {
    useLazyGetCardQuery,
    useAddCardMutation,
    useGetCardsQuery,
    useGetNotStartedCardsQuery,
    useStartCardsMutation,
    useGetRepeatCardsQuery,
    usePatchRememberCardsMutation,
    useDeleteCardMutation,
    useMoveCardMutation,
    useSearchCardsQuery,
    useRelearnCardMutation,
    useGetRelearningCardsQuery,
    useStopRepeatingCardMutation,
    usePostponeRepeatingCardMutation,
} = cardsApi;
