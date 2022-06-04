import { TypedUseSelectorHook, useSelector } from 'react-redux';
import { RootState } from '../redux/store';

const useTypedSelector: TypedUseSelectorHook<RootState> = useSelector;

export const useRequiredTypedSelector = <TResult>(
    select: (state: RootState) => TResult | undefined | null
): TResult => {
    const result = useTypedSelector(select);
    if (!result) throw new Error('Required service is undefined');
    return result;
};

export default useTypedSelector;
