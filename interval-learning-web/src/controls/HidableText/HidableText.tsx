import { Typography } from '@mui/material';
import classNames from 'classnames';
import { FC, useEffect, useState } from 'react';
import styles from './styles.module.css';

interface HidableTextProps {
    text: string;
    size: 'small' | 'medium' | 'big';
    forceVisible?: boolean;
    isVisibleByDefault?: boolean;
    onChange?: (isHidden: boolean) => void;
}

export const HidableText: FC<HidableTextProps> = ({ text, size, forceVisible, isVisibleByDefault, ...props }) => {
    const [isHidden, setHidden] = useState(!Boolean(isVisibleByDefault));

    const onChange = (value: boolean) => {
        setHidden(value);
        props.onChange && props.onChange(value);
    };

    useEffect(() => {
        if (forceVisible) {
            onChange(false);
        }
    }, [forceVisible]);

    const largeText = (wrappedText: string) => (
        <Typography variant="h3" fontSize={32}>
            {wrappedText}
        </Typography>
    );

    const mediumText = (wrappedText: string) => (
        <Typography variant="h5" fontSize={24}>
            {wrappedText}
        </Typography>
    );

    const small = (wrappedText: string) => <>{wrappedText}</>;

    return (
        <div
            className={classNames(styles.backContainer, { [styles.backHidden]: isHidden })}
            onClick={() => setHidden(!isHidden)}
        >
            {size == 'big' ? largeText(text) : size == 'medium' ? mediumText(text) : small(text)}
        </div>
    );
};
