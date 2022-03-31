import { Autocomplete } from '@mui/material';
import { forwardRef } from 'react';
import { Controller } from 'react-hook-form';
import useTypedSelector from '../../hooks/useTypedSelector';
import { selectThemes } from '../../redux/slices/themeSlice';
import { FormField, FormFieldProps } from '../Form/Form';

interface SelectThemeProps extends FormFieldProps {
    registeredName: string;
}

// eslint-disable-next-line react/display-name
export const SelectTheme = forwardRef<HTMLDivElement, SelectThemeProps>(({ registeredName, ...props }, ref) => {
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
});
