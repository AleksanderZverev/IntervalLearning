import { useState } from 'react';
import { NextJsHelper } from '../helpers/Infra/NextJsHelper';

export function useLocalStorageValue<TValue>(
    key: string,
    defaultValue?: TValue | (() => TValue)
): [TValue | undefined, (newValue: TValue | undefined) => void] {
    const getDefaultValue = () => {
        if (!defaultValue) return;
        return typeof defaultValue === 'function' ? (defaultValue as Function)() : defaultValue;
    };

    const [value, setValueInternal] = useState(() => {
        const defaultV = getDefaultValue();
        if (NextJsHelper.isServerSide()) return defaultV;

        const item = localStorage.getItem(key);
        if (item) {
            const parsedValue = JSON.parse(item) as TValue;
            return { ...defaultV, ...parsedValue };
        }

        return defaultV;
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
