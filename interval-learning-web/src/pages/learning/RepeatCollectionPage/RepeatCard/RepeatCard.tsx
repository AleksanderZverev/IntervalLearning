import { Construction, History, InfoOutlined, MoreTime, RefreshOutlined, TimerOff } from '@mui/icons-material';
import {
    Button,
    CircularProgress,
    colors,
    Divider,
    FormControlLabel,
    IconButton,
    ListItemIcon,
    ListItemText,
    Menu,
    MenuItem,
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
import { AssertionModal } from '../../../../controls/Modals/AssertionModal';
import {
    usePostponeRepeatingCardMutation,
    useRelearnCardMutation,
    useStopRepeatingCardMutation,
} from '../../../../redux/cardsApi';
import { Schedule } from '../../../../types/schedule';
import { RememberForm } from '../RepeatCollectionPage.logic';
import { FormField } from '../../../../controls/Form/Form';
import _ from 'lodash';
import { TagsList } from '../../../../controls/Tags/TagsList/TagsList';
import { CardHelper } from '../../../../helpers/Study/CardHelper';

interface RepeatCardProps {
    card: Card;
    schedule: Schedule;
    showNext: boolean;
    showPrevious: boolean;
    errorMessage?: string;
    value: RememberForm | null;
    onNext: () => void;
    onPrevious: () => void;
    onChange: (weight: number | undefined, comment: string | undefined | null) => void;
    onFinish: () => void;
    onCardDeletedFromRepeating: (cardId: string) => void;
    canDropAnswer: boolean;
    forceShowError: boolean;
    isValueSideDefault: boolean;
}

interface CardProps {
    backIsHidden: boolean;
    promptIsHidden: boolean;
}

export const RepeatCard: FC<RepeatCardProps> = ({
    card,
    schedule,
    showNext,
    showPrevious,
    value,
    onNext,
    onPrevious,
    onFinish: onEndButtonClick,
    errorMessage,
    canDropAnswer,
    onChange,
    isValueSideDefault,
    ...props
}) => {
    const [backIsHidden, setBackIsHidden] = useState(true);
    const [promptIsHidden, setPromptIsHidden] = useState(true);
    const { current: cardIdToProps } = useRef<Record<string, CardProps>>({});

    const [showStopRepeatingModel, setStopRepeatingModel] = useState(false);
    const [menuAnchorEl, setMenuAnchorEl] = useState<null | HTMLElement>(null);
    const openMenu = Boolean(menuAnchorEl);

    const [stopRepeatingCard, stopRepeatingCardState] = useStopRepeatingCardMutation();
    const [stoppedRepeatingCardIds, setIsStoppedRepeatingCardIds] = useState<string[]>([]);
    const canStopRepeating = !stoppedRepeatingCardIds.includes(card.id);

    const [showPostponeRepeatingModel, setPostponeRepeatingModel] = useState(false);
    const [postponeRepeatingCard, postponeRepeatingCardState] = usePostponeRepeatingCardMutation();

    const [showRelearnModal, setRelearnModal] = useState(false);
    const [relearnCard, relearnCardState] = useRelearnCardMutation();

    useEffect(() => {
        cardIdToProps[getCardUniqueKey(card)] = { backIsHidden, promptIsHidden };
    }, [backIsHidden, promptIsHidden]);

    useEffect(() => {
        const saveItem = cardIdToProps[getCardUniqueKey(card)];
        setBackIsHidden(_.isUndefined(saveItem?.backIsHidden) ? true : saveItem.backIsHidden);
        setPromptIsHidden(_.isUndefined(saveItem?.promptIsHidden) ? true : saveItem.promptIsHidden);
    }, [card]);

    const [isError, setIsError] = useState(false);
    const [showCardInfoModal, setShowCardInfoModal] = useState(false);

    const frontText = isValueSideDefault ? card.backSideText : card.frontSideText;
    const backText = isValueSideDefault ? card.frontSideText : card.backSideText;

    const onStopRepeatingCard = async () => {
        if (!canStopRepeating) return;

        setStopRepeatingModel(false);
        setMenuAnchorEl(null);
        try {
            const cardId = card.id;
            await stopRepeatingCard({
                userId: card.userId,
                collectionId: card.collectionId,
                request: {
                    cardId: card.id,
                    scheduleUserId: schedule.userId,
                    scheduleId: schedule.id,
                },
            });
            setIsStoppedRepeatingCardIds([...stoppedRepeatingCardIds, cardId]);
            props.onCardDeletedFromRepeating(cardId);
        } catch {}
    };

    const onPostponeRepeatingCard = async () => {
        setPostponeRepeatingModel(false);
        setMenuAnchorEl(null);
        try {
            const cardId = card.id;
            await postponeRepeatingCard({
                userId: card.userId,
                collectionId: card.collectionId,
                request: {
                    cardId: card.id,
                    scheduleUserId: schedule.userId,
                    scheduleId: schedule.id,
                    postponeDays: 1,
                },
            });
            props.onCardDeletedFromRepeating(cardId);
        } catch {}
    };

    const onRelearnCard = async () => {
        setRelearnModal(false);
        setMenuAnchorEl(null);
        try {
            const cardId = card.id;
            await relearnCard({
                userId: card.userId,
                collectionId: card.collectionId,
                request: {
                    cardId: card.id,
                    scheduleUserId: schedule.userId,
                    scheduleId: schedule.id,
                },
            });
            props.onCardDeletedFromRepeating(cardId);
        } catch {}
    };

    return (
        <PaperCard
            topRightControl={
                <IconButton onClick={() => setShowCardInfoModal(true)}>
                    <InfoOutlined />
                </IconButton>
            }
            topLeftControl={
                <>
                    <IconButton onClick={(e) => setMenuAnchorEl(e.currentTarget)}>
                        <Construction />
                    </IconButton>
                    <Menu open={openMenu} anchorEl={menuAnchorEl} onClose={() => setMenuAnchorEl(null)}>
                        <MenuItem
                            onClick={() => {
                                setIsError(false);
                                onChange(undefined, value?.comment);
                                setMenuAnchorEl(null);
                            }}
                            disabled={!canDropAnswer}
                        >
                            <ListItemIcon>
                                <RefreshOutlined />
                            </ListItemIcon>
                            <ListItemText>Сбросить выбор</ListItemText>
                        </MenuItem>
                        <MenuItem
                            disabled={postponeRepeatingCardState.isLoading}
                            onClick={() => setPostponeRepeatingModel(true)}
                        >
                            <ListItemIcon>
                                {postponeRepeatingCardState.isLoading ? <CircularProgress size={16} /> : <MoreTime />}
                            </ListItemIcon>
                            <ListItemText>Отложить до завтра</ListItemText>
                        </MenuItem>
                        <Divider />
                        <MenuItem
                            disabled={stopRepeatingCardState.isLoading || !canStopRepeating}
                            onClick={() => setStopRepeatingModel(true)}
                        >
                            <ListItemIcon>
                                {stopRepeatingCardState.isLoading ? <CircularProgress size={16} /> : <TimerOff />}
                            </ListItemIcon>
                            <ListItemText>Перестать повторять</ListItemText>
                        </MenuItem>
                        <MenuItem disabled={relearnCardState.isLoading} onClick={() => setRelearnModal(true)}>
                            <ListItemIcon>
                                {relearnCardState.isLoading ? <CircularProgress size={16} /> : <History />}
                            </ListItemIcon>
                            <ListItemText>Изучить заново</ListItemText>
                        </MenuItem>
                    </Menu>
                </>
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
                            if (typeof value?.weight !== 'number') {
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
                            if (typeof value?.weight !== 'number') {
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
            <div className={styles.container} style={{ whiteSpace: 'pre-line' }}>
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
                    {showStopRepeatingModel && (
                        <AssertionModal
                            title="Завершение повторения карточки"
                            message={`Карточка «${card.backSideText} - ${card.frontSideText}» будет удалена из повторения`}
                            assertTitle="Подтвердить"
                            onClose={() => setStopRepeatingModel(false)}
                            onAssert={() => onStopRepeatingCard()}
                        />
                    )}
                    {showPostponeRepeatingModel && (
                        <AssertionModal
                            title="Отладка карточки до следующего дня"
                            message={`Карточка «${card.backSideText} - ${card.frontSideText}» будет отложена до завтрашнего дня`}
                            assertTitle="Отложить"
                            onClose={() => setPostponeRepeatingModel(false)}
                            onAssert={() => onPostponeRepeatingCard()}
                        />
                    )}
                    {showRelearnModal && (
                        <AssertionModal
                            title="Сброс изучения карточки"
                            message={
                                `Карточка «${card.backSideText} - ${card.frontSideText}» будет добавлена в список ` +
                                `повторения, а дальнейшие повторения отменены`
                            }
                            assertTitle="Изучить снова"
                            onClose={() => setRelearnModal(false)}
                            onAssert={() => onRelearnCard()}
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
                            hidden={promptIsHidden}
                            onChange={(isHidden) => setPromptIsHidden(isHidden)}
                        />
                    </Stack>

                    <HidableText
                        size="medium"
                        text={backText}
                        hidden={backIsHidden}
                        onChange={(isHidden) => setBackIsHidden(isHidden)}
                    />

                    <TagsList cardUniqueId={CardHelper.GetCardUniqueId(card)} tags={card.tags} />
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
                        onChange(newValue, value?.comment);
                        if (newValue > 0.95 || newValue < 0.05) {
                            setPromptIsHidden(false);
                            setBackIsHidden(false);
                        }
                    }}
                >
                    <FormControlLabel
                        tabIndex={0}
                        checked={value?.weight === 0}
                        value={0}
                        control={<Radio />}
                        label="Не помню"
                    />
                    <FormControlLabel
                        tabIndex={0}
                        checked={value?.weight === 0.5}
                        value={0.5}
                        control={<Radio />}
                        label="Помню частично"
                    />
                    <FormControlLabel
                        tabIndex={0}
                        checked={value?.weight === 1}
                        value={1}
                        control={<Radio />}
                        label="Помню"
                    />
                </RadioGroup>

                <div className={styles.memoInput}>
                    {isError ? (
                        <div
                            className={styles.errorMessage}
                            style={{
                                border: `1px solid ${colors.red[400]}`,
                                visibility: isError ? 'visible' : 'hidden',
                            }}
                        >
                            {errorMessage}
                        </div>
                    ) : (
                        <FormField
                            size="small"
                            variant="outlined"
                            label="Заметка"
                            fontSize={16}
                            value={value?.comment || ''}
                            onChange={(e) => onChange(value?.weight, e.target.value || undefined)}
                        />
                    )}
                </div>
            </div>
        </PaperCard>
    );
};
