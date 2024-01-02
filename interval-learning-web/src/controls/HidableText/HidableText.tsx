import { Typography } from '@mui/material';
import classNames from 'classnames';
import { FC, useEffect, useLayoutEffect, useState } from 'react';
import styles from './styles.module.css';
import _ from 'lodash';

interface HidableTextProps {
    refreshKey?: string;
    text: string;
    size: 'small' | 'medium' | 'big';
    hidden?: boolean;
    onChange?: (isHidden: boolean) => void;
}

export const HidableText: FC<HidableTextProps> = ({ refreshKey, text, size, hidden, ...props }) => {
    const shouldBeHidden = _.isBoolean(hidden) ? hidden : true;

    const [isHidden, setHidden] = useState(shouldBeHidden);

    useLayoutEffect(() => {
        if (shouldBeHidden !== isHidden) {
            setHidden(shouldBeHidden);
        }
    }, [hidden, refreshKey]);

    const onChange = (value: boolean) => {
        setHidden(value);
        props.onChange && props.onChange(value);
    };

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
            onClick={() => onChange(!isHidden)}
        >
            {size == 'big' ? largeText(text) : size == 'medium' ? mediumText(text) : small(text)}
        </div>
    );
};
