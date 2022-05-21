import { Paper } from '@mui/material';
import { FC, PropsWithChildren, ReactNode } from 'react';
import styles from './styles.module.css';

interface PaperCardProps {
    topRightControl?: ReactNode;
    topLeftControl?: ReactNode;
    leftButton?: ReactNode;
    rightButton?: ReactNode;
    justifyButtons?: 'center' | 'flex-start' | 'flex-end' | 'space-around' | 'space-between';
}

export const PaperCard: FC<PropsWithChildren<PaperCardProps>> = ({
    leftButton,
    rightButton,
    topRightControl,
    topLeftControl,
    children,
    justifyButtons,
}) => {
    return (
        <Paper className={styles.container}>
            {topRightControl && <div className={styles.topRightControl}>{topRightControl}</div>}
            {topLeftControl && <div className={styles.topLeftControl}>{topLeftControl}</div>}
            {children}
            <div className={styles.buttonsContainer} style={{ justifyContent: justifyButtons }}>
                {leftButton ? leftButton : <div />}
                {rightButton ? rightButton : <div />}
            </div>
        </Paper>
    );
};
