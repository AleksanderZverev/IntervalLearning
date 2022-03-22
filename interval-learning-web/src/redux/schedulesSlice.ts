import { Schedule } from '../types/schedule';
import { api } from './apiSlice';

const basePath = '/schedules';

export const schedulesApi = api.injectEndpoints({
    endpoints: (build) => ({
        getSchedules: build.query<Schedule[], void>({
            query: () => ({ url: basePath, method: 'GET' }),
        }),
    }),
});

export const { useGetSchedulesQuery } = schedulesApi;
