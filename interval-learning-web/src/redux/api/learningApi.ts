import { Card, Collection } from '../../types/Collection';
import { api } from '../apiSlice';
import { setCollections } from '../slices/collectionsSlice';
import { setQueueCards } from '../slices/queueLearnSlice';

interface LearnResponse {
    collections: Collection[];
    queueCards: QueueCard[];
}

export interface QueueCard {
    repeatDate: Date;
    card: Card;
}

const baseUrl = '/queue';

export const learningApi = api.injectEndpoints({
    endpoints: (build) => ({
        getLearningCollections: build.query<LearnResponse, void>({
            query: () => ({ url: `${baseUrl}/learn`, method: 'GET' }),
            async onQueryStarted(arg, { queryFulfilled, dispatch }) {
                try {
                    const response = await queryFulfilled;
                    dispatch(setQueueCards(response.data.queueCards));
                    dispatch(setCollections(response.data.collections));
                } catch {}
            },
        }),
    }),
});

export const { useGetLearningCollectionsQuery } = learningApi;
