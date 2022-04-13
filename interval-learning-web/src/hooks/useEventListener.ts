import { useEffect, useRef } from 'react';

export function useEventListener<T extends keyof WindowEventMap>(
    eventName: T,
    handler: (event: WindowEventMap[T]) => void,
    element = window
) {
    const savedHandler = useRef<(event: WindowEventMap[T]) => void>();

    useEffect(() => {
        savedHandler.current = handler;
    }, [handler]);

    useEffect(() => {
        const isSupported = element && element.addEventListener;
        if (!isSupported) {
            console.error("element doesn't support event listeners");
            return;
        }

        const eventListener = (event: WindowEventMap[T]) => savedHandler.current && savedHandler.current(event);

        element.addEventListener(eventName, eventListener);

        return () => {
            element.removeEventListener(eventName, eventListener);
        };
    }, [eventName, element]);
}
