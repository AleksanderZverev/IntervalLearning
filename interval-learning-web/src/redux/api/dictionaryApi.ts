import { Translation } from '../../types/Dictionary';
import { api } from '../apiSlice';

interface GetWordTranslationsRequest {
    word: string;
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
    }),
});

export const { useLazyGetWordTranslationsQuery } = dictionaryApi;
