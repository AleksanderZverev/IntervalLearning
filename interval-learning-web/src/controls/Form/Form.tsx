import { ArrowForward, ArrowForwardIos, ArrowRight, SvgIconComponent } from '@mui/icons-material';
import { FormLabel, TextField, TextFieldProps, Typography } from '@mui/material';
import { FC, FormEventHandler, PropsWithChildren, forwardRef } from 'react';
import styles from './styles.module.css';

interface FormProps {
    onSubmit?: FormEventHandler<HTMLFormElement>;
}

export const Form: FC<PropsWithChildren<FormProps>> = (props) => {
    return (
        <form className={styles.formContainer} onSubmit={props.onSubmit}>
            {props.children}
        </form>
    );
};

type FormFieldOtherProps = Omit<TextFieldProps, 'helperText' | 'error' | 'fullWidth' | 'label' | 'variant'>;

export interface FormFieldProps extends FormFieldOtherProps {
    label: string;
    error?: boolean;
    errorMessage?: string;
}

interface FormFieldLabelProps {
    label: string;
    htmlFor?: string;
}

export const FormFiledLabel: FC<PropsWithChildren<FormFieldLabelProps>> = (props) => {
    return (
        <div className={styles.formField}>
            <FormLabel htmlFor={props.htmlFor}>{props.label}</FormLabel>
            {props.children}
        </div>
    );
};

// eslint-disable-next-line react/display-name
export const FormField = forwardRef<HTMLDivElement, FormFieldProps>(
    ({ label, error, errorMessage, ...otherProps }, ref) => {
        return (
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
                label={label}
                error={error}
                helperText={errorMessage ?? ' '}
                fullWidth
                variant="standard"
                {...otherProps}
            />
        );
    }
);

export interface IconFormFieldProps extends FormFieldProps {
    icon: SvgIconComponent;
}

// eslint-disable-next-line react/display-name
export const IconFormField = forwardRef<HTMLDivElement, IconFormFieldProps>(
    ({ label, error, errorMessage, icon, ...otherProps }, ref) => {
        const Icon = icon;

        return (
            <div className={styles.iconFormField}>
                <div className={styles.icon}>{<Icon fontSize="small" color={'primary'} />}</div>
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
                    label={label}
                    error={error}
                    helperText={errorMessage ?? ' '}
                    fullWidth
                    variant="standard"
                    {...otherProps}
                />
            </div>
        );
    }
);

interface FormHeaderProps {
    title: string;
}

export const FormHeader: FC<FormHeaderProps> = (props) => {
    return (
        <div className={styles.formHeader}>
            <Typography variant="h4" component={'h2'}>
                {props.title}
            </Typography>
        </div>
    );
};
