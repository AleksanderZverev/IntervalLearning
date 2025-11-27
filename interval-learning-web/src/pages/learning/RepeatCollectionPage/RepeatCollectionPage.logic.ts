import dayjs from 'dayjs';
import { LocalStorageHelper } from '../../../helpers/localStorageHelper';

const repeatingCardsKey = 'Repeating cards';

interface RepeatingCards {
    scheduleUserId: string;
    scheduleId: string;
    collectionId: string;
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
    repeatedCardIndex: -1,
});

export const saveRepeatingCardsState = (
    scheduleUserId: string,
    scheduleId: string,
    collectionId: string,
    dateString: string,
    state: State
) => {
    if (!LocalStorageHelper.isStorageDefined()) {
        return;
    }

    const date = dayjs(dateString);
    const item: RepeatingCards = {
        scheduleUserId,
        scheduleId,
        collectionId,
        date: date.format('YYYY-MM-DD'),
        cardIdToForm: state.rememberWeights,
        currentCardIndex: state.currentCardIndex,
        repeatedCardIndex: state.repeatedCardIndex,
    };

    localStorage.setItem(repeatingCardsKey, JSON.stringify(item));
};

export const getRepeatingCards = (
    scheduleUserId: string,
    scheduleId: string,
    dateString: string,
    collectionId: string
): State | null => {
    if (!LocalStorageHelper.isStorageDefined()) {
        return null;
    }

    const itemString = localStorage.getItem(repeatingCardsKey);
    if (!itemString) return null;

    const item: RepeatingCards = JSON.parse(itemString);
    const date = dayjs(dateString);

    if (
        !item ||
        item.scheduleUserId !== scheduleUserId ||
        item.scheduleId !== scheduleId ||
        item.collectionId !== collectionId ||
        item.date !== date.format('YYYY-MM-DD')
    ) {
        return null;
    }

    return {
        rememberWeights: item.cardIdToForm,
        currentCardIndex: item.currentCardIndex || 0,
        repeatedCardIndex: typeof item.repeatedCardIndex === 'number' ? item.repeatedCardIndex : -1,
    };
};
