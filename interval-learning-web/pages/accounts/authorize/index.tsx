import { FC } from 'react';
import { Button, TextField, Typography, Paper, FormLabel, Stack } from '@mui/material';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import styles from './authorize.module.css';
import { useAuthenticateMutation, useRefreshTokenQuery } from '../../../src/redux/accountSlice';
import { CenterContainer } from '../../../src/controls/CenterContainer/CenterContainer';
import { useTypedDispatch } from '../../../src/hooks/useTypedDispatch';
import { checkIsLoggedOut, selectCurrentUser } from '../../../src/redux/currentUserSlice';
import { useRouter } from 'next/router';
import { User } from '../../../src/types/user';
import useTypedSelector from '../../../src/hooks/useTypedSelector';
import { AppDispatch } from '../../../src/redux/store';
import { LocalStorageHelper } from '../../../src/helpers/localStorageHelper';
import { withMutationResolver, WithMutationResolverProps } from '../../../src/hoc/withQueryResolver';
import { Loader } from '../../../src/controls/Loader/Loader';

export const useAutoAuthorization = (currentUser: User | null, dispatch: AppDispatch) => {
    const isLoggedOut = checkIsLoggedOut();

    const autoAuthorizeData = useRefreshTokenQuery(undefined, {
        skip: isLoggedOut || currentUser !== null,
    });

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

interface AuthorizePageContentProps extends WithMutationResolverProps<typeof useAuthenticateMutation> {}

const AuthorizePageContent: FC<AuthorizePageContentProps> = ({
    mutationProps: { mutate: authenticate, isLoading: mutationLoading },
}) => {
    const {
        register,
        handleSubmit,
        setError,
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

    const isLoading = Boolean(autoQueryLoading || mutationLoading);

    const onSubmit = async (data: Form) => {
        try {
            await authenticate(data);
            const redirectUrl = LocalStorageHelper.getRedirectUrlAfterAuthorization();
            LocalStorageHelper.clearRedirectUrl();
            window.location.href = redirectUrl.endsWith('accounts/register') ? '/' : redirectUrl;
        } catch {
            setError('email', { message: 'Неверная почта или пароль' });
            setError('password', { message: 'Неверная почта или пароль' });
        }
    };

    if (isLoading) {
        return (
            <CenterContainer>
                <Paper sx={{ width: 415, height: 260 }}>
                    <Loader title="Авторизация" />
                </Paper>
            </CenterContainer>
        );
    }

    return (
        <CenterContainer>
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
                            autoComplete="email"
                            placeholder="some@gmail.com"
                            type="email"
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
                            autoComplete="off"
                            type="password"
                            error={!!errors.password}
                            helperText={errors.password?.message || ' '}
                            {...register('password')}
                        />
                    </div>
                    <Stack direction={'row'} justifyContent={'space-between'}>
                        <Button variant="outlined" onClick={() => router.push('/accounts/register')}>
                            Регистрация
                        </Button>
                        <Button variant="contained" className={styles.submitButton} type="submit">
                            Войти
                        </Button>
                    </Stack>
                </form>
            </Paper>
        </CenterContainer>
    );
};

const AuthorizePage = withMutationResolver(useAuthenticateMutation, 'Не удалось авторизоваться')(AuthorizePageContent);

export default AuthorizePage;
