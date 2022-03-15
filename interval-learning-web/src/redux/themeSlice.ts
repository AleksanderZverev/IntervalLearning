import { Theme } from '../types/global';
import { api, tagTypes } from './apiSlice';
import { RootState } from './store';

type ThemesApiState = Record<Theme['id'], Theme>;

export const themesApi = api.injectEndpoints({
    endpoints: (build) => ({
        getThemes: build.query<ThemesApiState, void>({
            query: () => ({ method: 'GET', url: 'themes' }),
            transformResponse: (result: Theme[], meta, arg) => {
                const state: ThemesApiState = {};

                for (const theme of result) {
                    state[theme.id] = theme;
                }

                return state;
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
export const selectThemes = (state: RootState): ThemesApiState =>
    themesApi.endpoints.getThemes.select()(state).data || {};
