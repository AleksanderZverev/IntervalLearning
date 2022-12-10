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
import classNames from 'classnames';

interface WebHeaderProps {
    isServerSide: boolean;
}

const WebHeader: FC<WebHeaderProps> = ({ isServerSide }) => {
    const currentUser = useTypedSelector((state) => state.currentUser.currentUser);

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
        navigate('/');
    };

    const onMenuClick = (action: () => void) => () => {
        action();
        setShowMenu(false);
    };

    return (
        <header className={styles.header}>
            <div className={styles.leftHeaderContainer}>
                <div>
                    <Link href="/">
                        <a className={classNames(styles.logo, styles.fullLogo)}>Interval Learning</a>
                    </Link>
                    <Link href="/">
                        <a className={classNames(styles.logo, styles.shortLogo)}>IL</a>
                    </Link>
                </div>
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
                <MuiLink
                    className={styles.link}
                    activeClassName={styles.activeLink}
                    href="/store"
                    underline="none"
                    fontSize={23}
                    onClick={() => !isServerSide && navigate('/store')}
                >
                    Поиск
                </MuiLink>
                <MuiLink className={styles.link}
                         activeClassName={styles.activeLink}
                         href="/courses"
                         underline="none"
                         fontSize={23}
                         onClick={() => !isServerSide && navigate('/courses')}
                >
                    Курсы
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
                <MenuItem
                    onClick={onMenuClick(() => {
                        router.push('/schedules');
                        !isServerSide && navigate('/schedules');
                    })}
                >
                    <ListItemIcon>
                        <EventNote />
                    </ListItemIcon>
                    <ListItemText>Мои учебные планы</ListItemText>
                </MenuItem>
                <Divider />
                <MenuItem onClick={onMenuClick(signOut)}>
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
