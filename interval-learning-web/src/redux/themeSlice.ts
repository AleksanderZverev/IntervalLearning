import { Theme } from '../types/global';
import { api, tagTypes } from './apiSlice';
import { addTheme, removeTheme, setThemes, updateTheme } from './slices/themeSlice';

export interface ThemeRequest {
    name: string;
}

export const themesApi = api.injectEndpoints({
    endpoints: (build) => ({
        getThemes: build.query<Theme[], void>({
            query: () => ({
                method: 'GET',
                url: 'themes',
                onSuccess: async (dispatch, data) => {
                    const themes = data as Theme[];
                    dispatch(setThemes(themes));
                },
            }),
            providesTags: [tagTypes.themes],
        }),
        createTheme: build.mutation<void, ThemeRequest>({
            query: (data) => ({
                method: 'POST',
                url: 'themes',
                data,
            }),
            invalidatesTags: [tagTypes.themes],
        }),
        updateTheme: build.mutation<Theme, { id: number; data: ThemeRequest }>({
            query: ({ id, data }) => ({
                method: 'PUT',
                url: `themes/${id}`,
                data,
            }),
            onQueryStarted: async ({ id, data }, { dispatch, queryFulfilled }) => {
                const { data: updated } = await queryFulfilled;
                dispatch(updateTheme(updated));
            },
        }),
        deleteTheme: build.mutation<void, number>({
            query: (id) => ({
                method: 'DELETE',
                url: `themes/${id}`,
            }),
            onQueryStarted: async (id, { dispatch, queryFulfilled }) => {
                await queryFulfilled;
                dispatch(removeTheme(id));
            },
        }),
    }),
});

export const { useGetThemesQuery, useCreateThemeMutation, useUpdateThemeMutation, useDeleteThemeMutation } = themesApi;
