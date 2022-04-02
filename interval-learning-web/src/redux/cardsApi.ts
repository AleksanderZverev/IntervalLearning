import { Card, Collection } from './../types/Collection';
import { api } from './apiSlice';
import { addCard, addManyCards } from './slices/cardsSlice';
import { cardAddedToCollection } from './slices/collectionsSlice';

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

export interface CardsPaginationResponse {
    collection: Collection | null;
    cards: Card[];
}

export const cardsApi = api.injectEndpoints({
    endpoints: (build) => ({
        getCards: build.query<CardsPaginationResponse, BaseRequestItem<GetCardItem>>({
            query: ({ collectionId, request }) => ({
                url: `/collections/${collectionId}/cards?page=${request.page}&count=${request.count}`,
                method: 'GET',
            }),
            async onQueryStarted(arg, { queryFulfilled, dispatch }) {
                try {
                    const cards = await queryFulfilled;
                    dispatch(addManyCards(cards.data.cards));
                } catch {}
            },
        }),
        addCard: build.mutation<Card, BaseRequestItem<CreateCardItem>>({
            query: ({ collectionId, request }) => ({
                url: `/collections/${collectionId}/cards`,
                method: 'POST',
                data: request,
            }),
            async onQueryStarted(arg, { queryFulfilled, dispatch, getState }) {
                try {
                    const newCard = await queryFulfilled;

                    dispatch(addCard(newCard.data));
                    dispatch(cardAddedToCollection({ collectionId: arg.collectionId, userId: arg.userId }));
                } catch {}
            },
        }),
    }),
});

export const { useAddCardMutation, useGetCardsQuery } = cardsApi;
