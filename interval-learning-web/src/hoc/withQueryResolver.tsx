/* eslint-disable react/display-name */
import React, { useRef, useState } from 'react';
import { MutationDefinition, QueryDefinition } from '@reduxjs/toolkit/dist/query';
import { UseMutation, UseQuery } from '@reduxjs/toolkit/dist/query/react/buildHooks';
import { TagType } from '../redux/apiSlice';
import { CircularProgress, Portal } from '@mui/material';
import { CustomBaseQueryType } from '../redux/axiosBaseQuery';
import { CenterContainer } from '../controls/CenterContainer/CenterContainer';
import { ModalLoader } from '../ModalLoader/ModalLoader';
import { AssertionModal } from '../controls/Modals/AssertionModal';

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
            return (
                <CenterContainer>
                    <CircularProgress />
                </CenterContainer>
            );
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

type GetInnerType<S> = S extends UseMutation<infer T> ? T : never;

type GetArgs<S> = S extends UseMutation<infer TDefinition>
    ? TDefinition extends MutationDefinition<infer TArg, CustomBaseQueryType, TagType, any>
        ? TArg
        : never
    : never;

type GetResult<S> = S extends UseMutation<infer TDefinition>
    ? TDefinition extends MutationDefinition<any, CustomBaseQueryType, TagType, infer TResult>
        ? TResult
        : never
    : never;

export interface WithMutationResolverProps<
    T extends UseMutation<MutationDefinition<any, CustomBaseQueryType, TagType, any>>
> {
    mutate: (args: GetArgs<T>) => Promise<GetResult<T>>;
    showRetryModal: (retry: () => void) => void;
    isLoading: boolean;
    isSuccess: boolean;
    mutationData: GetResult<T> | undefined;
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
        const { data, isLoading, isError, isSuccess, originalArgs } = mutationArgs;

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

        return (
            <>
                <Portal>
                    {showAssertionModal && (
                        <AssertionModal
                            open
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
                <Component
                    mutate={onMutate}
                    showRetryModal={showRetryModal}
                    isLoading={isLoading}
                    isSuccess={isSuccess}
                    mutationData={data}
                    {...otherProps}
                />
            </>
        );
    };
