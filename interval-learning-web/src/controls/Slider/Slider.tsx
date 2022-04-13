import { styled, Tooltip, tooltipClasses, TooltipProps } from '@mui/material';
import { FC, useMemo } from 'react';
import { useEventListener } from '../../hooks/useEventListener';
import styles from './styles.module.css';

interface SliderProps {
    min: number;
    max: number;
    value: number;
    activeValue: number;

    onValueChange: (newValue: number) => void;
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

export const Slider: FC<SliderProps> = ({ min, max, value, activeValue, onValueChange }) => {
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
        if (e.key === 'ArrowRight') {
            const nextValue = value + 1;
            if (nextValue <= max) onValueChange(nextValue);
        } else if (e.key === 'ArrowLeft') {
            const nextValue = value - 1;
            if (nextValue >= min) onValueChange(nextValue);
        }
    });

    return (
        <span className={styles.container}>
            <span className={styles.backLine} />
            <span className={styles.progressLine} style={{ width: `${sliderWidth}%` }} />
            {values.map((v, index) => {
                const left = ((index + 1) / (total + 2)) * 100;
                const activeClass = v < activeValue ? styles.markActive : '';
                const currentElement = v == value ? styles.markCurrent : '';

                const mark = (
                    <span
                        key={v}
                        onClick={() => onValueChange(v)}
                        className={styles.mark + ' ' + activeClass + ' ' + currentElement}
                        style={{ left: `${left}%` }}
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
