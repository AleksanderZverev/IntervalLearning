import { FC, PropsWithChildren } from 'react';
import { Container } from '@mui/material';
import styles from './styles.module.css';

interface PageContainerProps {
    transparent?: boolean;
}

export const PageContainer: FC<PropsWithChildren<PageContainerProps>> = ({ transparent, children }) => {
    return (
        <Container
            className={styles.container}
            maxWidth="lg"
            sx={{
                marginTop: '10px',
                backgroundColor: transparent ? undefined : 'white',
            }}
            style={{ display: 'grid', gridTemplateRows: 'auto 1fr' }}
        >
            {children}
        </Container>
    );
};
