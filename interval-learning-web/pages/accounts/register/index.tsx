import { Button, TextField, Typography, Paper, FormLabel, Fab, CircularProgress } from '@mui/material';
import { FC, useEffect, useState } from 'react';
import { SubmitHandler, useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import styles from './register.module.css';
import { useRegisterMutation } from '../../../src/redux/accountSlice';
import { RegisterRequest } from '../../../src/types/Authentication';
import { Check, HourglassBottomRounded } from '@mui/icons-material';
import { green } from '@mui/material/colors';
import { useRouter } from 'next/router';
import { CenterContainer } from '../../../src/controls/CenterContainer/CenterContainer';

interface IForm {
    email: string;
    givenName: string;
    familyName: string | null;
    password: string;
    confirmPassword: string;
}

const schema = yup
    .object({
        givenName: yup.string().max(50).required(),
        familyName: yup.string().max(50),
        email: yup.string().email().required(),
        password: yup
            .string()
            .required('Пароль обязателен')
            .min(5, 'Минимум 5 символов')
            .max(25, 'Максимум 25 символов'),
        confirmPassword: yup
            .string()
            .oneOf([yup.ref('password')], 'Пароли должны совпадать')
            .required('Подтвердите пароль'),
    })
    .required();

const RegisterPage: FC = () => {
    const {
        register,
        handleSubmit,
        formState: { errors },
    } = useForm<IForm>({ resolver: yupResolver(schema) });

    const router = useRouter();
    const [registerAccount, { isLoading, isSuccess }] = useRegisterMutation();

    const onSubmit: SubmitHandler<IForm> = (data) => {
        const registerRequest: RegisterRequest = {
            firstName: data.givenName,
            lastName: data.familyName,
            email: data.email,
            password: data.password,
        };

        registerAccount(registerRequest).then((v) => 'data' in v && window.setTimeout(() => router.push('/'), 500));
    };

    const renderRegisterPage = () => (
        <form className={styles.formContainer} onSubmit={handleSubmit(onSubmit)}>
            <div className={styles.formHeader}>
                <Typography variant="h4" component={'h2'}>
                    Регистрация
                </Typography>
            </div>
            <div className={styles.formField}>
                <FormLabel htmlFor="given-name-input">Name</FormLabel>
                <TextField
                    size="small"
                    id="given-name-input"
                    disabled={isLoading}
                    type="text"
                    error={!!errors.givenName}
                    helperText={errors.givenName?.message || ' '}
                    {...register('givenName')}
                />
            </div>
            <div className={styles.formField}>
                <FormLabel htmlFor="family-name-input">Surname</FormLabel>
                <TextField
                    size="small"
                    id="family-name-input"
                    disabled={isLoading}
                    type="text"
                    error={!!errors.familyName}
                    helperText={errors.familyName?.message || ' '}
                    {...register('familyName')}
                />
            </div>
            <div className={styles.formField}>
                <FormLabel htmlFor="email-input">Email</FormLabel>
                <TextField
                    size="small"
                    id="email-input"
                    disabled={isLoading}
                    type="email"
                    error={!!errors.email}
                    autoComplete="email"
                    helperText={errors.email?.message || ' '}
                    {...register('email')}
                />
            </div>
            <div className={styles.formField}>
                <FormLabel htmlFor="password-input">Password</FormLabel>
                <TextField
                    size="small"
                    id="password-input"
                    disabled={isLoading}
                    error={!!errors.password}
                    autoComplete="new-password"
                    helperText={errors.password?.message || ' '}
                    type={'password'}
                    {...register('password', { required: true })}
                />
            </div>
            <div className={styles.formField}>
                <FormLabel htmlFor="confirm-password-input">Confirm password</FormLabel>
                <TextField
                    size="small"
                    id="confirm-password-input"
                    disabled={isLoading}
                    type={'password'}
                    error={!!errors.confirmPassword}
                    autoComplete="new-password"
                    helperText={errors.confirmPassword?.message || ' '}
                    {...register('confirmPassword')}
                />
            </div>
            <Button className={styles.submitButton} type="submit">
                Submit
            </Button>
        </form>
    );

    return (
        <CenterContainer>
            <Paper sx={{ width: 415, height: 445 }}>
                {isLoading || isSuccess ? (
                    <div
                        style={{
                            width: '100%',
                            height: '100%',
                            display: 'flex',
                            justifyContent: 'center',
                            alignItems: 'center',
                        }}
                    >
                        <Fab
                            sx={{
                                position: 'relative',
                                backgroundColor: isSuccess ? green[500] : 'primary',
                                color: 'white',
                            }}
                        >
                            {isSuccess ? <Check /> : <HourglassBottomRounded />}
                            {isLoading && (
                                <CircularProgress
                                    size={68}
                                    sx={{
                                        position: 'absolute',
                                        top: -6,
                                        left: -6,
                                        zIndex: 1,
                                    }}
                                />
                            )}
                        </Fab>
                    </div>
                ) : (
                    renderRegisterPage()
                )}
            </Paper>
        </CenterContainer>
    );
};

export default RegisterPage;
