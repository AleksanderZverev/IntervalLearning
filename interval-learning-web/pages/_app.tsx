import '../styles/globals.css';
import type { AppProps } from 'next/app';
import { SessionProvider, useSession } from 'next-auth/react';
import { FC, PropsWithChildren, useEffect } from 'react';
import axiosInstance, { removeAuthToken, setAuthToken } from '../src/api/axiosInstance';
import { CustomSession } from '../src/types/Session';
import Authorize from './authorize';
import PageHeader from '../src/controls/PageHeader/PageHeader';

import * as React from 'react';
import Head from 'next/head';
import { ThemeProvider } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { CacheProvider, EmotionCache } from '@emotion/react';
import theme from '../src/theme';
import createEmotionCache from '../src/createEmotionCache';
import { Box, Container } from '@mui/material';

interface AuthProps {
    needAuth: boolean;
}

const Auth: FC<PropsWithChildren<any>> = ({ children }) => {
    const { data: session, status } = useSession({
        required: true,
        onUnauthenticated: () => {
            console.log('remove auth token');
            removeAuthToken();
        },
    });

    useEffect(() => {
        if (status == 'authenticated' && session?.accessToken) {
            console.log('setting aut token: ', session.accessToken);
            setAuthToken(session.accessToken as string);
        }
    }, [status, session?.accessToken]);

    if (status == 'authenticated') {
        return children;
    }

    return <Authorize />;
};

// Client-side cache, shared for the whole session of the user in the browser.
const clientSideEmotionCache = createEmotionCache();

interface MyAppProps extends AppProps {
    emotionCache?: EmotionCache;
}

function MyApp(props: MyAppProps) {
    const { Component, emotionCache = clientSideEmotionCache, pageProps } = props;
    const { session, ...otherPageProps } = pageProps;

    return (
        <CacheProvider value={emotionCache}>
            <Head>
                <meta name="viewport" content="initial-scale=1, width=device-width" />
            </Head>
            <ThemeProvider theme={theme}>
                {/* CssBaseline kickstart an elegant, consistent, and simple baseline to build upon. */}
                <CssBaseline />
                <SessionProvider session={session}>
                    <PageHeader />
                    <Container maxWidth="lg" style={{ marginTop: '10px', height: '100%' }}>
                        <Box sx={{ backgroundColor: 'white', height: 'inherit' }}>
                            {(Component as any).auth ? (
                                <Auth>
                                    <Component {...otherPageProps} />
                                </Auth>
                            ) : (
                                <Component {...otherPageProps} />
                            )}
                        </Box>
                    </Container>
                </SessionProvider>
            </ThemeProvider>
        </CacheProvider>
    );
}

export default MyApp;
