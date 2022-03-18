import { FC, useEffect } from 'react';
import { Button, TextField, Typography, Paper, FormLabel, Fab, CircularProgress } from '@mui/material';
import { SubmitHandler, useForm, useFormState } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import styles from './authorize.module.css';
import { useAuthenticateMutation, useRefreshTokenQuery } from '../../../src/redux/accountSlice';
import { ModalPageContainer } from '../../../src/controls/ModalPageContainer/ModalPageContainer';
import { useTypedDispatch } from '../../../src/hooks/useTypedDispatch';
import { setCurrentUser } from '../../../src/redux/currentUserSlice';
import { useRouter } from 'next/router';

interface Form {
    email: string;
    password: string;
}

const schema = yup
    .object({
        email: yup.string().email().required(),
        password: yup.string().min(5).max(25).required(),
    })
    .required();

const AuthorizePage: FC = () => {
    const {
        register,
        handleSubmit,
        formState: { errors },
    } = useForm<Form>({ resolver: yupResolver(schema) });
    const dispatch = useTypedDispatch();
    const router = useRouter();
    const { data: refreshedUser, isSuccess: isRefreshSuccess } = useRefreshTokenQuery();
    const [authenticate, { isLoading, isSuccess }] = useAuthenticateMutation();

    useEffect(() => {
        if (isRefreshSuccess) {
            dispatch(setCurrentUser(refreshedUser));
            router.push('/');
        }
    }, [isRefreshSuccess]);

    const onSubmit = async (data: Form) => {
        try {
            const user = await authenticate(data).unwrap();
            dispatch(setCurrentUser(user));
            router.push('/');
        } catch {}
    };

    return (
        <ModalPageContainer>
            <Paper sx={{ width: 415, height: 260 }}>
                <form className={styles.formContainer} onSubmit={handleSubmit(onSubmit)}>
                    <div className={styles.formHeader}>
                        <Typography variant="h4" component={'h2'}>
                            Вход
                        </Typography>
                    </div>
                    <div className={styles.formField}>
                        <FormLabel htmlFor="email-input">Email</FormLabel>
                        <TextField
                            size="small"
                            id="email-input"
                            disabled={isLoading}
                            type="text"
                            error={!!errors.email}
                            helperText={errors.email?.message || ' '}
                            {...register('email')}
                        />
                    </div>
                    <div className={styles.formField}>
                        <FormLabel htmlFor="password-name-input">Password</FormLabel>
                        <TextField
                            size="small"
                            id="password-name-input"
                            autoComplete="current-password"
                            disabled={isLoading}
                            type="password"
                            error={!!errors.password}
                            helperText={errors.password?.message || ' '}
                            {...register('password')}
                        />
                    </div>
                    <Button className={styles.submitButton} type="submit">
                        Submit
                    </Button>
                </form>
            </Paper>
        </ModalPageContainer>
    );
};

export default AuthorizePage;
