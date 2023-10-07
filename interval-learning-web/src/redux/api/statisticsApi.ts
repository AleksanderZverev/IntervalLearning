import { LearningStatistic } from '../../types/Collection';
import { api } from '../apiSlice';

export interface GetLearningStatisticRequest {
    date: string;
}

const basePath = '/statistics';

export const statisticsApi = api.injectEndpoints({
    endpoints: (build) => ({
        getStatistic: build.query<LearningStatistic, GetLearningStatisticRequest>({
            query: ({ date }) => ({
                url: `${basePath}/learning`,
                method: 'GET',
                params: { date: date },
            }),
        }),
    }),
});

export const { useGetStatisticQuery } = statisticsApi;
