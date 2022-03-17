export interface Theme {
    id: string;
    name: string;
}

export type Validated<T> = {
    value: T;
    error: boolean;
    errorMessage: string | null;
};

export function validatedDefault<T>(value: T): Validated<T> {
    return {
        value,
        error: false,
        errorMessage: null,
    };
}
