import React from 'react';
import { QueryDefinition } from '@reduxjs/toolkit/dist/query';
import { UseQuery } from '@reduxjs/toolkit/dist/query/react/buildHooks';
import { TagType } from '../redux/apiSlice';
import { CircularProgress } from '@mui/material';
import { CustomBaseQueryType } from '../redux/axiosBaseQuery';

interface ResolverProps<TQueryArg> {
    containsFetching?: boolean;
    containsError?: boolean;
    queryArg: TQueryArg;
}

export const withQueryResolver =
    <TQueryArg, TResult>(useQuery: UseQuery<QueryDefinition<TQueryArg, CustomBaseQueryType, TagType, TResult>>) =>
    (Component: React.FunctionComponent<unknown>) =>
    // eslint-disable-next-line react/display-name
    ({ queryArg, containsError, containsFetching, ...props }: ResolverProps<TQueryArg> & unknown) => {
        const { data, isError: isQueryError, isFetching: isQueryFetching, isSuccess, error } = useQuery(queryArg);

        const isFetching = isQueryFetching || Boolean(containsFetching);
        const isError = isQueryError || Boolean(containsError);

        if (isFetching) {
            return <CircularProgress />;
        }

        if (isError || !isSuccess) {
            console.log(isError, isSuccess, data);
            return <div>Error</div>;
        }

        return <Component {...props} />;
    };

export const withOtherQueryResolver =
    <TQueryArg, TResult>(useQuery: UseQuery<QueryDefinition<TQueryArg, CustomBaseQueryType, TagType, TResult>>) =>
    <TOtherQueryArg,>(Component: React.FunctionComponent<ResolverProps<TOtherQueryArg> & unknown>) =>
    // eslint-disable-next-line react/display-name
    ({ queryArg, ...props }: { queryArg: TQueryArg & TOtherQueryArg } & unknown) => {
        const { data, isError, isFetching, isSuccess, error } = useQuery(queryArg);

        return (
            <Component
                queryArg={queryArg}
                containsFetching={isFetching}
                containsError={isError || !isSuccess}
                {...props}
            />
        );
    };
