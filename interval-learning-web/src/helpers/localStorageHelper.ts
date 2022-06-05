const redirectKey = 'RedirectUrlAfterAuthorization';
const forbidShowingKey = 'ForbidShowing';
const repeatingCardsKey = 'Repeating cards';

interface RepeatingCards {
    scheduleUserId: string;
    scheduleId: string;
    collectionId: string;
    phaseIndex: number;
    date: string;
    cardIdToWeight: Record<string, number | undefined>;
}

export class LocalStorageHelper {
    static save<TItem extends object>(key: string, object: TItem) {
        if (LocalStorageHelper.isStorageDefined()) {
            localStorage.setItem(key, JSON.stringify(object));
        }
    }

    static get<TItem extends object>(key: string, defaultValue: TItem) {
        let item: TItem | null = null;

        if (LocalStorageHelper.isStorageDefined()) {
            const json = localStorage.getItem(key);
            if (json) {
                item = JSON.parse(json);
            }
        }

        return item ?? defaultValue;
    }

    static saveRedirectUrlAfterAuthorization = () => {
        if (LocalStorageHelper.isStorageDefined()) {
            console.log(window.location);
            localStorage.setItem(redirectKey, window.location.pathname);
        }
    };

    static clearRedirectUrl = () => {
        if (LocalStorageHelper.isStorageDefined()) {
            localStorage.removeItem(redirectKey);
        }
    };

    static getRedirectUrlAfterAuthorization = () => {
        if (LocalStorageHelper.isStorageDefined()) {
            const redirectUrl = localStorage.getItem(redirectKey);
            if (redirectUrl) {
                return redirectUrl;
            }
        }

        return '/';
    };

    static saveRepeatingCardsWeights = (
        scheduleUserId: string,
        scheduleId: string,
        phaseIndex: number,
        collectionId: string,
        date: string,
        weights: Record<string, number | undefined>
    ) => {
        if (!LocalStorageHelper.isStorageDefined()) {
            return;
        }

        const item: RepeatingCards = {
            scheduleUserId,
            scheduleId,
            collectionId,
            date,
            cardIdToWeight: weights,
            phaseIndex,
        };

        localStorage.setItem(repeatingCardsKey, JSON.stringify(item));
    };

    static getRepeatingCards = (
        scheduleUserId: string,
        scheduleId: string,
        phaseIndex: number,
        date: string,
        collectionId: string
    ): Record<string, number | undefined> | null => {
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
        return item.cardIdToWeight;
    };

    static setForbidShowing = (key: string) => {
        if (!LocalStorageHelper.isStorageDefined()) {
            return;
        }

        const itemJson = localStorage.getItem(forbidShowingKey);
        const forbidItems: string[] = itemJson ? JSON.parse(itemJson) : [];
        forbidItems.push(key);

        localStorage.setItem(forbidShowingKey, JSON.stringify(forbidItems));
    };

    static hasForbidShowing = (key: string): boolean => {
        if (!LocalStorageHelper.isStorageDefined()) {
            return false;
        }

        const itemJson = localStorage.getItem(forbidShowingKey);
        const forbidItems: string[] = itemJson ? JSON.parse(itemJson) : [];
        const itemIndex = forbidItems.findIndex((item) => item === key);
        return itemIndex >= 0;
    };

    private static isStorageDefined = () => Boolean(typeof window !== 'undefined' && window?.localStorage);
}
