import { Button, Divider, IconButton } from '@mui/material';
import Link from 'next/link';
import MuiLink from '../../Link';
import React, { FC, useMemo } from 'react';
import { useRouter } from 'next/router';
import useTypedSelector from '../../hooks/useTypedSelector';
import { signOutUser } from '../../redux/currentUserSlice';
import { useTypedDispatch } from '../../hooks/useTypedDispatch';
import styles from './WebHeader.module.css';
import { useNavigate } from 'react-router-dom';
import { Logout } from '@mui/icons-material';

interface WebHeaderProps {
    isServerSide: boolean;
}

const WebHeader: FC<WebHeaderProps> = ({ isServerSide }) => {
    const currentUser = useTypedSelector((state) => state.currentUser.currentUser);

    const userNameTitle = useMemo(
        () => (currentUser !== null ? (currentUser.firstName + ' ' + currentUser.lastName).trim() : ''),
        [currentUser]
    );
    const router = useRouter();
    const navigate = useNavigate();
    const dispatch = useTypedDispatch();

    const signOut = () => {
        if (currentUser === null) {
            return;
        }

        dispatch(signOutUser());
        router.push('/');
    };

    return (
        <header className={styles.header}>
            <div className={styles.leftHeaderContainer}>
                <Link href="/">
                    <a className={styles.logo}>Interval Learning</a>
                </Link>
                <Divider orientation="vertical" flexItem />
                <MuiLink
                    href="/collections"
                    underline="none"
                    fontSize={23}
                    onClick={() => !isServerSide && navigate('/collections')}
                >
                    Коллекции
                </MuiLink>
                <MuiLink
                    href="/learning"
                    underline="none"
                    fontSize={23}
                    onClick={() => !isServerSide && navigate('/learning')}
                >
                    Изучение
                </MuiLink>
            </div>
            <div className={styles.rightHeaderContainer}>
                {currentUser !== null ? (
                    <>
                        <span>{userNameTitle}</span>
                        <IconButton onClick={signOut}>
                            <Logout />
                        </IconButton>
                    </>
                ) : (
                    <>
                        <Button variant="contained" onClick={() => router.push('/accounts/register')}>
                            Sign Up
                        </Button>
                        <Button variant="contained" onClick={() => router.push('/accounts/authorize')}>
                            Sign In
                        </Button>
                    </>
                )}
            </div>
        </header>
    );
};

export default WebHeader;
