import '../styles/globals.css';
import type { AppContext, AppProps } from 'next/app';
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
import { BrowserRouter } from 'react-router-dom';
import { StaticRouter } from 'react-router-dom/server';
import App from 'next/app';
import dayjs from 'dayjs';
import duration from 'dayjs/plugin/duration';
import relativeTime from 'dayjs/plugin/relativeTime';
import 'dayjs/locale/ru';

dayjs.extend(duration);
dayjs.extend(relativeTime);

dayjs.locale('ru');

const clientSideEmotionCache = createEmotionCache();

interface MyAppProps extends AppProps {
    isServerSide: boolean;
    url: string;
    emotionCache?: EmotionCache;
}

const fetchStartData = async (dispatch: AppDispatch) => {
    dispatch(themesApi.endpoints.getThemes.initiate());
    dispatch(schedulesApi.endpoints.getSchedules.initiate());
};

function MyApp(props: MyAppProps) {
    const { Component, emotionCache = clientSideEmotionCache, url, isServerSide, pageProps } = props;

    const currentUser = useTypedSelector(selectCurrentUser);
    const dispatch = useTypedDispatch();
    useAutoAuthorization(currentUser, dispatch);

    useOnMount(() => {
        dispatch(fetchStartData);
    });

    const Router = url !== undefined ? StaticRouter : BrowserRouter;

    return (
        <Router location={url}>
            <CacheProvider value={emotionCache}>
                <Head>
                    <meta name="viewport" content="initial-scale=1, width=device-width" />
                </Head>
                <ThemeProvider theme={theme}>
                    {/* CssBaseline kickstart an elegant, consistent, and simple baseline to build upon. */}
                    <CssBaseline />
                    <ErrorHandler>
                        <WebHeader isServerSide={isServerSide} />
                        <Component {...pageProps} />
                    </ErrorHandler>
                </ThemeProvider>
            </CacheProvider>
        </Router>
    );
}

MyApp.getInitialProps = async (appContext: AppContext) => {
    // calls page's `getInitialProps` and fills `appProps.pageProps`
    const appProps = await App.getInitialProps(appContext);
    const url = appContext.ctx.req?.url;
    const isServerSide = Boolean(url);

    return { ...appProps, url, isServerSide };
};
const wrappedApp = wrapper.withRedux(MyApp);

export default wrappedApp;
