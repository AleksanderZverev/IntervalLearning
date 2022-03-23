import { createEntityAdapter, createSlice, PayloadAction } from '@reduxjs/toolkit';
import { Schedule } from '../../types/schedule';
import { RootState } from '../store';

const scheduleAdapter = createEntityAdapter<Schedule>({ selectId: (s) => `${s.userId}-${s.id}` });
const initialState = scheduleAdapter.getInitialState();

export const scheduleSlice = createSlice({
    name: 'schedules',
    initialState,
    reducers: {
        setSchedule: (state, action: PayloadAction<Schedule>) => {
            scheduleAdapter.setOne(state, action.payload);
        },
    },
});

export const { setSchedule } = scheduleSlice.actions;

export const { selectAll: selectSchedules } = scheduleAdapter.getSelectors((state: RootState) => state.schedules);
