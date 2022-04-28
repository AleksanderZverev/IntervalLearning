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
                height: '100%',
            }}
            style={{ padding: 0 }}
        >
            <div
                style={{
                    backgroundColor: transparent ? undefined : 'white',
                    padding: '20px 50px',
                    minHeight: '100%',
                    height: 'auto',
                    display: 'flex',
                    flexDirection: 'column',
                }}
            >
                {children}
            </div>
        </Container>
    );
};
