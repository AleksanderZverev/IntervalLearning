import { CreateScheduleItem } from './../types/schedule';
import { Schedule } from '../types/schedule';
import { api } from './apiSlice';
import { setSchedule, setSchedules } from './slices/scheduleSlice';

const basePath = '/schedules';

export const schedulesApi = api.injectEndpoints({
    endpoints: (build) => ({
        getSchedules: build.query<Schedule[], void>({
            query: () => ({ url: basePath, method: 'GET' }),
            async onQueryStarted(arg, { queryFulfilled, dispatch }) {
                try {
                    const data = await queryFulfilled;
                    dispatch(setSchedules(data.data));
                } catch {}
            },
        }),
        createSchedule: build.mutation<Schedule, CreateScheduleItem>({
            query: (item) => ({ url: basePath, method: 'POST', data: item }),
            async onQueryStarted(arg, { queryFulfilled, dispatch }) {
                try {
                    const data = await queryFulfilled;
                    dispatch(setSchedule(data.data));
                } catch {}
            },
        }),
    }),
});

export const { useGetSchedulesQuery, useCreateScheduleMutation } = schedulesApi;
