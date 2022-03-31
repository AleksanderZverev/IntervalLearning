import { createEntityAdapter, createSlice, PayloadAction } from '@reduxjs/toolkit';
import { Schedule } from '../../types/schedule';
import { RootState } from '../store';

export const getScheduleId = (userId: string, scheduleId: number) => `${userId}-${scheduleId}`;

const adapter = createEntityAdapter<Schedule>({ selectId: (s) => getScheduleId(s.userId, s.id) });
const initialState = adapter.getInitialState();

export const scheduleSlice = createSlice({
    name: 'schedules',
    initialState,
    reducers: {
        setSchedule: (state, action: PayloadAction<Schedule>) => {
            adapter.setOne(state, action.payload);
        },
        setSchedules: (state, action: PayloadAction<Schedule[]>) => {
            adapter.setMany(state, action.payload);
        },
    },
});

export const { setSchedule, setSchedules } = scheduleSlice.actions;

export const { selectAll: selectSchedules, selectById: selectScheduleById } = adapter.getSelectors(
    (state: RootState) => state.schedules
);
