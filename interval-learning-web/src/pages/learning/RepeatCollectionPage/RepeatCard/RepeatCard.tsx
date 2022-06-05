import { InfoOutlined, RefreshOutlined } from '@mui/icons-material';
import {
    Button,
    colors,
    FormControlLabel,
    IconButton,
    Portal,
    Radio,
    RadioGroup,
    Stack,
    Typography,
} from '@mui/material';
import classNames from 'classnames';
import { FC, useEffect, useState } from 'react';
import { ShowCardModal } from '../../../../controls/Modals/ShowCardModal';
import { PaperCard } from '../../../../controls/PaperCard/PaperCard';
import { useEventListener } from '../../../../hooks/useEventListener';
import { getCardUniqueKey } from '../../../../redux/slices/cardsSlice';
import { Card } from '../../../../types/Collection';
import { RememberList } from './RememberList/RememberList';
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
    isValueSideDefault: boolean;
}

interface CardProps {
    backIsHidden: boolean;
    promptIsHidden: boolean;
}

const cardIdToProps: Record<string, CardProps> = {};

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
    isValueSideDefault,
}) => {
    const [backIsHidden, setBackIsHidden] = useState(true);
    const [promptIsHidden, setPromptIsHidden] = useState(true);

    useEffect(() => {
        cardIdToProps[getCardUniqueKey(card)] = { backIsHidden, promptIsHidden };
    }, [backIsHidden, promptIsHidden]);

    useEffect(() => {
        const saveItem = cardIdToProps[getCardUniqueKey(card)];
        setBackIsHidden(saveItem ? saveItem.backIsHidden : true);
        setPromptIsHidden(saveItem ? saveItem.promptIsHidden : true);
    }, [card]);

    const [isError, setIsError] = useState(false);
    const [showCardInfoModal, setShowCardInfoModal] = useState(false);

    useEventListener('keydown', (e) => {
        e.key === '1' && onChange(0);
        e.key === '2' && onChange(0.5);
        e.key === '3' && onChange(1);
    });

    const frontText = isValueSideDefault ? card.backSideText : card.frontSideText;
    const backText = isValueSideDefault ? card.frontSideText : card.backSideText;

    return (
        <PaperCard
            topRightControl={
                <IconButton onClick={() => setShowCardInfoModal(true)}>
                    <InfoOutlined />
                </IconButton>
            }
            topLeftControl={
                isActive && (
                    <IconButton
                        onClick={() => {
                            setIsError(false);
                            onChange(undefined);
                        }}
                    >
                        <RefreshOutlined />
                    </IconButton>
                )
            }
            leftButton={
                showPrevious && (
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
                )
            }
            rightButton={
                showNext ? (
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
                )
            }
        >
            <div className={styles.container}>
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

                {card.remembers && card.remembers.length > 0 && <RememberList remembers={card.remembers} />}

                <Stack direction={'column'} gap="12px" alignItems={'center'}>
                    <Stack direction={'column'} gap="6px" alignItems={'center'}>
                        <Typography variant="h3" fontSize={32}>
                            {frontText}
                        </Typography>

                        <div
                            className={classNames(styles.backContainer, { [styles.backHidden]: promptIsHidden })}
                            onClick={() => setPromptIsHidden(!promptIsHidden)}
                        >
                            {card.promptText}
                        </div>
                    </Stack>

                    <div
                        className={classNames(styles.backContainer, { [styles.backHidden]: backIsHidden })}
                        onClick={() => setBackIsHidden(!backIsHidden)}
                    >
                        <Typography variant="h5" fontSize={24}>
                            {backText}
                        </Typography>
                    </div>
                </Stack>
                <RadioGroup
                    onKeyDownCapture={(e) => e.preventDefault()}
                    tabIndex={0}
                    row
                    sx={{ padding: '20px' }}
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
        </PaperCard>
    );
};
