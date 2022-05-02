import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { Card } from '../../types/Collection';
import { RootState } from '../store';

export const getCardKey = (userId: string, collectionId: string) => `${userId}-${collectionId}`;

export const getCardUniqueKey = (card: Card) => getCardUniqueId(card.userId, card.collectionId, card.id);

const getCardUniqueId = (userId: string, collectionId: string, cardId: string) => `${userId}-${collectionId}-${cardId}`;

interface State {
    userToCollectionToCard: { [userId: string]: { [collectionId: string]: { [cardId: string]: Card } } };
}

const initialState: State = { userToCollectionToCard: {} };

const setCard = (state: State, card: Card) => {
    const root = state.userToCollectionToCard;

    if (!(card.userId in root)) {
        root[card.userId] = {};
    }

    const collectionsIndex = root[card.userId];

    if (!(card.collectionId in collectionsIndex)) {
        collectionsIndex[card.collectionId] = {};
    }

    const collection = collectionsIndex[card.collectionId];
    collection[card.id] = card;
};

export const cardsSlice = createSlice({
    name: 'cards',
    initialState,
    reducers: {
        addCard: (state, action: PayloadAction<Card>) => {
            setCard(state, action.payload);
        },
        addManyCards: (state, action: PayloadAction<Card[]>) => {
            for (const card of action.payload) {
                setCard(state, card);
            }
        },
    },
});

export const selectCards = (state: RootState, userId?: string, collectionId?: string) => {
    if (userId === undefined || collectionId === undefined) {
        return [];
    }

    const collectionsIndex = state.cards.userToCollectionToCard[userId] ?? {};
    const cardsIndex = collectionsIndex[collectionId];

    return cardsIndex ? Object.values(cardsIndex) : [];
};

export const selectCardsByIds = (state: RootState, userId?: string, collectionId?: string, cardIds?: string[]) => {
    if (userId === undefined || collectionId === undefined || cardIds == undefined) {
        return [];
    }

    const cards = selectCards(state, userId, collectionId);
    return cards.filter((c) => cardIds.includes(c.id));
};

export const selectCardById = (
    state: RootState,
    userId: string,
    collectionId: string,
    cardId?: string
): Card | undefined => {
    if (!userId || !collectionId || !cardId) {
        return undefined;
    }

    const collectionsIndex = state.cards.userToCollectionToCard[userId] ?? {};
    const cardsIndex = collectionsIndex[collectionId] ?? {};
    return cardsIndex[cardId];
};

export const { addCard, addManyCards } = cardsSlice.actions;
