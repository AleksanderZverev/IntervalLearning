import { FormLabel, Typography } from '@mui/material';
import { FC, FormEventHandler, PropsWithChildren, ReactNode } from 'react';
import styles from './styles.module.css';

interface FormProps {
    onSubmit: FormEventHandler<HTMLFormElement>;
}

export const Form: FC<PropsWithChildren<FormProps>> = (props) => {
    return (
        <form className={styles.formContainer} onSubmit={props.onSubmit}>
            {props.children}
        </form>
    );
};

interface FormFieldProps {
    label: string;
    htmlFor?: string;
}

export const FormField: FC<PropsWithChildren<FormFieldProps>> = (props) => {
    return (
        <div className={styles.formField}>
            <FormLabel htmlFor={props.htmlFor}>{props.label}</FormLabel>
            {props.children}
        </div>
    );
};

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
