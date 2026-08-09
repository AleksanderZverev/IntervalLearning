import { Language, Translation, Word } from '../../types/Dictionary';
import { api } from '../apiSlice';
import { addLanguage, addLanguages, removeLanguage, updateLanguage } from '../slices/languagesSlice';

interface GetWordTranslationsRequest {
    word: string;
}

export interface AddTranslationsRequest {
    languageId: string;
    translationLanguageId: string;
    text: string;
}

export interface SearchWordsRequest {
    word: string | null;
    pronunciation: string | null;
}

export interface LanguageRequest {
    name: string;
    nativeLanguageName: string;
    translationLink?: string | null;
    translationLinkTitle?: string | null;
}

const baseUrl = 'dictionary';

export const dictionaryApi = api.injectEndpoints({
    endpoints: (build) => ({
        getWordTranslations: build.query<Translation[], GetWordTranslationsRequest>({
            query: ({ word }) => ({
                url: `${baseUrl}/translations`,
                method: 'GET',
                params: { word },
            }),
        }),
        getLanguages: build.query<Language[], void>({
            query: () => ({
                url: `${baseUrl}/languages`,
                method: 'GET',
                onSuccess: async (dispatch, data) => {
                    const languages = data as Language[];
                    dispatch(addLanguages(languages));
                },
            }),
        }),
        createLanguage: build.mutation<Language, LanguageRequest>({
            query: (data) => ({
                url: `${baseUrl}/languages`,
                method: 'POST',
                data,
            }),
            onQueryStarted: async (_, { dispatch, queryFulfilled }) => {
                const { data: created } = await queryFulfilled;
                dispatch(addLanguage(created));
            },
        }),
        updateLanguage: build.mutation<Language, { id: string; data: LanguageRequest }>({
            query: ({ id, data }) => ({
                url: `${baseUrl}/languages/${id}`,
                method: 'PUT',
                data,
            }),
            onQueryStarted: async (_, { dispatch, queryFulfilled }) => {
                const { data: updated } = await queryFulfilled;
                dispatch(updateLanguage(updated));
            },
        }),
        deleteLanguage: build.mutation<void, string>({
            query: (id) => ({
                url: `${baseUrl}/languages/${id}`,
                method: 'DELETE',
            }),
            onQueryStarted: async (id, { dispatch, queryFulfilled }) => {
                await queryFulfilled;
                dispatch(removeLanguage(id));
            },
        }),
        addTranslations: build.mutation<string, AddTranslationsRequest>({
            query: (req) => ({
                url: `${baseUrl}/translations`,
                method: 'POST',
                data: req,
            }),
        }),
        searchWords: build.query<Word[], SearchWordsRequest>({
            query: (req) => ({
                url: `${baseUrl}/words/search`,
                method: 'GET',
                params: req,
            }),
        }),
    }),
});

export const {
    useLazyGetWordTranslationsQuery,
    useGetLanguagesQuery,
    useCreateLanguageMutation,
    useUpdateLanguageMutation,
    useDeleteLanguageMutation,
    useAddTranslationsMutation,
    useLazySearchWordsQuery,
} = dictionaryApi;
