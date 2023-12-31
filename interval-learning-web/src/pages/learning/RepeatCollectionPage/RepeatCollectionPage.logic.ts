import { LocalStorageHelper } from '../../../helpers/localStorageHelper';

const repeatingCardsKey = 'Repeating cards';

interface RepeatingCards {
    scheduleUserId: string;
    scheduleId: string;
    collectionId: string;
    phaseIndex: number;
    date: string;
    cardIdToForm: Record<string, RememberForm | undefined>;
    currentCardIndex: number;
    repeatedCardIndex: number;
}

export interface RememberForm {
    weight: number | undefined;
    comment: string | undefined | null;
}

export interface State {
    rememberWeights: Record<string, RememberForm | undefined>;
    currentCardIndex: number;
    repeatedCardIndex: number;
}

export const getDefaultState = (): State => ({
    rememberWeights: {},
    currentCardIndex: 0,
    repeatedCardIndex: 0,
});

export const saveRepeatingCardsState = (
    scheduleUserId: string,
    scheduleId: string,
    phaseIndex: number,
    collectionId: string,
    date: string,
    state: State
) => {
    if (!LocalStorageHelper.isStorageDefined()) {
        return;
    }

    const item: RepeatingCards = {
        scheduleUserId,
        scheduleId,
        collectionId,
        date,
        cardIdToForm: state.rememberWeights,
        phaseIndex,
        currentCardIndex: state.currentCardIndex,
        repeatedCardIndex: state.repeatedCardIndex,
    };

    localStorage.setItem(repeatingCardsKey, JSON.stringify(item));
};

export const isRepeatingInProgress = (
    scheduleUserId: string,
    scheduleId: string,
    phaseIndex: number,
    date: string,
    collectionId: string
): boolean => {
    const weights = getRepeatingCards(scheduleUserId, scheduleId, phaseIndex, date, collectionId);
    return Boolean(weights && Object.values(weights).length > 0);
};

export const getRepeatingCards = (
    scheduleUserId: string,
    scheduleId: string,
    phaseIndex: number,
    date: string,
    collectionId: string
): State | null => {
    if (!LocalStorageHelper.isStorageDefined()) {
        return null;
    }

    const itemString = localStorage.getItem(repeatingCardsKey);
    if (!itemString) return null;

    const item: RepeatingCards = JSON.parse(itemString);

    if (
        !item ||
        item.scheduleUserId !== scheduleUserId ||
        item.scheduleId !== scheduleId ||
        item.collectionId !== collectionId ||
        item.phaseIndex !== phaseIndex ||
        item.date !== date
    ) {
        return null;
    }
    return {
        rememberWeights: item.cardIdToForm,
        currentCardIndex: item.currentCardIndex || 0,
        repeatedCardIndex: item.repeatedCardIndex || 0,
    };
};
