import { FC, PropsWithChildren } from 'react';
import { Box, Container } from '@mui/material';

interface PageContainerProps {
    transparent?: boolean;
}

export const PageContainer: FC<PropsWithChildren<PageContainerProps>> = ({ transparent, children }) => {
    return (
        <div style={{ marginTop: '10px', width: '100%', height: '100%' }}>
            <Container maxWidth="lg" sx={{ height: '100%' }}>
                <Box
                    sx={{
                        backgroundColor: transparent ? undefined : 'white',
                        height: 'inherit',
                        display: 'flex',
                        flexDirection: 'column',
                    }}
                >
                    {children}
                </Box>
            </Container>
        </div>
    );
};
