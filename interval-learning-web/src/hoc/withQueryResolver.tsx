/* eslint-disable react/display-name */
import React, { useRef, useState } from 'react';
import { MutationDefinition, QueryDefinition } from '@reduxjs/toolkit/dist/query';
import { UseMutation, UseQuery } from '@reduxjs/toolkit/dist/query/react/buildHooks';
import { TagType } from '../redux/apiSlice';
import { Button, Paper, Portal } from '@mui/material';
import { CustomBaseQueryType } from '../redux/axiosBaseQuery';
import { CenterContainer } from '../controls/CenterContainer/CenterContainer';
import { ModalLoader } from '../ModalLoader/ModalLoader';
import { AssertionModal } from '../controls/Modals/AssertionModal';
import { Loader } from '../controls/Loader/Loader';
import { ErrorPage } from '../controls/ErrorPage/ErrorPage';

type GetInnerType<S> = S extends UseMutation<infer T> ? T : never;

type GetMutationArgs<S> = S extends UseMutation<infer TDefinition>
    ? TDefinition extends MutationDefinition<infer TArg, CustomBaseQueryType, TagType, any>
        ? TArg
        : never
    : never;

type GetMutationResult<S> = S extends UseMutation<infer TDefinition>
    ? TDefinition extends MutationDefinition<any, CustomBaseQueryType, TagType, infer TResult>
        ? TResult
        : never
    : never;

type GetQueryArgs<S> = S extends UseQuery<infer TDefinition>
    ? TDefinition extends QueryDefinition<infer TArg, CustomBaseQueryType, TagType, any>
        ? TArg
        : never
    : never;

type GetQueryResult<S> = S extends UseQuery<infer TDefinition>
    ? TDefinition extends QueryDefinition<any, CustomBaseQueryType, TagType, infer TResult>
        ? TResult
        : never
    : never;

interface PrivateResolverProps {
    containsFetching?: boolean;
    containsError?: boolean;
    extendedData?: object;
    onRefetch?: () => void;
}

interface ResolverProps<TQueryArg> {
    disableLoading?: boolean;
    queryArg: TQueryArg;
}

export interface WithQueryResolverData<T extends UseQuery<QueryDefinition<any, CustomBaseQueryType, TagType, any>>> {
    queryData: GetQueryResult<T>;
}

export interface WithOtherQueryResolverData<
    T extends UseQuery<QueryDefinition<any, CustomBaseQueryType, TagType, any>>
> {
    extendedData: GetQueryResult<T>;
}

export const withQueryResolver =
    <TQueryArg, TResult>(useQuery: UseQuery<QueryDefinition<TQueryArg, CustomBaseQueryType, TagType, TResult>>) =>
    <TComponentProps,>(
        Component: React.FunctionComponent<WithQueryResolverData<typeof useQuery> & TQueryArg & TComponentProps>
    ) =>
    (
        props: PrivateResolverProps &
            ResolverProps<TQueryArg> &
            Omit<
                TComponentProps,
                | keyof (WithQueryResolverData<typeof useQuery> & TQueryArg)
                | keyof (WithOtherQueryResolverData<typeof useQuery> & TQueryArg)
            >
    ) => {
        const { queryArg, containsError, disableLoading, containsFetching, extendedData, onRefetch, ...otherProps } =
            props;
        const {
            data,
            isError: isQueryError,
            isFetching: isQueryFetching,
            isSuccess,
            error,
            refetch,
        } = useQuery(queryArg, { skip: disableLoading });

        const isFetching = isQueryFetching || Boolean(containsFetching);
        const isError = isQueryError || Boolean(containsError);

        const tryRefetch = () => {
            if (!isSuccess) {
                console.debug('refetch 1');
                refetch();
            }
            onRefetch && onRefetch();
        };

        if (isFetching) {
            console.debug('loading');
            return (
                <CenterContainer>
                    <Loader textColor="black" />
                </CenterContainer>
            );
        }

        if (isError || !data) {
            console.log(isError, isSuccess, data);
            return <ErrorPage errorMessage="Не удалось загрузить данные" onReload={tryRefetch} />;
        }

        //TODO: don't know how to fix
        const HackComponent = Component as any;

        return <HackComponent queryData={data} extendedData={extendedData} {...queryArg} {...otherProps} />;
    };

export const withOtherQueryResolver =
    <TQueryArg, TResult>(useQuery: UseQuery<QueryDefinition<TQueryArg, CustomBaseQueryType, TagType, TResult>>) =>
    <TComponentProps, TOtherQueryArg>(
        Component: React.FunctionComponent<PrivateResolverProps & ResolverProps<TOtherQueryArg> & TComponentProps>
    ) =>
    (props: { queryArg: TQueryArg & TOtherQueryArg } & TComponentProps & PrivateResolverProps) => {
        const {
            queryArg,
            containsError: otherError,
            containsFetching: otherFetching,
            extendedData,
            onRefetch: onOtherRefetch,
            ...otherProps
        } = props;
        const { data, isError, isFetching, isSuccess, error, refetch } = useQuery(queryArg);

        const onRefetch = () => {
            if (isError) {
                refetch();
                console.debug('refetch 2');
            }

            if (otherError) {
                onOtherRefetch && onOtherRefetch();
                console.debug('refetch other');
            }
        };

        const resultExtendedData = {
            ...extendedData,
            ...data,
        };

        const HackComponent = Component as any;

        return (
            <HackComponent
                queryArg={queryArg}
                containsFetching={isFetching || otherFetching}
                containsError={isError || otherError}
                extendedData={resultExtendedData}
                onRefetch={onRefetch}
                {...otherProps}
            />
        );
    };

export interface WithMutationResolverProps<
    T extends UseMutation<MutationDefinition<any, CustomBaseQueryType, TagType, any>>
> {
    mutationProps: {
        mutate: (args: GetMutationArgs<T>) => Promise<GetMutationResult<T>>;
        showRetryModal: (retry: () => void) => void;
        isLoading: boolean;
        isSuccess: boolean;
        data: GetMutationResult<T> | undefined;
        reset: () => void;
    };
}

interface MutationResolverProps {}

export const withMutationResolver =
    <TMutationArg, TResult>(
        useMutation: UseMutation<MutationDefinition<TMutationArg, CustomBaseQueryType, TagType, TResult>>,
        errorMessage: string
    ) =>
    <TComponentProps,>(
        Component: React.FunctionComponent<WithMutationResolverProps<typeof useMutation> & TComponentProps>
    ) =>
    (props: MutationResolverProps & Omit<TComponentProps, keyof WithMutationResolverProps<typeof useMutation>>) => {
        const [mutate, mutationArgs] = useMutation();
        const { data, isLoading, isError, isSuccess, originalArgs, reset } = mutationArgs;

        const onMutate = (args: TMutationArg) => mutate(args).unwrap();

        const { ...otherProps } = props;
        const [showAssertionModal, setShowAssertionModal] = useState(false);
        const { current } = useRef<{ retryFunc: (() => void) | null }>({ retryFunc: null });

        const onRetry = () => {
            current.retryFunc && current.retryFunc();
            setShowAssertionModal(false);
        };

        const showRetryModal = (retry: () => void) => {
            setShowAssertionModal(true);
            current.retryFunc = retry;
        };

        //TODO: don't know how to fix
        const HackComponent = Component as any;

        return (
            <>
                <Portal>
                    {showAssertionModal && (
                        <AssertionModal
                            onClose={() => setShowAssertionModal(false)}
                            onCancel={() => setShowAssertionModal(false)}
                            onAssert={onRetry}
                            title={'Ошибка при отправке данных'}
                            message={errorMessage}
                            assertTitle={'Повторить запрос'}
                            cancelTitle={'Отмена'}
                        />
                    )}
                </Portal>
                <ModalLoader loading={isLoading} />
                <HackComponent
                    mutationProps={{
                        mutate: onMutate,
                        showRetryModal: showRetryModal,
                        isLoading: isLoading,
                        isSuccess: isSuccess,
                        data: data,
                        reset: reset,
                    }}
                    {...otherProps}
                />
            </>
        );
    };
