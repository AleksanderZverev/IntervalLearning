import { Button, Divider } from '@mui/material';
import Link from 'next/link';
import MuiLink from '../../Link';
import React, { FC } from 'react';
import styles from './PageHeader.module.css';
import { signIn, signOut, useSession } from 'next-auth/react';

const PageHeader: FC = () => {
    const { data: session, status } = useSession();

    return (
        <header className={styles.header}>
            <div className={styles.leftHeaderContainer}>
                <Link href="/">
                    <a className={styles.logo}>Interval Learning</a>
                </Link>
                <Divider orientation="vertical" flexItem />
                <MuiLink href="/collections" underline="none" fontSize={23}>
                    Коллекции
                </MuiLink>
            </div>
            <div className={styles.rightHeaderContainer}>
                {status === 'authenticated' ? (
                    <Button variant="contained" onClick={() => signOut()}>
                        Sign Out
                    </Button>
                ) : (
                    <Button variant="contained" onClick={() => signIn()}>
                        Sign IN
                    </Button>
                )}
            </div>
        </header>
    );
};

export default PageHeader;
