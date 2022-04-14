const redirectKey = 'RedirectUrlAfterAuthorization';

export class LocalStorageHelper {
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

    static saveLearningCardsWeights = (
        collectionsId: string,
        cardIds: string[],
        weights: Record<string, number | undefined>
    ) => {
        if (LocalStorageHelper.isStorageDefined()) {
            const key = [collectionsId, ...cardIds].join('-');
            localStorage.setItem(key, JSON.stringify(weights));
        }
    };

    static getLearningCards = (collectionsId: string, cardIds: string[]): Record<string, number | undefined> | null => {
        if (!LocalStorageHelper.isStorageDefined()) {
            return null;
        }

        const key = [collectionsId, ...cardIds].join('-');
        const item = localStorage.getItem(key);
        if (!item) return null;
        const weights: Record<string, number | undefined> = JSON.parse(item);
        return weights;
    };

    private static isStorageDefined = () => Boolean(typeof window !== 'undefined' && window?.localStorage);
}
