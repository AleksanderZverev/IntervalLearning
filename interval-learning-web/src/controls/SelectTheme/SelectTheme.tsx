import { Autocomplete, TextField } from '@mui/material';
import { FC, useMemo } from 'react';
import { useSelector } from 'react-redux';
import { selectThemes } from '../../redux/themeSlice';
import { Theme, Validated } from '../../types/global';

interface SelectThemeProps {
    value: Validated<Theme | null>;
    onValueChange: (newValue: Validated<Theme | null>) => void;
    notNullOrEmpty?: boolean;
}

export const SelectTheme: FC<SelectThemeProps> = ({ value, notNullOrEmpty, ...props }) => {
    const themeIndex = useSelector(selectThemes);
    const themeItems = useMemo(() => Object.values(themeIndex), [themeIndex]);

    const onValueChange = (newValue: Theme | null) => {
        const hasError = Boolean(notNullOrEmpty && !newValue);
        props.onValueChange({ value: newValue, error: hasError, errorMessage: hasError ? 'Выберите значение' : '' });
    };

    return (
        <Autocomplete
            value={value.value}
            options={themeItems}
            getOptionLabel={(o) => o.name}
            renderInput={(params) => (
                <TextField {...params} error={value.error} helperText={value.errorMessage} label="theme" />
            )}
            onChange={(event, newValue) => onValueChange(newValue)}
        />
    );
};
