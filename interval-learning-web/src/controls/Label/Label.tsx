import { FC, PropsWithChildren } from 'react';
import styles from './styles.module.css';

interface LabelProps {
    label: string;
}

export const Label: FC<PropsWithChildren<LabelProps>> = ({ label, children }) => {
    return (
        <div>
            <div className={styles.label}>{label}</div>
            <div className={styles.children}>{children}</div>
        </div>
    );
};
