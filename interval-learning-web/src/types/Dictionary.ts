export interface Word {
    id: string;
    word: string;
    pronunciation: string | null;
    languageId: string;
}

export interface Language {
    id: string;
    name: string;
    nativeLanguageName: string;
    translationLinkTitle: string | null;
    translationLink: string | null;
}

export interface Translation {
    languageId: string;
    id: string;
    translation: string;
}
