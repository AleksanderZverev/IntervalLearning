export interface Collection {
    userId: string;
    id: string;
    title: string;
    createdAt: string;
    defaultScheduleUserId: string;
    defaultScheduleId: number;
    themeId: string;
    cards: Card[];
}

export interface Card {
    userId: string;
    collectionId: string;
    id: string;
    backSideText: string;
    frontSideText: string;
    createdDate: string;
    isFinished: boolean | null;
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
    phaseStep: number;
    pPassedSecondsFromLastStep: number;
}
