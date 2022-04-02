export interface Collection {
    userId: string;
    id: string;
    title: string;
    createdAt: string;
    defaultScheduleUserId: string;
    defaultScheduleId: number;
    themeId: string;
    cardsCount: number;
}

export interface Card {
    userId: string;
    collectionId: string;
    id: string;
    backSideText: string;
    frontSideText: string;
    createdDate: string;
    scheduleUserId: string;
    scheduleId: string;
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
