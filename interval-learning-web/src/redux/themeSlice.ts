import { Theme } from '../types/global';
import { api, tagTypes } from './apiSlice';
import { setThemes } from './slices/themeSlice';

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
    }),
});

export const { useGetThemesQuery } = themesApi;
