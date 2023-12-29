import { FC, useState } from 'react';
import useTypedSelector from '../../../hooks/useTypedSelector';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { selectCollectionById } from '../../../redux/slices/collectionsSlice';
import { RepeatCard } from './RepeatCard/RepeatCard';
import {
    withMutationResolver,
    WithMutationResolverProps,
    withOtherQueryResolver,
    withQueryResolver,
    WithQueryResolverData,
} from '../../../hoc/withQueryResolver';
import { RememberRequest, useGetRepeatCardsQuery, usePatchRememberCardsMutation } from '../../../redux/cardsApi';
import { PageContainer } from '../../../controls/PageContainer/PageContainer';
import { PageHeader } from '../../../controls/PageHeader/PageHeader';
import { selectTheme } from '../../../redux/slices/themeSlice';
import { CenterContainer } from '../../../controls/CenterContainer/CenterContainer';
import { Slider } from '../../../controls/Slider/Slider';
import { Button, Paper, Stack } from '@mui/material';
import { LocalStorageHelper } from '../../../helpers/localStorageHelper';
import { useGetCollectionQuery } from '../../../redux/collectionApi';
import { selectCardsByIds } from '../../../redux/slices/cardsSlice';
import { AssertionModal } from '../../../controls/Modals/AssertionModal';
import { CardResult } from '../CardResult/CardResult';
import { getScheduleId, selectScheduleById } from '../../../redux/slices/scheduleSlice';
import { LightTooltip } from '../../../controls/LightTooltip/LightTooltip';
import { HelpOutline } from '@mui/icons-material';
import dayjs from 'dayjs';
import { getRepeatingNavigationLink } from '../LearningPage/InProgressCollections/InProgressCollections';
import { ErrorPage } from '../../../controls/ErrorPage/ErrorPage';
import { useGetScheduleQuery } from '../../../redux/schedulesSlice';
import Head from 'next/head';
import { useDocumentTitle } from '../../../hooks/useCollectionTitle';

type WithResolvers = WithQueryResolverData<typeof useGetRepeatCardsQuery> &
    WithMutationResolverProps<typeof usePatchRememberCardsMutation>;

interface RepeatCollectionPageContentProps extends WithResolvers {
    userId: string;
    collectionId: string;
    scheduleUserId: string;
    scheduleId: string;
    phaseIndex: number;
    setSkipLoading: (skip: boolean) => void;
    date: string;
}

export const RepeatCollectionPageContent: FC<RepeatCollectionPageContentProps> = ({
    queryData: { cardIds },
    userId,
    collectionId,
    scheduleUserId,
    scheduleId,
    phaseIndex,
    setSkipLoading,
    date,
    mutationProps: {
        mutate: rememberCards,
        data: mutationData,
        showRetryModal,
        isLoading,
        isSuccess,
        reset: mutationReset,
    },
}) => {
    const collection = useTypedSelector((state) => selectCollectionById(state, userId, collectionId));
    const schedule = useTypedSelector((state) => selectScheduleById(state, getScheduleId(scheduleUserId, scheduleId)));

    if (!collection || !schedule) {
        throw new Error();
    }

    useDocumentTitle(collection.title, '🧠');

    const navigate = useNavigate();
    const theme = useTypedSelector((state) => selectTheme(state, collection.themeId));
    const [deletedCards, setDeletedCards] = useState<string[]>([]);
    const mergedCardIds = cardIds.filter((c) => !deletedCards.includes(c));
    const repeatCards = useTypedSelector((state) => selectCardsByIds(state, userId, collectionId, mergedCardIds));
    const [showAssertionModal, setShowAssertionModal] = useState(false);
    const [showCurrentCardError, setShowCurrentCardError] = useState(false);
    const [showStartModal, setShowStartModal] = useState(true);
    const [forceShowStartModal, setForceShowStartModal] = useState(false);
    const [showMoveToRepeatModal, setShowMoveToRepeatModal] = useState(false);

    const [rememberWeights, setRememberWeights] = useState<Record<string, number | undefined>>(
        () => LocalStorageHelper.getRepeatingCards(schedule.userId, schedule.id, phaseIndex, date, collection.id) ?? {}
    );

    const maxCards = repeatCards.length;

    let notActiveIndex = repeatCards.map((c) => rememberWeights[c.id]).indexOf(undefined);
    notActiveIndex = notActiveIndex < 0 ? maxCards : notActiveIndex;

    const [cardIndex, setCardIndex] = useState(
        notActiveIndex >= maxCards ? maxCards - 1 : notActiveIndex - 1 >= 0 ? notActiveIndex - 1 : 0
    );
    const card = repeatCards[cardIndex];

    const currentCard = repeatCards[cardIndex];

    const saveWeights = (clearSaves: boolean) => {
        LocalStorageHelper.saveRepeatingCardsWeights(
            schedule.userId,
            schedule.id,
            phaseIndex,
            collection.id,
            date,
            clearSaves ? {} : rememberWeights
        );
    };

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

        if (mutationData && mutationData.nextRepeatDate && fromModal) {
            setSkipLoading(false);
            mutationReset();

            navigate(
                getRepeatingNavigationLink(
                    userId,
                    collectionId,
                    scheduleUserId,
                    scheduleId,
                    mutationData.nextPhaseIndex,
                    mutationData.nextRepeatDate
                )
            );
            return;
        }
        navigate('/learning');
    };

    const onFinish = async (fromAssertionModal: boolean) => {
        if (isLoading || isSuccess) {
            if (isSuccess) {
                onSuccessFinish(false);
            }
            return;
        }

        const resultWeights = Object.entries(rememberWeights);

        if (resultWeights.some(([_, weight]) => weight === undefined || weight === null)) {
            console.error('remember weights are incorrect');
            return;
        }

        if (rememberWeights[card.id] === undefined || rememberWeights[card.id] === null) {
            setShowCurrentCardError(true);
            return;
        }

        if (!fromAssertionModal && cardIndex + 1 < maxCards) {
            setShowAssertionModal(true);
            return;
        }

        const request: RememberRequest = {
            scheduleUserId,
            scheduleId,
            phaseIndex,
            rememberItems: resultWeights.map(([cardId, weight]) => ({ cardId, weight: weight ?? 0 })),
        };
        try {
            setSkipLoading(true);
            await rememberCards({ userId, collectionId, request });
            saveWeights(true);
        } catch (e) {
            setSkipLoading(false);
            showRetryModal(() => onFinish(true));
        }
    };

    const onChange = (weight: number | undefined) => {
        if (weight !== undefined) {
            rememberWeights[card.id] = weight;
        } else {
            delete rememberWeights[card.id];
        }

        setRememberWeights({ ...rememberWeights });
        saveWeights(false);
    };

    const onDeleteCardFromRepeating = (cardId: string) => {
        setDeletedCards([...deletedCards, cardId]);
    };

    const onNext = () => {
        setCardIndex(cardIndex + 1);
    };

    const onPrevious = () => {
        setCardIndex(cardIndex - 1);
    };

    const isEmptyCollection = repeatCards.length === 0;

    const phase = schedule.phases[phaseIndex];

    const phaseShortDescription =
        phase.shortDescription ||
        (phase.secondsFromLastPhase < 10
            ? schedule.defaultRepeatPhaseShortDescription
            : schedule.defaultPhaseShortDescription);

    return (
        <PageContainer transparent>
            <PageHeader
                title={collection?.title || ''}
                subTitle={theme?.name || ''}
                subMenu={
                    !isEmptyCollection && (
                        <Button
                            variant="outlined"
                            onClick={() => (isSuccess ? onSuccessFinish(false) : onFinish(false))}
                        >
                            Завершить
                        </Button>
                    )
                }
            />
            <div>
                {(showStartModal || forceShowStartModal) && phase && phase.description && (
                    <AssertionModal
                        forceOpen={forceShowStartModal}
                        title={`Учебный план: ${schedule.title}`}
                        message={
                            phase.description ||
                            (phase.secondsFromLastPhase < 10
                                ? schedule.defaultRepeatPhaseDescription
                                : schedule.defaultPhaseDescription)
                        }
                        assertTitle="OK"
                        onClose={() => {
                            setShowStartModal(false);
                            setForceShowStartModal(false);
                        }}
                        forbidShowingKey={`${scheduleUserId}-${scheduleId}`}
                    />
                )}
                {showCurrentCardError && (
                    <AssertionModal
                        title="Значение не выбрано"
                        message="Выберите значение текущей карточки"
                        assertTitle="OK"
                        onClose={() => setShowCurrentCardError(false)}
                    />
                )}
                {showAssertionModal && (
                    <AssertionModal
                        title="Не все карточки повторены"
                        message="Завершить повторение на текущей карточке?"
                        assertTitle="Да"
                        cancelTitle="Отмена"
                        onClose={() => setShowAssertionModal(false)}
                        onAssert={() => {
                            setShowAssertionModal(false);
                            onFinish(true);
                        }}
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
                    style={{ marginTop: 10, cursor: 'pointer', color: '#b7b7b7' }}
                    onClick={() => setForceShowStartModal(true)}
                >
                    {phase && (
                        <LightTooltip
                            open={Boolean(phaseShortDescription) ? undefined : false}
                            placement="bottom-start"
                            title={
                                <div style={{ padding: 5, fontSize: 18, fontWeight: 'normal' }}>
                                    {phaseShortDescription}
                                </div>
                            }
                            sx={{ maxWidth: '70%' }}
                        >
                            <Stack direction={'row'} alignItems={'center'} columnGap={'5px'}>
                                <HelpOutline />
                                <span>
                                    {phase.secondsFromLastPhase < 10
                                        ? `Повторение ${
                                              phaseIndex === 0
                                                  ? 'после изучения'
                                                  : 'интервала: ' +
                                                    dayjs
                                                        .duration(
                                                            schedule.phases[phaseIndex - 1].secondsFromLastPhase,
                                                            's'
                                                        )
                                                        .humanize()
                                          }`
                                        : `Спустя ${dayjs.duration(phase.secondsFromLastPhase, 's').humanize()}`}
                                </span>
                            </Stack>
                        </LightTooltip>
                    )}
                </div>
                <CenterContainer>
                    {isEmptyCollection ? (
                        <Paper sx={{ padding: '30px 50px' }}>
                            <CenterContainer>
                                <div style={{ display: 'flex', flexDirection: 'column', rowGap: 10 }}>
                                    <div>Нет карт для повторения</div>
                                    <Button variant="outlined" onClick={() => navigate('/learning')}>
                                        Вернуться
                                    </Button>
                                </div>
                            </CenterContainer>
                        </Paper>
                    ) : (
                        <div
                            style={{
                                width: '100%',
                                display: 'flex',
                                flexDirection: 'column',
                                alignItems: 'center',
                                rowGap: 36,
                            }}
                        >
                            {isSuccess && mutationData && (
                                <CardResult
                                    wordsLearned={notActiveIndex > maxCards ? maxCards : notActiveIndex}
                                    nextRepeatDate={mutationData.nextRepeatDate}
                                    onEndButtonClick={() => onSuccessFinish(false)}
                                />
                            )}
                            {!isSuccess && currentCard && (
                                <RepeatCard
                                    value={rememberWeights[card.id] ?? null}
                                    card={currentCard}
                                    schedule={schedule}
                                    onCardDeletedFromRepeating={(cardId) => onDeleteCardFromRepeating(cardId)}
                                    showNext={cardIndex < maxCards - 1}
                                    showPrevious={cardIndex !== 0}
                                    isActive={notActiveIndex - 1 === cardIndex}
                                    onFinish={() => onFinish(false)}
                                    onNext={onNext}
                                    onChange={onChange}
                                    onPrevious={onPrevious}
                                    forceShowError={showCurrentCardError}
                                    errorMessage={'Помните слово?'}
                                    isValueSideDefault={phase.isDefaultValueSide}
                                />
                            )}

                            <Slider
                                value={cardIndex}
                                min={0}
                                max={maxCards - 1}
                                activeValue={notActiveIndex}
                                finishMode={isSuccess}
                                onValueChange={(v) => {
                                    if (v > notActiveIndex) return;
                                    setCardIndex(v);
                                }}
                                getHoverTitle={(index) => repeatCards[index].backSideText}
                            />
                        </div>
                    )}
                </CenterContainer>
            </div>
        </PageContainer>
    );
};

const ConnectedRepeatCollectionPage = withQueryResolver(useGetRepeatCardsQuery)(RepeatCollectionPageContent);
const CollectionQueryResolver = withOtherQueryResolver(useGetCollectionQuery)(ConnectedRepeatCollectionPage);
const WithSchedule = withOtherQueryResolver(useGetScheduleQuery)(CollectionQueryResolver);

const ConnectedMutationResolver = withMutationResolver(
    usePatchRememberCardsMutation,
    'Не удалось завершить повторение'
)(WithSchedule);

export const RepeatCollection: FC = () => {
    const { userId, collectionId } = useParams();

    const location = useLocation();
    const params = new URLSearchParams(location.search);
    const scheduleUserId = params.get('scheduleUserId');
    const scheduleId = params.get('scheduleId');
    const phaseIndexString = params.get('phaseIndex');
    const date = params.get('date');

    const [skipLoading, setSkipLoading] = useState(false);

    if (!collectionId || !userId) {
        throw new Error();
    }

    if (
        !scheduleUserId ||
        !scheduleId ||
        !date ||
        !dayjs(date).isValid() ||
        phaseIndexString === undefined ||
        phaseIndexString == null ||
        parseInt(phaseIndexString) < 0
    ) {
        return <ErrorPage errorMessage="Неверная ссылка" />;
    }

    const phaseIndex = parseInt(phaseIndexString);

    return (
        <ConnectedMutationResolver
            queryArg={{
                userId,
                collectionId,
                scheduleUserId,
                scheduleId,
                request: { scheduleUserId, scheduleId, phaseIndex, date },
            }}
            disableLoading={skipLoading}
            scheduleUserId={scheduleUserId}
            scheduleId={scheduleId}
            phaseIndex={phaseIndex}
            date={date}
            setSkipLoading={(skip) => setSkipLoading(skip)}
        />
    );
};
