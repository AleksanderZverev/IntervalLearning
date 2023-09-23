import { Autocomplete } from '@mui/material';
import { forwardRef } from 'react';
import { Controller } from 'react-hook-form';
import useTypedSelector from '../../hooks/useTypedSelector';
import { selectThemes } from '../../redux/slices/themeSlice';
import { FormField, FormFieldProps } from '../Form/Form';
import { Theme } from '../../types/global';

interface SelectThemeProps {
    value: Theme | null;
    onChange: (newTheme: Theme | null) => void;
}

export const SelectTheme = forwardRef<HTMLDivElement, SelectThemeProps>(({ value, onChange, ...props }, ref) => {
    const themes = useTypedSelector(selectThemes);

    return (
        <Autocomplete
            style={{
                minWidth: '148px',
            }}
            value={value ?? null}
            options={themes}
            getOptionLabel={(o) => o.name}
            isOptionEqualToValue={(o, v) => o.id === v.id}
            renderInput={(params) => <FormField {...params} {...props} />}
            onChange={(event, newValue) => onChange(newValue ?? null)}
        />
    );
});

SelectTheme.displayName = 'SelectTheme';

interface SelectThemeControlProps extends FormFieldProps {
    registeredName: string;
}

export const SelectThemeControl = forwardRef<HTMLDivElement, SelectThemeControlProps>(
    ({ registeredName, ...props }, ref) => {
        const themes = useTypedSelector(selectThemes);

        return (
            <Controller
                name={registeredName}
                render={({ field: { value, ...field } }) => {
                    return (
                        <Autocomplete
                            value={value ?? null}
                            {...field}
                            options={themes}
                            getOptionLabel={(o) => o.name}
                            isOptionEqualToValue={(o, v) => o.id === v.id}
                            renderInput={(params) => <FormField {...params} {...props} />}
                            onChange={(event, newValue) => field.onChange(newValue ?? null)}
                        />
                    );
                }}
            />
        );
    }
);

SelectThemeControl.displayName = 'SelectThemeControl';
