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
import { FC, useEffect, useRef, useState } from 'react';
import { ShowCardModal } from '../../../../controls/Modals/ShowCardModal';
import { PaperCard } from '../../../../controls/PaperCard/PaperCard';
import { getCardUniqueKey } from '../../../../redux/slices/cardsSlice';
import { Card } from '../../../../types/Collection';
import { RememberList } from './RememberList/RememberList';
import styles from './styles.module.css';
import { HidableText } from '../../../../controls/HidableText/HidableText';

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
    isValuesHidden: boolean;
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
    isValueSideDefault,
}) => {
    const [isValuesHidden, setValuesHidden] = useState(true);
    const { current: cardIdToProps } = useRef<Record<string, CardProps>>({});

    useEffect(() => {
        cardIdToProps[getCardUniqueKey(card)] = { isValuesHidden: isValuesHidden };
    }, [isValuesHidden]);

    useEffect(() => {
        const saveItem = cardIdToProps[getCardUniqueKey(card)];
        setValuesHidden(saveItem ? saveItem.isValuesHidden : true);
    }, [card]);

    const [isError, setIsError] = useState(false);
    const [showCardInfoModal, setShowCardInfoModal] = useState(false);

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

                        <HidableText
                            size="small"
                            text={card.promptText || ''}
                            forceVisible={!isValuesHidden}
                            onChange={() => setValuesHidden(true)}
                        />
                    </Stack>

                    <HidableText
                        size="medium"
                        text={backText}
                        forceVisible={!isValuesHidden}
                        onChange={() => setValuesHidden(true)}
                    />
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
                        const newValue = parseFloat(v);
                        onChange(newValue);
                        if (newValue > 0.95 || newValue < 0.05) {
                            setValuesHidden(false);
                        }
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
