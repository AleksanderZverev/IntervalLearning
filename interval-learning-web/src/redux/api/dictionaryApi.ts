import { Language, Translation, Word } from '../../types/Dictionary';
import { api } from '../apiSlice';
import { addLanguages } from '../slices/languagesSlice';

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
    useAddTranslationsMutation,
    useLazySearchWordsQuery,
} = dictionaryApi;
