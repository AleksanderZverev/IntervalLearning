import { InfoOutlined, RefreshOutlined } from '@mui/icons-material';
import { Button, colors, FormControlLabel, IconButton, Paper, Radio, RadioGroup, Typography } from '@mui/material';
import classNames from 'classnames';
import { FC, useState } from 'react';
import { useEventListener } from '../../../../hooks/useEventListener';
import { Card } from '../../../../types/Collection';
import styles from './styles.module.css';

interface LearnCardProps {
    card: Card;
    showNext: boolean;
    showPrevious: boolean;
    errorMessage?: string;
    value: number | null;
    onNext: () => void;
    onPrevious: () => void;
    onChange: (weight: number | null) => void;
    onFinish: () => void;
    isActive: boolean;
}

export const LearnCard: FC<LearnCardProps> = ({
    card,
    showNext,
    showPrevious,
    value,
    onNext,
    onPrevious,
    onFinish: onEndButtonClick,
    errorMessage,
    isActive,
    onChange,
}) => {
    const [backIsHidden, setBackIsHidden] = useState(true);
    const [isError, setIsError] = useState(false);

    useEventListener('keydown', (e) => {
        e.key === '1' && onChange(0);
        e.key === '2' && onChange(0.5);
        e.key === '3' && onChange(1);
    });

    return (
        <Paper className={styles.container}>
            <InfoOutlined className={styles.infoIcon} />
            {isActive && (
                <IconButton
                    className={styles.refreshIcon}
                    onClick={() => {
                        setIsError(false);
                        onChange(null);
                    }}
                >
                    <RefreshOutlined />
                </IconButton>
            )}
            <Typography variant="h3" fontSize={32}>
                {card.frontSideText}
            </Typography>
            <div
                className={classNames(styles.backContainer, { [styles.backHidden]: backIsHidden })}
                onClick={() => setBackIsHidden(!backIsHidden)}
            >
                {card.backSideText}
            </div>
            <div>
                <RadioGroup
                    onKeyDownCapture={(e) => e.preventDefault()}
                    row
                    onChange={(e, v) => {
                        if (isError) {
                            setIsError(false);
                        }
                        onChange(parseFloat(v));
                    }}
                >
                    <FormControlLabel checked={value === 0} value={0} control={<Radio />} label="Не помню" />
                    <FormControlLabel checked={value === 0.5} value={0.5} control={<Radio />} label="Помню частично" />
                    <FormControlLabel checked={value === 1} value={1} control={<Radio />} label="Помню" />
                </RadioGroup>

                <div
                    className={styles.errorMessage}
                    style={{ border: `1px solid ${colors.red[400]}`, visibility: isError ? 'visible' : 'hidden' }}
                >
                    {errorMessage}
                </div>
            </div>
            <div className={styles.buttonsContainer}>
                {showPrevious ? (
                    <Button
                        variant="outlined"
                        onClick={() => {
                            onPrevious();
                            setIsError(false);
                        }}
                    >
                        Назад
                    </Button>
                ) : (
                    <div />
                )}
                {showNext ? (
                    <Button
                        variant="outlined"
                        onClick={() => {
                            if (value === null) {
                                setIsError(true);
                            } else {
                                onNext();
                            }
                        }}
                    >
                        Далее
                    </Button>
                ) : (
                    <Button
                        variant="contained"
                        onClick={() => {
                            if (value === null) {
                                setIsError(true);
                            } else {
                                onEndButtonClick();
                            }
                        }}
                    >
                        Завершить
                    </Button>
                )}
            </div>
        </Paper>
    );
};
