import { FC, PropsWithChildren } from 'react';
import _ from 'lodash';
import styles from './styles.module.css';

interface TagProps {}

export const Tag: FC<PropsWithChildren<TagProps>> = ({ children }) => {
    return <span className={styles.tag}>{children}</span>;
};
