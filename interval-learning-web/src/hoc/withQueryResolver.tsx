import React from 'react';
import { QueryDefinition } from '@reduxjs/toolkit/dist/query';
import { UseQuery } from '@reduxjs/toolkit/dist/query/react/buildHooks';
import { TagType } from '../redux/apiSlice';
import { CircularProgress } from '@mui/material';
import { CustomBaseQueryType } from '../redux/axiosBaseQuery';

export const withQueryResolver =
    <TQueryArg, TResult>(useQuery: UseQuery<QueryDefinition<TQueryArg, CustomBaseQueryType, TagType, TResult>>) =>
    (Component: React.FunctionComponent<unknown>) =>
    // eslint-disable-next-line react/display-name
    ({ queryArg, ...props }: { queryArg: TQueryArg } & unknown) => {
        const { isError, isFetching, isSuccess, error } = useQuery(queryArg);

        if (isFetching) {
            return <CircularProgress />;
        }

        if (isError || !isSuccess) {
            return <div>Error</div>;
        }

        return <Component {...props} />;
    };
