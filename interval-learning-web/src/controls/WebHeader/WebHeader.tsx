import { Button, Divider, IconButton, ListItemIcon, ListItemText, Menu, MenuItem, Stack } from '@mui/material';
import Link from 'next/link';
import MuiLink from '../../Link';
import React, { FC, useMemo, useState } from 'react';
import { useRouter } from 'next/router';
import useTypedSelector from '../../hooks/useTypedSelector';
import { signOutUser } from '../../redux/currentUserSlice';
import { useTypedDispatch } from '../../hooks/useTypedDispatch';
import styles from './WebHeader.module.css';
import { useNavigate } from 'react-router-dom';
import { EventNote, Logout } from '@mui/icons-material';
import { StringAvatar } from '../StringAvatar/StringAvatar';
import { UserHelper } from '../../helpers/UserHelper';

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
    const [showMenu, setShowMenu] = useState(false);
    const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null);

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
                    className={styles.link}
                    activeClassName={styles.activeLink}
                    href="/collections"
                    underline="none"
                    fontSize={23}
                    onClick={() => !isServerSide && navigate('/collections')}
                >
                    Коллекции
                </MuiLink>
                <MuiLink
                    className={styles.link}
                    activeClassName={styles.activeLink}
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
                    <IconButton
                        onClick={(e) => {
                            setShowMenu(true);
                            setAnchorEl(e.currentTarget);
                        }}
                    >
                        <StringAvatar size={30} fontSize={18} name={UserHelper.getFullName(currentUser)} />
                    </IconButton>
                ) : (
                    <>
                        <Button variant="contained" onClick={() => router.push('/accounts/authorize')}>
                            Вход
                        </Button>
                    </>
                )}
            </div>
            <Menu anchorEl={anchorEl} open={showMenu} onClose={() => setShowMenu(false)}>
                <MenuItem>
                    <ListItemIcon>
                        <EventNote />
                    </ListItemIcon>
                    <ListItemText>Мои учебные планы</ListItemText>
                </MenuItem>
                <Divider />
                <MenuItem onClick={signOut}>
                    <ListItemIcon>
                        <Logout />
                    </ListItemIcon>
                    <ListItemText>Выйти</ListItemText>
                </MenuItem>
            </Menu>
        </header>
    );
};

export default WebHeader;
