import { FC, PropsWithChildren } from 'react';
import _ from 'lodash';
import styles from './styles.module.css';

interface TagProps {
    // name: string;
}

export const Tag: FC<PropsWithChildren<TagProps>> = ({ children }) => {
    //TODO:
    return <span className={styles.tag}>{children}</span>;
};
