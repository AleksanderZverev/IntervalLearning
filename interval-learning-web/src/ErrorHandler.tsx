import { useRouter } from 'next/router';
import React, { FC } from 'react';
import { LocalStorageHelper } from './helpers/localStorageHelper';
import { useTypedDispatch } from './hooks/useTypedDispatch';
import useTypedSelector from './hooks/useTypedSelector';
import { clearErrors, selectErrors } from './redux/errorSlice';

interface ErrorHandlerProps {
    children: any;
}

export const ErrorHandler: FC<ErrorHandlerProps> = (props) => {
    const dispatch = useTypedDispatch();
    const errors = useTypedSelector(selectErrors);
    const router = useRouter();

    if (errors.length === 0) {
        return props.children;
    }

    if (errors.length === 1 && errors[0].code === 401) {
        LocalStorageHelper.saveRedirectUrlAfterAuthorization();
        router.push('/accounts/authorize');
        dispatch(clearErrors());
    }

    return <div>Unknown error</div>;
};
