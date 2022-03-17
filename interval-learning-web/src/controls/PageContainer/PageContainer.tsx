import { FC, ReactNode } from 'react';
import { Box, Container } from '@mui/material';

const PageContainer: FC<{ children: ReactNode }> = ({ children }) => {
    return (
        <Container maxWidth="lg" style={{ marginTop: '10px', height: '100%' }}>
            <Box sx={{ backgroundColor: 'white', height: 'inherit' }}>{children}</Box>
        </Container>
    );
};
