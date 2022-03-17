import '../styles/globals.css';
import type { AppProps } from 'next/app';
import { SessionProvider, useSession } from 'next-auth/react';
import { FC, PropsWithChildren, useLayoutEffect, useEffect } from 'react';
import { removeAuthToken, setAuthToken } from '../src/api/axiosInstance';
import { Provider } from 'react-redux';
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
import { AppDispatch, RootState, store } from '../src/redux/store';
import { ErrorHandler } from './ErrorHandler';
import { useTypedDispatch } from '../src/hooks/useTypedDispatch';
import { api } from '../src/redux/apiSlice';
import { themesApi } from '../src/redux/themeSlice';
import { useOnMount } from '../src/hooks/useOnMount';

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

    useLayoutEffect(() => {
        if (status != 'authenticated' || !session?.accessToken) {
            return;
        }

        console.log('setting aut token: ', session.accessToken);
        setAuthToken(session.accessToken as string);
    }, [status, session?.accessToken, session?.expires]);

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

const fetchStartData = async (dispatch: AppDispatch) => {
    dispatch(themesApi.endpoints.getThemes.initiate());
};

function MyApp(props: MyAppProps) {
    const { Component, emotionCache = clientSideEmotionCache, pageProps } = props;
    const { session, ...otherPageProps } = pageProps;

    useOnMount(() => {
        store.dispatch(fetchStartData);
    });

    return (
        <CacheProvider value={emotionCache}>
            <Head>
                <meta name="viewport" content="initial-scale=1, width=device-width" />
            </Head>
            <ThemeProvider theme={theme}>
                {/* CssBaseline kickstart an elegant, consistent, and simple baseline to build upon. */}
                <CssBaseline />
                <Provider store={store}>
                    <SessionProvider session={session}>
                        <div
                            style={{
                                height: '100%',
                                display: 'flex',
                                flexDirection: 'column',
                            }}
                        >
                            <PageHeader />
                            <ErrorHandler>
                                {(Component as any).auth ? (
                                    <Auth>
                                        <Component {...otherPageProps} />
                                    </Auth>
                                ) : (
                                    <Component {...otherPageProps} />
                                )}
                            </ErrorHandler>
                        </div>
                    </SessionProvider>
                </Provider>
            </ThemeProvider>
        </CacheProvider>
    );
}

export default MyApp;
