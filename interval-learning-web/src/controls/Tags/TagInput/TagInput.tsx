import { FC, FormEventHandler, PropsWithChildren, forwardRef } from 'react';
import styles from './styles.module.css';
import { TextField } from '@mui/material';
import { FormFieldProps } from '../../Form/Form';

export interface TagInputProps extends FormFieldProps {}

// eslint-disable-next-line react/display-name
export const TagFormField = forwardRef<HTMLInputElement, TagInputProps>(
    ({ error, errorMessage, ...otherProps }, ref) => {
        return (
            <div className={styles.tagFormField}>
                <TextField
                    ref={ref}
                    sx={{
                        '& label': {
                            color: '#B7B7B7',
                            fontSize: 16,
                        },
                        '& input': {
                            fontSize: 20,
                        },
                    }}
                    label={null}
                    error={error}
                    helperText={errorMessage}
                    fullWidth
                    variant="standard"
                    {...otherProps}
                />
            </div>
        );
    }
);
