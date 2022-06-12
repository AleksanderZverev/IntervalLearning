export interface Collection {
    userId: string;
    id: string;
    title: string;
    createdAt: string;
    themeId: number;
    cardsCount: number;
    notStartedCards: number;
    isPublic: boolean;
    publication?: CollectionPublication;
}

export interface CollectionPublication {
    publishDate: string;
    subscribersCount: number;
    likesCount: number;
    dislikesCount: number;
}

export interface Card {
    userId: string;
    collectionId: string;
    id: string;
    backSideText: string;
    promptText: string | null;
    frontSideText: string;
    createdDate: string;
    description: string | null;
    examples: string[] | null;
    remembers: Remember[] | null;
}

export interface Remember {
    userId: string;
    collectionId: string;
    cardId: string;
    id: string;
    weight: number;
    phaseId: number;
    repeatedDate: string;
}
