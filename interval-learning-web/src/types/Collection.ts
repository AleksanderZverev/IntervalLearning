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

export interface LearningStatistic {
    repeatedCards: number;
    learnedCards: number;
}

export interface CalendarLearningStatisticModel {
    learnedCards: number;
    dateToLearnedCards: Record<string, number>;
    dateToRepeatedCards: Record<string, number>;
    dateQueueCards: Record<string, number>;
    dateToRecommendationToLearn: Record<string, number>;
}
