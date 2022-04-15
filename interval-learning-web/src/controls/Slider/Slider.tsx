import { styled, Tooltip, tooltipClasses, TooltipProps } from '@mui/material';
import { padding } from '@mui/system';
import { FC, useMemo } from 'react';
import { useEventListener } from '../../hooks/useEventListener';
import styles from './styles.module.css';

interface SliderProps {
    min: number;
    max: number;
    value: number;
    activeValue: number;

    onValueChange: (newValue: number) => void;
    vertical?: boolean;
}

const LightTooltip = styled(({ className, ...props }: TooltipProps) => (
    <Tooltip {...props} classes={{ popper: className }} />
))(({ theme }) => ({
    [`& .${tooltipClasses.tooltip}`]: {
        backgroundColor: theme.palette.common.white,
        color: 'rgba(0, 0, 0, 0.87)',
        boxShadow: theme.shadows[1],
        fontSize: 11,
    },
}));

const horizontalKeys: Record<string, number> = {
    ArrowRight: 1,
    ArrowLeft: -1,
};

const verticalKeys: Record<string, number> = {
    ArrowUp: -1,
    ArrowDown: 1,
};

export const Slider: FC<SliderProps> = ({ min, max, value, activeValue, onValueChange, vertical }) => {
    const widthProperty = vertical ? 'height' : 'width';
    const heightProperty = vertical ? 'width' : 'height';
    const topProperty = vertical ? 'left' : 'top';
    const leftProperty = vertical ? 'top' : 'left';
    const maxWidthProperty = vertical ? 'maxHeight' : 'maxWidth';

    const total = max - min;
    const sliderWidth = ((activeValue + 1 - min) / (total + 2)) * 100;
    const values = useMemo(() => {
        const values: number[] = [];

        for (let i = min; i <= max; i++) {
            values.push(i);
        }

        return values;
    }, [min, max]);

    useEventListener('keydown', (e) => {
        const keys = vertical ? verticalKeys : horizontalKeys;

        if (e.key in keys) {
            const offset = keys[e.key];
            const nextValue = value + offset;
            if ((offset > 0 && nextValue <= max) || (offset < 0 && nextValue >= min)) {
                onValueChange(nextValue);
            }
        }
    });

    return (
        <span
            className={styles.container}
            style={{
                [heightProperty]: 4,
                padding: vertical ? '0 14px' : '14px 0',
                [widthProperty]: '100%',
                [heightProperty]: '4px',
                [widthProperty]: 650,
            }}
        >
            <span
                className={styles.backLine}
                style={{ [widthProperty]: '100%', [topProperty]: '50%', [heightProperty]: '4px' }}
            />
            <span
                className={styles.progressLine}
                style={{
                    [widthProperty]: `${sliderWidth}%`,
                    [topProperty]: '50%',
                    transition: `${widthProperty} 1s ease;`,
                }}
            />
            {values.map((v, index) => {
                const left = ((index + 1) / (total + 2)) * 100;
                const activeClass = v < activeValue ? styles.markActive : '';
                const currentElement = v == value ? styles.markCurrent : '';

                const mark = (
                    <span
                        key={v}
                        onClick={() => onValueChange(v)}
                        className={styles.mark + ' ' + activeClass + ' ' + currentElement}
                        style={{ [leftProperty]: `${left}%`, [topProperty]: currentElement ? 8 : 11 }}
                    />
                );
                return currentElement ? (
                    mark
                ) : (
                    <LightTooltip key={v} title={v.toString()}>
                        {mark}
                    </LightTooltip>
                );
            })}
        </span>
    );
};
