import { FC, useMemo, useState } from 'react';
import useTypedSelector from '../../../hooks/useTypedSelector';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { selectCollectionById } from '../../../redux/slices/collectionsSlice';
import {
    withMutationResolver,
    withOtherQueryResolver,
    withQueryResolver,
    WithQueryResolverData,
    WithMutationResolverProps,
} from '../../../hoc/withQueryResolver';
import { CardsItem, useGetNotStartedCardsQuery, useStartCardsMutation } from '../../../redux/cardsApi';
import { PageContainer } from '../../../controls/PageContainer/PageContainer';
import { PageHeader } from '../../../controls/PageHeader/PageHeader';
import { selectTheme } from '../../../redux/slices/themeSlice';
import { Slider } from '../../../controls/Slider/Slider';
import { Button, IconButton, Stack, Tooltip } from '@mui/material';
import { LearnCard } from './LearnCard/LearnCard';
import { useGetCollectionQuery } from '../../../redux/collectionApi';
import { AssertionModal } from '../../../controls/Modals/AssertionModal';
import { CardResult } from '../CardResult/CardResult';
import { getScheduleId, selectScheduleById } from '../../../redux/slices/scheduleSlice';
import { LightTooltip } from '../../../controls/LightTooltip/LightTooltip';
import { Casino, HelpOutline } from '@mui/icons-material';
import { selectCards } from '../../../redux/slices/cardsSlice';
import dayjs from 'dayjs';
import { getRepeatingNavigationLink } from '../LearningPage/InProgressCollections/InProgressCollections';
import { useGetScheduleQuery } from '../../../redux/schedulesSlice';
import { ArrayHelper } from '../../../helpers/ArrayHelper';

type WithResolvers = WithQueryResolverData<typeof useGetNotStartedCardsQuery> &
    WithMutationResolverProps<typeof useStartCardsMutation>;

interface LearnCollectionPageContentProps extends WithResolvers {
    userId: string;
    collectionId: string;
    scheduleId: string;
    scheduleUserId: string;
    setDisableLoading: (disable: boolean) => void;
}

export const LearnCollectionPageContent: FC<LearnCollectionPageContentProps> = ({
    userId,
    collectionId,
    scheduleId,
    scheduleUserId,
    setDisableLoading,
    queryData: notStartedCardIdsOrdered,
    mutationProps: { mutate: startCards, showRetryModal, isLoading: isMutationLoading, isSuccess, data: mutationData },
}) => {
    const collection = useTypedSelector((state) => selectCollectionById(state, userId, collectionId));
    const schedule = useTypedSelector((state) => selectScheduleById(state, getScheduleId(scheduleUserId, scheduleId)));
    const cards = useTypedSelector((state) => selectCards(state, userId, collectionId));

    if (!collection || !schedule || !cards) {
        console.debug(collection, schedule, cards);
        throw new Error();
    }

    const theme = useTypedSelector((state) => selectTheme(state, collection.themeId));

    const navigate = useNavigate();
    const [showAssetModal, setShowAssertModal] = useState(false);
    const [showStartModal, setShowStartModal] = useState(true);
    const [forceShowStartModal, setForceShowStartModal] = useState(false);
    const [showMoveToRepeatModal, setShowMoveToRepeatModal] = useState(false);
    const [activeCardIndex, setActiveCardIndex] = useState(0);
    const [cardIndex, setCardIndex] = useState(0);
    const [shuffled, setShuffled] = useState(false);

    const notStartedCardIds = useMemo(() => {
        if (notStartedCardIdsOrdered.length === 0) {
            return [];
        }

        const cardsIds = [...notStartedCardIdsOrdered];

        if (!shuffled) {
            return cardsIds;
        }

        const learnedCardsIds = cardsIds.slice(0, activeCardIndex + 1);
        const cardsIdsToShuffle = cardsIds.slice(activeCardIndex + 1);
        ArrayHelper.shuffleArray(cardsIdsToShuffle);
        return [...learnedCardsIds, ...cardsIdsToShuffle];
    }, [shuffled, notStartedCardIdsOrdered]);

    if (notStartedCardIds.length === 0) {
        return <div>No cards</div>;
    }

    const maxCards = notStartedCardIds.length;

    const currentCardId = notStartedCardIds[cardIndex];

    const onSuccessFinish = (fromModal: boolean) => {
        if (mutationData && !fromModal) {
            const now = dayjs();
            const date = dayjs(mutationData.nextRepeatDate);
            const diffMinutes = date.diff(now, 'minutes');

            if (diffMinutes <= 1) {
                setShowMoveToRepeatModal(true);
                return;
            }
        }

        if (mutationData && mutationData.nextPhaseIndex >= 0 && fromModal) {
            const now = dayjs();
            navigate(
                getRepeatingNavigationLink(
                    userId,
                    collectionId,
                    scheduleUserId,
                    scheduleId,
                    mutationData.nextPhaseIndex,
                    mutationData.nextRepeatDate ?? now.toISOString()
                )
            );
            return;
        }

        navigate('/learning');
    };

    const onFinish = async (fromAssertionModal: boolean) => {
        if (isMutationLoading || isSuccess) {
            if (isSuccess) {
                onSuccessFinish(false);
            }
            return;
        }

        if (!fromAssertionModal) {
            setShowAssertModal(true);
            return;
        }

        if (fromAssertionModal && showAssetModal) {
            setShowAssertModal(false);
        }

        const cardIdsToStart = [...notStartedCardIds];

        if (cardIndex + 1 < maxCards) {
            cardIdsToStart.splice(cardIndex + 1);
        }

        const item: CardsItem = {
            scheduleId,
            scheduleUserId,
            cardIds: [...cardIdsToStart],
        };

        try {
            setDisableLoading(true);
            await startCards({ userId, collectionId, request: item });
            setActiveCardIndex(activeCardIndex + 1);
        } catch {
            setDisableLoading(false);
            showRetryModal(() => onFinish(true));
        }
    };

    const onNext = () => {
        setCardIndex(cardIndex + 1);
        setActiveCardIndex(activeCardIndex + 1);
    };

    const onPrevious = () => {
        setCardIndex(cardIndex - 1);
        setActiveCardIndex(activeCardIndex - 1);
    };

    return (
        <PageContainer transparent>
            <PageHeader
                title={collection?.title || ''}
                subTitle={theme?.name || ''}
                subMenu={
                    <Button variant="outlined" onClick={() => onFinish(cardIndex + 1 === maxCards)}>
                        Завершить
                    </Button>
                }
            />
            <div
                style={{
                    marginTop: 10,
                    cursor: 'pointer',
                    color: '#b7b7b7',
                    alignSelf: 'start',
                    display: 'flex',
                    justifyContent: 'space-between',
                }}
            >
                {schedule.description ? (
                    <LightTooltip
                        open={Boolean(schedule.shortDescription) ? undefined : false}
                        placement="bottom-start"
                        title={
                            <div style={{ padding: 5, fontSize: 18, fontWeight: 'normal' }}>
                                {schedule.shortDescription}
                            </div>
                        }
                        sx={{ maxWidth: '70%' }}
                    >
                        <div onClick={() => setForceShowStartModal(true)}>
                            <Stack direction={'row'} alignItems={'center'} columnGap={'5px'}>
                                <HelpOutline />
                                <span>Что делать?</span>
                            </Stack>
                        </div>
                    </LightTooltip>
                ) : (
                    <div />
                )}
                {shuffled ? (
                    <div />
                ) : (
                    <Tooltip title="Перемешать оставшиеся">
                        <IconButton onClick={() => !shuffled && setShuffled(true)}>
                            <Casino />
                        </IconButton>
                    </Tooltip>
                )}
            </div>

            {(showStartModal || forceShowStartModal) && schedule && schedule.description && (
                <AssertionModal
                    title={`Учебный план: ${schedule.title}`}
                    message={schedule.description}
                    assertTitle="OK"
                    forceOpen={forceShowStartModal}
                    onClose={() => {
                        setShowStartModal(false);
                        if (forceShowStartModal) {
                            setForceShowStartModal(false);
                        }
                    }}
                    forbidShowingKey={`${scheduleUserId}-${scheduleId}`}
                />
            )}
            {showAssetModal && (
                <AssertionModal
                    title="Не все карточки изучены"
                    message="Завершить изучение на текущей карточке?"
                    assertTitle="Да"
                    cancelTitle="Отмена"
                    onAssert={() => onFinish(true)}
                    onClose={() => setShowAssertModal(false)}
                />
            )}
            {showMoveToRepeatModal && (
                <AssertionModal
                    title="Слова необходимо повторить"
                    message="Перейти к повторению?"
                    assertTitle="Да"
                    cancelTitle="Нет"
                    onAssert={() => onSuccessFinish(true)}
                    onClose={() => setShowMoveToRepeatModal(false)}
                />
            )}
            <div
                style={{
                    width: '100%',
                    margin: '10px 0',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    columnGap: 45,
                }}
            >
                <Slider
                    value={cardIndex}
                    min={0}
                    max={maxCards - 1}
                    activeValue={activeCardIndex}
                    onValueChange={(v) => {
                        setCardIndex(v);
                        setActiveCardIndex(v);
                    }}
                    finishMode={isSuccess}
                    getHoverTitle={(index) => {
                        const cardId = notStartedCardIds[index];
                        const target = cards.find((c) => c.id === cardId);
                        return target?.frontSideText ?? index.toString();
                    }}
                    vertical
                />

                {isSuccess && mutationData && (
                    <CardResult
                        nextRepeatDate={mutationData.nextRepeatDate}
                        wordsLearned={cardIndex + 1}
                        onEndButtonClick={() => onSuccessFinish(false)}
                    />
                )}

                {currentCardId && !isSuccess && (
                    <LearnCard
                        userId={userId}
                        collectionId={collectionId}
                        cardId={currentCardId}
                        showNext={cardIndex < maxCards - 1}
                        showPrevious={cardIndex !== 0}
                        onFinish={() => onFinish(true)}
                        onNext={onNext}
                        onPrevious={onPrevious}
                    />
                )}
            </div>
        </PageContainer>
    );
};

const ConnectedLearnCollectionPage = withQueryResolver(useGetNotStartedCardsQuery)(LearnCollectionPageContent);
const ConnectedCollectionResolver = withOtherQueryResolver(useGetCollectionQuery)(ConnectedLearnCollectionPage);
const ConnectedScheduleResolver = withOtherQueryResolver(useGetScheduleQuery)(ConnectedCollectionResolver);

const ConnectedMutationResolver = withMutationResolver(
    useStartCardsMutation,
    'Не удалось завершить изучение коллекции'
)(ConnectedScheduleResolver);

interface CardResult {
    nextRepeatDate: string | null;
    learnedCardsCount: number;
}

export const LearnCollection: FC = () => {
    const { userId, collectionId } = useParams();

    const location = useLocation();
    const params = new URLSearchParams(location.search);
    const scheduleUserId = params.get('scheduleUserId');
    const scheduleId = params.get('scheduleId');
    const cardsCountString = params.get('cardsCount');

    if (!collectionId || !userId || !scheduleUserId || !scheduleId || !cardsCountString) {
        throw new Error();
    }

    let cardsCount = parseInt(cardsCountString);
    const [disableLoading, setDisableLoading] = useState(false);

    if (isNaN(cardsCount) || cardsCount > 1000 || cardsCount < 1) {
        cardsCount = 30;
        params.set('cardsCount', cardsCount.toString());

        if (typeof window !== 'undefined') {
            const newUrl = location.pathname + '?' + params.toString();
            window.history.replaceState(null, '', newUrl);
        }
    }

    return (
        <ConnectedMutationResolver
            queryArg={{
                userId,
                scheduleUserId,
                scheduleId,
                collectionId,
                request: { scheduleUserId, scheduleId, count: cardsCount },
            }}
            disableLoading={disableLoading}
            scheduleUserId={scheduleUserId}
            scheduleId={scheduleId}
            setDisableLoading={(disable) => setDisableLoading(disable)}
        />
    );
};
