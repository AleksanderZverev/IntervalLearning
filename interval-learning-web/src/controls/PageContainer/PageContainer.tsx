import { FC, PropsWithChildren } from 'react';
import { Container } from '@mui/material';

interface PageContainerProps {
    transparent?: boolean;
}

export const PageContainer: FC<PropsWithChildren<PageContainerProps>> = ({ transparent, children }) => {
    return (
        <Container
            maxWidth="lg"
            sx={{
                marginTop: '10px',
                backgroundColor: transparent ? undefined : 'white',
            }}
            style={{ padding: '20px 50px' }}
        >
            {children}
        </Container>
    );
};
