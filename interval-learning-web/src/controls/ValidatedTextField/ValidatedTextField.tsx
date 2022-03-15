import { TextField } from '@mui/material';
import { FC } from 'react';
import { Validated } from '../../types/global';

interface ValidatedTextFieldProps {
    value: Validated<string | null>;
    onValueChange: (newValue: Validated<string | null>) => void;
    label?: string;
    notNullOrEmpty?: boolean;
    maxLength?: number;
}

export const ValidatedTextField: FC<ValidatedTextFieldProps> = ({ value, notNullOrEmpty, maxLength, ...props }) => {
    const onValueChange = (newValue: string | null) => {
        let error = false;
        let errorMessage: string | null = null;

        if (notNullOrEmpty && !newValue) {
            error = true;
            errorMessage = 'Не может быть пусты';
        }

        if (maxLength && newValue && newValue.length > maxLength) {
            error = true;
            errorMessage = 'Максимальная длина символов: ' + maxLength;
        }

        const newValueItem: Validated<string | null> = { value: newValue, error, errorMessage };
        props.onValueChange(newValueItem);
    };

    return (
        <TextField
            value={value.value}
            onChange={(e) => onValueChange(e.target.value)}
            label={props.label}
            error={value.error}
            helperText={value.errorMessage}
        />
    );
};
