import { createEntityAdapter, createSlice, PayloadAction } from '@reduxjs/toolkit';
import { Schedule } from '../../types/schedule';
import { RootState } from '../store';

export const getScheduleId = (userId: string, scheduleId: string | number) => `${userId}-${scheduleId}`;

const adapter = createEntityAdapter<Schedule>({ selectId: (s) => getScheduleId(s.userId, s.id) });
const initialState = adapter.getInitialState();

function prepareSchedule(schedule: Schedule): Schedule {
    const newSchedule = { ...schedule };
    newSchedule.phases = [...schedule.phases].sort((f, s) => parseInt(f.id) - parseInt(s.id));
    return newSchedule;
}

export const scheduleSlice = createSlice({
    name: 'schedules',
    initialState,
    reducers: {
        setSchedule: {
            reducer: (state, action: PayloadAction<Schedule>) => {
                adapter.setOne(state, action.payload);
            },
            prepare: (schedule: Schedule) => {
                return { payload: prepareSchedule(schedule) };
            },
        },
        setSchedules: {
            reducer: (state, action: PayloadAction<Schedule[]>) => {
                adapter.setMany(state, action.payload);
            },
            prepare: (schedules: Schedule[]) => {
                return { payload: schedules.map(prepareSchedule) };
            },
        },
    },
});

export const { setSchedule, setSchedules } = scheduleSlice.actions;

export const { selectAll: selectSchedules, selectById: selectScheduleById } = adapter.getSelectors(
    (state: RootState) => state.schedules
);
