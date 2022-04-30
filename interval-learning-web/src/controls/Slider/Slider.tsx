import { styled, Tooltip, tooltipClasses, TooltipProps } from '@mui/material';
import classNames from 'classnames';
import { FC, useMemo } from 'react';
import { useEventListener } from '../../hooks/useEventListener';
import { LightTooltip } from '../LightTooltip/LightTooltip';
import styles from './styles.module.css';

interface SliderProps {
    min: number;
    max: number;
    value: number;
    activeValue: number;

    finishMode?: boolean;
    onValueChange: (newValue: number) => void;
    vertical?: boolean;
}

const horizontalKeys: Record<string, number> = {
    ArrowRight: 1,
    ArrowLeft: -1,
};

const verticalKeys: Record<string, number> = {
    ArrowUp: -1,
    ArrowDown: 1,
};

export const Slider: FC<SliderProps> = ({ min, max, value, activeValue, onValueChange, vertical, finishMode }) => {
    const widthProperty = vertical ? 'height' : 'width';
    const heightProperty = vertical ? 'width' : 'height';
    const topProperty = vertical ? 'left' : 'top';
    const leftProperty = vertical ? 'top' : 'left';

    const counterTopProperty = vertical ? 'bottom' : 'top';
    const counterTopValuePrefix = vertical ? '' : '-';
    const counterRightValuePrefix = vertical ? '-' : '';
    const counterHeight = vertical ? '5px' : '25px';
    const counterWidth = vertical ? '25px' : '5px';

    const total = max - min;
    const activePoint = activeValue - min;
    const sliderWidth = activeValue > max ? 100 : (activePoint / (total + 2)) * 100;

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
                className={classNames(styles.backLine, { [styles.markNotFinished]: finishMode })}
                style={{ [widthProperty]: '100%', [topProperty]: '50%', [heightProperty]: '4px' }}
            />
            <span
                className={styles.progressLine}
                style={{
                    [widthProperty]: `${sliderWidth}%`,
                    [topProperty]: '50%',
                    [heightProperty]: '4px',
                    transition: `${widthProperty} ${sliderWidth === 100 ? '2s' : '1s'} ease`,
                }}
            />

            <span
                className={styles.counter}
                style={{
                    [counterTopProperty]: `${counterTopValuePrefix}${counterHeight}`,
                    right: `${counterRightValuePrefix}${counterWidth}`,
                }}
            >{`${activeValue}/${max}`}</span>

            {values.map((v, index) => {
                const left = ((index + 1) / (total + 2)) * 100;
                const isActive = v < activeValue;
                const isCurrentElement = v == value && !finishMode;

                const mark = (
                    <span
                        key={v}
                        onClick={() => onValueChange(v)}
                        className={classNames(styles.mark, {
                            [styles.markActive]: isActive,
                            [styles.markCurrent]: isCurrentElement,
                            [styles.markNotFinished]: finishMode && !isActive,
                        })}
                        style={{
                            [leftProperty]: `${left}%`,
                            [topProperty]: isCurrentElement ? 8 : 11,
                            transition: finishMode ? undefined : `background-color .5s ease 1s`,
                        }}
                    />
                );
                return isCurrentElement ? (
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
