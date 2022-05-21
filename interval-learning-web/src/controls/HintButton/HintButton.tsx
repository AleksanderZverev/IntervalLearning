import { Button, ButtonProps } from '@mui/material';
import { FC, PropsWithChildren } from 'react';
import { StringHelper } from '../../helpers/StringHelper';

interface HintButtonProps extends ButtonProps {
    hint: string;
    hintPosition: 'top left' | 'top right' | 'bottom left' | 'bottom right';
    hintSpace?: boolean;
}

export const HintButton: FC<PropsWithChildren<HintButtonProps>> = ({
    hint,
    hintPosition,
    hintSpace,
    children,
    ...otherProps
}) => {
    const [topBottom, leftRight] = hintPosition.split(' ');

    return (
        <div
            style={{
                position: 'relative',
                [`margin${StringHelper.CapitalizeFirstLetter(topBottom)}`]: hintSpace ? '10px' : undefined,
            }}
        >
            <div
                style={{ position: 'absolute', [topBottom]: '-20px', [leftRight]: '0', fontSize: 14, color: '#b7b7b7' }}
            >
                {hint}
            </div>
            <Button {...otherProps}>{children}</Button>
        </div>
    );
};
