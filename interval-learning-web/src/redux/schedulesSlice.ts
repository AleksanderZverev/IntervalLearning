import { CreateScheduleItem } from './../types/schedule';
import { Schedule } from '../types/schedule';
import { api } from './apiSlice';
import { setSchedule, setSchedules, getScheduleId } from './slices/scheduleSlice';

const basePath = '/schedules';

interface GetMyScheduleRequest {
    myScheduleId: string;
}

interface GetScheduleRequest {
    scheduleUserId: string;
    scheduleId: string;
}

export interface UpdateScheduleRequest {
    scheduleId: string;
    data: UpdateScheduleData;
}

export interface UpdateScheduleData {
    cardsCountPerPhase: number;
    title: string;
    shortDescription: string | null;
    description: string | null;
    phases: UpdatePhaseInfo[] | null;
    defaultPhaseShortDescription: string | null;
    defaultPhaseDescription: string | null;
    defaultRepeatPhaseShortDescription: string | null;
    defaultRepeatPhaseDescription: string | null;
}

export interface UpdatePhaseInfo {
    id: string;
    shortDescription: string | null;
    description: string | null;
    isDefaultValueSide: boolean;
}

export const schedulesApi = api.injectEndpoints({
    endpoints: (build) => ({
        getSchedules: build.query<Schedule[], void>({
            query: () => ({
                url: basePath,
                method: 'GET',
                onSuccess: async (dispatch, data) => {
                    const schedules = data as Schedule[];
                    dispatch(setSchedules(schedules));
                },
            }),
        }),
        getMySchedule: build.query<Schedule, GetMyScheduleRequest>({
            query: (req) => ({
                url: `${basePath}/my/${req.myScheduleId}`,
                method: 'GET',
                onSuccess: async (dispatch, data) => {
                    const schedule = data as Schedule;
                    dispatch(setSchedule(schedule));
                },
            }),
        }),
        getSchedule: build.query<Schedule, GetScheduleRequest>({
            query: (req) => ({
                url: `${basePath}/${req.scheduleUserId}/${req.scheduleId}`,
                method: 'GET',
                onSuccess: async (dispatch, data) => {
                    const schedule = data as Schedule;
                    dispatch(setSchedule(schedule));
                },
            }),
        }),
        updateSchedule: build.mutation<Schedule, UpdateScheduleRequest>({
            query: (item) => ({
                url: `${basePath}/${item.scheduleId}`,
                method: 'PATCH',
                data: item.data,
            }),
            async onQueryStarted(arg, { queryFulfilled, dispatch }) {
                try {
                    const data = await queryFulfilled;
                    dispatch(setSchedule(data.data));
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

export const {
    useGetSchedulesQuery,
    useCreateScheduleMutation,
    useGetMyScheduleQuery,
    useGetScheduleQuery,
    useUpdateScheduleMutation,
} = schedulesApi;
