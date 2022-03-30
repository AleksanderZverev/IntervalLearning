import { Autocomplete } from '@mui/material';
import { forwardRef, useMemo } from 'react';
import { Controller } from 'react-hook-form';
import { useSelector } from 'react-redux';
import { selectThemes } from '../../redux/themeSlice';
import { FormField, FormFieldProps } from '../Form/Form';

interface SelectThemeProps extends FormFieldProps {
    registeredName: string;
    notNullOrEmpty?: boolean;
}

// eslint-disable-next-line react/display-name
export const SelectTheme = forwardRef<HTMLDivElement, SelectThemeProps>(
    ({ registeredName, notNullOrEmpty, ...props }, ref) => {
        const themeIndex = useSelector(selectThemes);
        const themeItems = useMemo(() => Object.values(themeIndex), [themeIndex]);

        return (
            <Controller
                name={registeredName}
                render={({ field: { value, ...field } }) => {
                    return (
                        <Autocomplete
                            value={value ?? null}
                            {...field}
                            options={themeItems}
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
