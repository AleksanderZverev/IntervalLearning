import { FC, useEffect } from 'react';
import { Button, TextField, Typography, Paper, FormLabel } from '@mui/material';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import styles from './authorize.module.css';
import { useAuthenticateMutation, useRefreshTokenQuery } from '../../../src/redux/accountSlice';
import { ModalPageContainer } from '../../../src/controls/ModalPageContainer/ModalPageContainer';
import { useTypedDispatch } from '../../../src/hooks/useTypedDispatch';
import { checkIsLoggedOut, selectCurrentUser, setCurrentUser } from '../../../src/redux/currentUserSlice';
import { useRouter } from 'next/router';
import { User } from '../../../src/types/user';
import useTypedSelector from '../../../src/hooks/useTypedSelector';
import { AppDispatch } from '../../../src/redux/store';

export const useAutoAuthorization = (currentUser: User | null, dispatch: AppDispatch) => {
    const isLoggedOut = checkIsLoggedOut();

    const autoAuthorizeData = useRefreshTokenQuery(undefined, {
        skip: isLoggedOut || currentUser !== null,
    });

    useEffect(() => {
        if (autoAuthorizeData.isSuccess) {
            dispatch(setCurrentUser(autoAuthorizeData.data));
        }
    }, [autoAuthorizeData.isSuccess]);

    return autoAuthorizeData;
};

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
    const currentUser = useTypedSelector(selectCurrentUser);

    const {
        data: refreshedUser,
        isLoading: autoQueryLoading,
        isSuccess: isRefreshSuccess,
    } = useAutoAuthorization(currentUser, dispatch);

    const [authenticate, { isLoading: mutationLoading, isSuccess }] = useAuthenticateMutation();

    const isLoading = autoQueryLoading || mutationLoading;

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
