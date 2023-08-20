import { useEffect } from 'react';

export const useDocumentTitle = (title: string | undefined | null, prefix = '') => {
    useEffect(() => {
        if (typeof document == 'undefined') return;

        if (title) {
            document.title = (prefix + ' ' + title).trim();
        }
    }, [title, prefix]);
};
