import { Card } from './../types/Collection';
import { api, tagTypes } from './apiSlice';
import { addCard, addManyCards, getCardUniqueKey } from './slices/cardsSlice';
import { addStartedCards, cardAddedToCollection } from './slices/collectionsSlice';
import { setNotStartedCards } from './slices/notStartedCardsSlice';

interface BaseRequestItem<T> {
    userId: string;
    collectionId: string;
    request: T;
}

export interface CreateCardItem {
    frontText: string;
    backText: string;
    scheduleUserId: string;
    scheduleId: number;
    description: string | null;
    examples: string[] | null;
}

export interface GetCardItem {
    page: number;
    count: number;
}

export interface CardsItem {
    cardIds: string[];
}

interface GetRepeatCardsRequest {
    date: string;
}

export interface RememberRequest {
    rememberItems: RememberItem[];
    date: string;
}

export interface StartCardResponse {
    nextRepeatDate: string | null;
}

export interface RememberCardResponse {
    nextRepeatDate: string | null;
}

interface RememberItem {
    cardId: string;
    weight: number;
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
                    dispatch(cardAddedToCollection({ collectionId: arg.collectionId, userId: arg.userId }));
                } catch {}
            },
        }),
        getNotStartedCards: build.query<Card[], BaseRequestItem<undefined>>({
            query: ({ collectionId, request }) => ({
                url: `/collections/${collectionId}/cards/not-started`,
                method: 'GET',
                onSuccess: async (dispatch, data) => {
                    dispatch(setNotStartedCards(data as Card[]));
                    dispatch(addManyCards(data as Card[]));
                },
            }),
            providesTags: [tagTypes.notStartedCardsList],
        }),
        getRepeatCards: build.query<CardsItem, BaseRequestItem<GetRepeatCardsRequest>>({
            query: ({ collectionId, request }) => ({
                url: `/queue/${collectionId}/cards/repeat`,
                method: 'GET',
                params: { date: request.date },
                onSuccess: async (dispatch, data) => {
                    const cards = data as Card[];
                    dispatch(addManyCards(cards));
                },
            }),
            transformResponse: (response: Card[], meta, arg) => {
                const item: CardsItem = {
                    cardIds: response.map((c) => c.id),
                };
                return item;
            },
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
