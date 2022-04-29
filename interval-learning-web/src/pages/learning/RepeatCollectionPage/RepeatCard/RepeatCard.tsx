import { InfoOutlined, RefreshOutlined } from '@mui/icons-material';
import {
    Button,
    colors,
    FormControlLabel,
    Icon,
    IconButton,
    Paper,
    Portal,
    Radio,
    RadioGroup,
    Typography,
} from '@mui/material';
import classNames from 'classnames';
import { FC, useLayoutEffect, useState } from 'react';
import { ShowCardModal } from '../../../../controls/Modals/ShowCardModal';
import { useEventListener } from '../../../../hooks/useEventListener';
import { Card } from '../../../../types/Collection';
import styles from './styles.module.css';

interface RepeatCardProps {
    card: Card;
    showNext: boolean;
    showPrevious: boolean;
    errorMessage?: string;
    value: number | null;
    onNext: () => void;
    onPrevious: () => void;
    onChange: (weight: number | undefined) => void;
    onFinish: () => void;
    isActive: boolean;
    forceShowError: boolean;
}

export const RepeatCard: FC<RepeatCardProps> = ({
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
    const [showCardInfoModal, setShowCardInfoModal] = useState(false);

    useEventListener('keydown', (e) => {
        e.key === '1' && onChange(0);
        e.key === '2' && onChange(0.5);
        e.key === '3' && onChange(1);
    });

    return (
        <Paper className={styles.container}>
            <Portal>
                {showCardInfoModal && (
                    <ShowCardModal
                        open
                        onClose={() => setShowCardInfoModal(false)}
                        userId={card.userId}
                        collectionId={card.collectionId}
                        cardId={card.id}
                    />
                )}
            </Portal>
            <IconButton className={styles.infoIcon} onClick={() => setShowCardInfoModal(true)}>
                <InfoOutlined />
            </IconButton>
            {isActive && (
                <IconButton
                    className={styles.refreshIcon}
                    onClick={() => {
                        setIsError(false);
                        onChange(undefined);
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
                    tabIndex={0}
                    row
                    onChange={(e, v) => {
                        if (isError) {
                            setIsError(false);
                        }
                        onChange(parseFloat(v));
                    }}
                >
                    <FormControlLabel
                        tabIndex={0}
                        checked={value === 0}
                        value={0}
                        control={<Radio />}
                        label="Не помню"
                    />
                    <FormControlLabel
                        tabIndex={0}
                        checked={value === 0.5}
                        value={0.5}
                        control={<Radio />}
                        label="Помню частично"
                    />
                    <FormControlLabel tabIndex={0} checked={value === 1} value={1} control={<Radio />} label="Помню" />
                </RadioGroup>

                <div
                    className={styles.errorMessage}
                    style={{
                        border: `1px solid ${colors.red[400]}`,
                        visibility: isError ? 'visible' : 'hidden',
                    }}
                >
                    {errorMessage}
                </div>
            </div>
            <div className={styles.buttonsContainer}>
                {showPrevious ? (
                    <Button
                        tabIndex={1}
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
                        tabIndex={2}
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
                        tabIndex={2}
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
