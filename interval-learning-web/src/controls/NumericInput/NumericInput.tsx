import { TextField, TextFieldProps } from '@mui/material';
import { forwardRef } from 'react';

export const NumericInput = forwardRef<HTMLDivElement, TextFieldProps>(function ForwardedRefNumericInput(
    textFieldProps,
    ref
) {
    const { inputProps, sx, ...otherProps } = textFieldProps;
    return (
        <TextField
            {...otherProps}
            inputProps={{ ...inputProps, inputMode: 'numeric', pattern: '[0-9]*' }}
            ref={ref}
            type="number"
            onWheel={() => false}
            sx={{
                ...sx,
                '& label': {
                    color: '#B7B7B7',
                    fontSize: 16,
                },
                '& input': {
                    fontSize: 20,
                },
            }}
        />
    );
});
