import { Theme } from '../types/global';
import { api, tagTypes } from './apiSlice';
import { setThemes } from './slices/themeSlice';

export const themesApi = api.injectEndpoints({
    endpoints: (build) => ({
        getThemes: build.query<Theme[], void>({
            query: () => ({ method: 'GET', url: 'themes' }),
            async onQueryStarted(arg, { queryFulfilled, dispatch }) {
                try {
                    const themes = await queryFulfilled;
                    dispatch(setThemes(themes.data));
                } catch {}
            },
            providesTags: (result, error, arg) => {
                const tags = result
                    ? Object.keys(result).map((themeId) => ({ type: tagTypes.theme, id: themeId }))
                    : [];
                tags.push({ type: tagTypes.theme, id: 'LIST' });
                return tags;
            },
        }),
    }),
});

export const { useGetThemesQuery } = themesApi;
