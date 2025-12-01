import { UserInfo } from './user';

export interface Collection {
    userId: string;
    id: string;
    title: string;
    createdAt: string;
    themeId: number;
    cardsCount: number;
    notStartedCards: number;
    canRelearnCardCount: number;
    isPublic: boolean;
    isDeletable: boolean;
    publication?: CollectionPublication;
}

export interface StoreCollection extends Collection {
    ownerUser: UserInfo;
    isLiked: boolean;
    isDisliked: boolean;
    isAdded: boolean;
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
    tags: string[] | null;
    remembers: Remember[] | null;
}

export interface Remember {
    userId: string;
    collectionId: string;
    cardId: string;
    id: string;
    weight: number;
    phaseIndex: number;
    repeatedDate: string;
    comment: string | null | undefined;
}

export interface LearningStatistic {
    totalRepeatingCards: number;
    phaseIdToStatistic: Record<string, PhaseStatisticDto>;
    repeatedCards: number;
    learnedCards: number;
}

export interface PhaseStatisticDto {
    phaseId: string;
    totalRepeatingCards: number;
    lateCards: number;
    todayCards: number;
    futureCards: number;
}

export interface CalendarLearningStatisticModel {
    learnedCards: number;
    dateToLearnedCards: Record<string, number>;
    dateToRepeatedCards: Record<string, number>;
    dateQueueCards: Record<string, number>;
    dateToRecommendationToLearn: Record<string, number>;
}
