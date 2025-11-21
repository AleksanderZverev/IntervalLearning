import '../styles/globals.css';
import type { AppContext, AppProps } from 'next/app';
import WebHeader from '../src/controls/WebHeader/WebHeader';
import React from 'react';
import Head from 'next/head';
import { ThemeProvider } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { CacheProvider, EmotionCache } from '@emotion/react';
import theme from '../src/theme';
import createEmotionCache from '../src/createEmotionCache';
import { AppDispatch, wrapper } from '../src/redux/store';
import { ErrorHandler } from '../src/ErrorHandler';
import { themesApi } from '../src/redux/themeSlice';
import { useOnMount } from '../src/hooks/useOnMount';
import { useTypedDispatch } from '../src/hooks/useTypedDispatch';
import { schedulesApi } from '../src/redux/schedulesSlice';
import { BrowserRouter } from 'react-router-dom';
import { StaticRouter } from 'react-router-dom/server';
import App from 'next/app';
import dayjs from 'dayjs';
import duration from 'dayjs/plugin/duration';
import relativeTime from 'dayjs/plugin/relativeTime';
import localizedFormat from 'dayjs/plugin/localizedFormat';
import 'dayjs/locale/ru';
import { NextComponentType } from 'next';
import GlobalErrorBoundary from '../src/GlobalErrorBoundary';
import { dictionaryApi } from '../src/redux/api/dictionaryApi';
import { LocalizationProvider } from '@mui/x-date-pickers';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import { NextJsHelper } from '../src/helpers/Infra/NextJsHelper';

dayjs.extend(duration);
dayjs.extend(relativeTime);
dayjs.extend(localizedFormat);

dayjs.locale('ru');

const clientSideEmotionCache = createEmotionCache();

export type NextComponentProps = NextComponentType & {
    isServerSide: boolean;
};

interface MyAppProps extends AppProps {
    url: string;
    emotionCache?: EmotionCache;
    Component: NextComponentProps;
}

const fetchStartData = async (dispatch: AppDispatch) => {
    dispatch(dictionaryApi.endpoints.getLanguages.initiate());
    dispatch(themesApi.endpoints.getThemes.initiate());
    dispatch(schedulesApi.endpoints.getSchedules.initiate());
};

function MyApp(props: MyAppProps) {
    const { Component, emotionCache = clientSideEmotionCache, url, pageProps } = props;

    //const currentUser = useTypedSelector(selectCurrentUser);
    const dispatch = useTypedDispatch();
    // useAutoAuthorization(currentUser, dispatch);

    useOnMount(() => {
        dispatch(fetchStartData);
    });

    const isServerSide = NextJsHelper.isServerSide();
    const Router = isServerSide ? StaticRouter : BrowserRouter;

    return (
        <Router location={url}>
            <CacheProvider value={emotionCache}>
                <Head>
                    <meta name="viewport" content="initial-scale=1, width=device-width" />
                    <link rel="icon" href="/favicon.svg" />
                </Head>
                <LocalizationProvider dateAdapter={AdapterDayjs} adapterLocale="ru">
                    <ThemeProvider theme={theme}>
                        {/* CssBaseline kickstart an elegant, consistent, and simple baseline to build upon. */}
                        <CssBaseline />
                        <GlobalErrorBoundary>
                            <ErrorHandler>
                                <WebHeader isServerSide={isServerSide} />
                                <Component isServerSide={isServerSide} {...pageProps} />
                            </ErrorHandler>
                        </GlobalErrorBoundary>
                    </ThemeProvider>
                </LocalizationProvider>
            </CacheProvider>
        </Router>
    );
}

MyApp.getInitialProps = async (appContext: AppContext) => {
    // calls page's `getInitialProps` and fills `appProps.pageProps`
    const appProps = await App.getInitialProps(appContext);
    const url = appContext.ctx.req?.url ?? '';

    return { ...appProps, url };
};
const wrappedApp = wrapper.withRedux(MyApp);

export default wrappedApp;
