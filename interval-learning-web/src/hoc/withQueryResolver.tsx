/* eslint-disable react/display-name */
import React from 'react';
import { QueryDefinition } from '@reduxjs/toolkit/dist/query';
import { UseQuery } from '@reduxjs/toolkit/dist/query/react/buildHooks';
import { TagType } from '../redux/apiSlice';
import { CircularProgress } from '@mui/material';
import { CustomBaseQueryType } from '../redux/axiosBaseQuery';

interface ResolverProps<TQueryArg> {
    containsFetching?: boolean;
    containsError?: boolean;
    disableLoading?: boolean;
    queryArg: TQueryArg;
}

export interface WithQueryResolverData<TData> {
    resolverData: TData;
}

export const withQueryResolver =
    <TQueryArg, TResult>(useQuery: UseQuery<QueryDefinition<TQueryArg, CustomBaseQueryType, TagType, TResult>>) =>
    <TComponentProps,>(
        Component: React.FunctionComponent<WithQueryResolverData<TResult> & TQueryArg & TComponentProps>
    ) =>
    (props: ResolverProps<TQueryArg> & Omit<TComponentProps, keyof (WithQueryResolverData<TResult> & TQueryArg)>) => {
        const { queryArg, containsError, disableLoading, containsFetching, ...otherProps } = props;
        const {
            data,
            isError: isQueryError,
            isFetching: isQueryFetching,
            isSuccess,
            error,
        } = useQuery(queryArg, { skip: disableLoading });

        const isFetching = isQueryFetching || Boolean(containsFetching);
        const isError = isQueryError || Boolean(containsError);

        if (isFetching) {
            console.debug('loading');
            return <CircularProgress />;
        }

        if (isError || !data) {
            console.log(isError, isSuccess, data);
            return <div>Error</div>;
        }

        //TODO: don't know how to fix
        const HackComponent = Component as any;

        return <HackComponent resolverData={data} {...queryArg} {...otherProps} />;
    };

export const withOtherQueryResolver =
    <TQueryArg, TResult>(useQuery: UseQuery<QueryDefinition<TQueryArg, CustomBaseQueryType, TagType, TResult>>) =>
    <TComponentProps, TOtherQueryArg>(
        Component: React.FunctionComponent<ResolverProps<TOtherQueryArg> & TComponentProps>
    ) =>
    // eslint-disable-next-line react/display-name
    (props: { queryArg: TQueryArg & TOtherQueryArg } & TComponentProps) => {
        const { queryArg, ...otherProps } = props;
        const { data, isError, isFetching, isSuccess, error } = useQuery(queryArg);

        return (
            <Component
                // queryArg={queryArg}
                containsFetching={isFetching}
                containsError={isError || !isSuccess}
                {...props}
            />
        );
    };
