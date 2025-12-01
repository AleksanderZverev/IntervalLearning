import { CalendarLearningStatisticModel, LearningStatistic } from '../../types/Collection';
import { api, tagTypes } from '../apiSlice';

export interface GetLearningStatisticRequest {
    scheduleUserId: string;
    scheduleId: string;
    themeId: number;
    date: string;
    userCurrentDateTime: string;
}

export interface GetDetailedCalendarStatisticRequest {
    scheduleUserId: string;
    scheduleId: string;
    from: string;
    to: string;
    timezoneOffsetInMinutes: number;
}

const basePath = '/statistics';

export const statisticsApi = api.injectEndpoints({
    endpoints: (build) => ({
        getStatistic: build.query<LearningStatistic, GetLearningStatisticRequest>({
            query: ({ date, scheduleId, scheduleUserId, themeId, userCurrentDateTime }) => ({
                url: `${basePath}/learning`,
                method: 'GET',
                params: { date: date, scheduleId, scheduleUserId, themeId, userCurrentDateTime },
            }),
            providesTags: [tagTypes.learningStatistic],
        }),
        getDetailedCalendarStatistic: build.query<CalendarLearningStatisticModel, GetDetailedCalendarStatisticRequest>({
            query: (request) => ({
                url: `${basePath}/calendar/detailed`,
                method: 'GET',
                params: request,
            }),
        }),
    }),
});

export const { useGetStatisticQuery, useGetDetailedCalendarStatisticQuery } = statisticsApi;
