export interface Collection {
    userId: string;
    id: string;
    title: string;
    createdAt: string;
    defaultScheduleId: string;
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
    UserId: string;
    CollectionId: string;
    CardId: string;
    Id: string;
    weight: number;
    phaseStep: number;
    pPassedSecondsFromLastStep: number;
}
