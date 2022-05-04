import { Card } from './../types/Collection';
import { api, tagTypes } from './apiSlice';
import { addCard, addManyCards, getCardUniqueKey } from './slices/cardsSlice';
import { addStartedCards, cardAddedToCollection } from './slices/collectionsSlice';

interface BaseRequestItem<T> {
    userId: string;
    collectionId: string;
    request: T;
}

export interface CreateCardItem {
    cardId: string | undefined;
    frontText: string;
    backText: string;
    description: string | null;
    examples: string[] | null;
}

export interface GetCardItem {
    page: number;
    count: number;
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
}

export interface RememberCardResponse {
    nextRepeatDate: string | null;
}

export interface GetNotStartedCardsRequest {
    scheduleUserId: string;
    scheduleId: string;
}

export const cardsApi = api.injectEndpoints({
    endpoints: (build) => ({
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
                result ? [...result.map((c) => ({ type: tagTypes.card, id: getCardUniqueKey(c) }))] : [],
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
            invalidatesTags: [tagTypes.notFinishedCollectionsList],
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
            providesTags: [tagTypes.notStartedCardsList],
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
                result
                    ? [tagTypes.notStartedCardsList, tagTypes.queueCollectionsList, tagTypes.notFinishedCollectionsList]
                    : [],
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
            providesTags: [tagTypes.repeatCardsList],
            // providesTags: (result) => (
            //     result ? [...result.cardIds.map(cardId => {type: tagTypes.card, id: })] : []
            // )
        }),
        patchRememberCards: build.mutation<RememberCardResponse, BaseRequestItem<RememberRequest>>({
            query: ({ collectionId, request }) => ({
                url: `/collections/${collectionId}/cards/remember`,
                method: 'PATCH',
                data: request,
            }),
            invalidatesTags: [tagTypes.repeatCardsList, tagTypes.queueCollectionsList],
        }),
    }),
});

export const {
    useAddCardMutation,
    useGetCardsQuery,
    useGetNotStartedCardsQuery,
    useStartCardsMutation,
    useGetRepeatCardsQuery,
    usePatchRememberCardsMutation,
} = cardsApi;
