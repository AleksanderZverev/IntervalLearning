import { useState } from 'react';

export function useLocalStorageValue<TValue>(
    key: string,
    defaultValue?: TValue | (() => TValue)
): [TValue | undefined, (newValue: TValue | undefined) => void] {
    const [value, setValueInternal] = useState(() => {
        const item = localStorage.getItem(key);
        if (item) {
            return JSON.parse(item) as TValue;
        }

        if (!defaultValue) {
            return;
        }

        return typeof defaultValue === 'function' ? (defaultValue as Function)() : defaultValue;
    });

    const setValue = (newValue: TValue | undefined) => {
        setValueInternal(newValue);

        if (!newValue) {
            localStorage.removeItem(key);
        } else {
            localStorage.setItem(key, JSON.stringify(newValue));
        }
    };

    return [value, setValue];
}
