import { useEffect } from 'react';

export const useDocumentTitle = (title: string | undefined | null, prefix = '') => {
    useEffect(() => {
        if (title && document) {
            document.title = (prefix + ' ' + title).trim();
        }
    }, [title, prefix]);
};
