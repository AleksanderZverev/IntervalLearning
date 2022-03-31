import '../styles/globals.css';
import type { AppProps } from 'next/app';
import { SessionProvider } from 'next-auth/react';
import WebHeader from '../src/controls/WebHeader/WebHeader';

import * as React from 'react';
import Head from 'next/head';
import { ThemeProvider } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { CacheProvider, EmotionCache } from '@emotion/react';
import theme from '../src/theme';
import createEmotionCache from '../src/createEmotionCache';
import { AppDispatch, wrapper } from '../src/redux/store';
import { ErrorHandler } from './ErrorHandler';
import { themesApi } from '../src/redux/themeSlice';
import { useOnMount } from '../src/hooks/useOnMount';
import { useTypedDispatch } from '../src/hooks/useTypedDispatch';
import { selectCurrentUser } from '../src/redux/currentUserSlice';
import useTypedSelector from '../src/hooks/useTypedSelector';
import { useAutoAuthorization } from './accounts/authorize';
import { schedulesApi } from '../src/redux/schedulesSlice';

interface AuthProps {
    needAuth: boolean;
}

const clientSideEmotionCache = createEmotionCache();

interface MyAppProps extends AppProps {
    emotionCache?: EmotionCache;
}

const fetchStartData = async (dispatch: AppDispatch) => {
    dispatch(themesApi.endpoints.getThemes.initiate());
    dispatch(schedulesApi.endpoints.getSchedules.initiate());
};

function MyApp(props: MyAppProps) {
    const { Component, emotionCache = clientSideEmotionCache, pageProps } = props;
    const { session, ...otherPageProps } = pageProps;

    const currentUser = useTypedSelector(selectCurrentUser);
    const dispatch = useTypedDispatch();
    useAutoAuthorization(currentUser, dispatch);

    useOnMount(() => {
        dispatch(fetchStartData);
    });

    return (
        <CacheProvider value={emotionCache}>
            <Head>
                <meta name="viewport" content="initial-scale=1, width=device-width" />
            </Head>
            <ThemeProvider theme={theme}>
                {/* CssBaseline kickstart an elegant, consistent, and simple baseline to build upon. */}
                <CssBaseline />
                <SessionProvider session={session}>
                    <div
                        style={{
                            height: '100%',
                            display: 'flex',
                            flexDirection: 'column',
                        }}
                    >
                        <WebHeader />
                        <ErrorHandler>
                            {/* {(Component as any).auth ? (
                                        <Component {...otherPageProps} />
                                ) : ( */}
                            <Component {...otherPageProps} />
                            {/* )} */}
                        </ErrorHandler>
                    </div>
                </SessionProvider>
            </ThemeProvider>
        </CacheProvider>
    );
}

const wrappedApp = wrapper.withRedux(MyApp);

export default wrappedApp;
