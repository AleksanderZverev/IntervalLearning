import { createEntityAdapter, createSlice, PayloadAction } from '@reduxjs/toolkit';
import { RootState } from "../store";
import { Course, Topic } from "../../types/Course";

export const getCourseKey = (courseId: string) => courseId;
const adapter = createEntityAdapter<Course>({ selectId: (c) => getCourseKey(c.id) });
const { selectAll, selectById } = adapter.getSelectors();

export const coursesSlice = createSlice({
    name: 'courses',
    initialState: adapter.getInitialState(),
    reducers: {
        setCourses: (state, action: PayloadAction<Course[]>) => {
            adapter.setMany(state, action.payload);
        },
        setCourse: (state, action: PayloadAction<Course>) => {
            adapter.setOne(state, action.payload);
        },
        deleteCourse: (state, action: PayloadAction<Course>) => {
            adapter.removeOne(state, action.payload.id);
        },
        addTopicToCourse: (state, action: PayloadAction<Topic>) => {
            const course = selectById(state, action.payload.parentCourseId);
            if (!course)
                throw new Error(`Course not found`)
            adapter.updateOne(state, {
                id: action.payload.parentCourseId,
                changes: {
                    topics: [...(course?.topics ?? []), action.payload],
                },
            });
        },
    }
});

export const { setCourses, setCourse, deleteCourse, addTopicToCourse } = coursesSlice.actions;

export const { selectCourses, selectCourse } = {
    selectCourses: (state: RootState) => selectAll(state.courses),
    selectCourse: (state: RootState, courseId: string): Course | undefined => {
        return selectById(state.courses, getCourseKey(courseId))
    },
};