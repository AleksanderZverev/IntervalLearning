import { useEffect } from 'react';

export const useOnMount = (action: () => void) => {
    useEffect(() => {
        action();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);
};
