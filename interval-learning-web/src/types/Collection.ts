export interface Collection {
    userId: string;
    id: string;
    title: string;
    createdAt: string;
    themeId: string;
    cardsCount: number;
    notStartedCards: number;
}

export interface Card {
    userId: string;
    collectionId: string;
    id: string;
    backSideText: string;
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
