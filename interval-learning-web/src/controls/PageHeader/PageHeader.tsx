import { Typography } from '@mui/material';
import { FC, ReactNode } from 'react';
import styles from './styles.module.css';

interface PageHeaderProps {
    title: string;
    subTitle?: string;
    subMenu?: ReactNode;
}

export const PageHeader: FC<PageHeaderProps> = ({ title, subTitle, subMenu }) => {
    return (
        <div className={styles.container}>
            <div className={styles.innerContainer}>
                <Typography variant="h1" fontSize={36}>
                    {title}
                </Typography>
                {subMenu}
            </div>
            {subTitle && <div className={styles.subTitle}>{subTitle}</div>}
            <div className={styles.endLine} />
        </div>
    );
};
