import { FC, useState, useEffect, Fragment } from 'react';
import { TextField, Autocomplete, CircularProgress, TextFieldProps } from '@mui/material';

interface Option {
    label: string;
    id: string;
}

interface AsyncAutocompleteProps {
    onChange: (newValue: string) => void;
    value: string;
    query?: () => Promise<Option[]>;
    onFocus?: () => Promise<Option[]>;

    label?: string;
    error?: boolean;
    errorMessage?: string;
    required?: boolean;
    textFieldProps?: TextFieldProps;
    multiline?: boolean;
    rows?: number;
}

export const AsyncAutocomplete: FC<AsyncAutocompleteProps> = ({
    query,
    value,
    label,
    error,
    errorMessage,
    required,
    textFieldProps,
    multiline,
    rows,
    ...otherProps
}) => {
    const [open, setOpen] = useState(false);
    const [options, setOptions] = useState<Option[]>([]);
    const [loading, setLoading] = useState(false);

    const onFocus = async () => {
        if (!otherProps.onFocus) {
            return;
        }

        setLoading(true);
        const options = await otherProps.onFocus();
        setOptions(options);
        setLoading(false);
    };

    const onChange = async (input: string) => {
        otherProps.onChange(input);

        if (!query) {
            return;
        }

        if (!input) {
            return;
        }

        setLoading(true);
        const options = await query();
        setOptions(options);
        setLoading(false);
    };

    return (
        <Autocomplete
            freeSolo
            open={Boolean(open && options.length > 0)}
            onOpen={() => {
                setOpen(true);
            }}
            onClose={() => {
                setOpen(false);
            }}
            onFocus={onFocus}
            value={value}
            autoComplete={false}
            options={options}
            loading={loading}
            onInputChange={(e, v, r) => {
                onChange(v);
            }}
            renderInput={(params) => (
                <TextField
                    {...params}
                    {...textFieldProps}
                    variant="standard"
                    autoComplete="off"
                    multiline={multiline}
                    rows={rows}
                    label={label}
                    error={error}
                    helperText={errorMessage ?? ' '}
                    fullWidth
                    required={required}
                    InputProps={{
                        ...params.InputProps,
                        endAdornment: (
                            <Fragment>
                                {loading ? <CircularProgress color="inherit" size={20} /> : null}
                                {params.InputProps.endAdornment}
                            </Fragment>
                        ),
                    }}
                />
            )}
        />
    );
};
