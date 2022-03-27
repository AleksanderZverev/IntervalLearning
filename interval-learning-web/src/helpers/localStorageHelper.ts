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

    private static isStorageDefined = () => Boolean(typeof window !== 'undefined' && window?.localStorage);
}
