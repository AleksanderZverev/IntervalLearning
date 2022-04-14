import { Card } from './../types/Collection';
import { api } from './apiSlice';
import { addCard, addManyCards } from './slices/cardsSlice';
import { cardAddedToCollection } from './slices/collectionsSlice';
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

export const cardsApi = api.injectEndpoints({
    endpoints: (build) => ({
        getCards: build.query<Card[], BaseRequestItem<GetCardItem>>({
            query: ({ collectionId, request }) => ({
                url: `/collections/${collectionId}/cards?page=${request.page}&count=${request.count}`,
                method: 'GET',
            }),
            async onQueryStarted(arg, { queryFulfilled, dispatch }) {
                try {
                    const cards = await queryFulfilled;
                    dispatch(addManyCards(cards.data));
                } catch {}
            },
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
        }),
    }),
});

export const { useAddCardMutation, useGetCardsQuery, useGetNotStartedCardsQuery } = cardsApi;
