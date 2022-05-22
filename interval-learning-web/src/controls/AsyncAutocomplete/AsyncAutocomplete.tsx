import { FC, useState, useEffect, Fragment } from 'react';
import TextField from '@mui/material/TextField';
import Autocomplete from '@mui/material/Autocomplete';
import CircularProgress from '@mui/material/CircularProgress';

interface Option {
    label: string;
    item: any;
}

interface AsyncAutocompleteProps {
    onChange: (input: string) => void;
    value: any;
    valueToLabel: (value: any) => string;
    query?: () => Promise<Option[]>;
    onFocus?: () => Promise<Option[]>;

    label?: string;
    error?: boolean;
    errorMessage?: string;
    required?: boolean;
}

export const AsyncAutocomplete: FC<AsyncAutocompleteProps> = ({
    query,
    value,
    valueToLabel,
    label,
    error,
    errorMessage,
    required,
    ...props
}) => {
    console.log('AsyncAutocomplete: value = ', value);

    const [open, setOpen] = useState(false);
    const [options, setOptions] = useState<Option[]>([]);
    const [loading, setLoading] = useState(false);

    const onFocus = async () => {
        if (!props.onFocus) {
            return;
        }

        setLoading(true);
        const options = await props.onFocus();
        setOptions(options);
        setLoading(false);
    };

    const onChange = async (input: string) => {
        if (!query) {
            return;
        }

        setOptions([]);

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
            open={Boolean(open && options.length > 0)}
            onOpen={() => {
                setOpen(true);
            }}
            onClose={() => {
                setOpen(false);
            }}
            onFocus={onFocus}
            getOptionLabel={(option) => option.label ?? option}
            value={value}
            autoComplete={false}
            options={options}
            loading={loading}
            filterOptions={(x) => x}
            renderInput={(params) => (
                <TextField
                    {...params}
                    variant="standard"
                    autoComplete="off"
                    label={label}
                    error={error}
                    helperText={errorMessage ?? ' '}
                    fullWidth
                    required={required}
                    onChange={(e) => onChange(e.target.value)}
                    onBlur={(e) => props.onChange(e.target.value)}
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
