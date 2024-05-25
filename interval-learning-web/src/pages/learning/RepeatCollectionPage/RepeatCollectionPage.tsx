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
import { Slider, SliderPointColor } from '../../../controls/Slider/Slider';
import { Button, Paper, Stack } from '@mui/material';
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
import { useDocumentTitle } from '../../../hooks/useCollectionTitle';
import {
    RememberForm,
    State,
    getDefaultState,
    getRepeatingCards,
    saveRepeatingCardsState,
} from './RepeatCollectionPage.logic';
import _ from 'lodash';

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

    const [state, setState] = useState<State>(
        getRepeatingCards(schedule.userId, schedule.id, phaseIndex, date, collection.id) ?? getDefaultState()
    );

    const updateState = (update: (state: State) => void) => {
        const newState = { ...state };
        update(newState);
        setState(newState);
        saveRepeatingCardsState(schedule.userId, schedule.id, phaseIndex, collection.id, date, newState);
    };

    const maxCards = repeatCards.length;
    const cardIndex = state.currentCardIndex;
    const card = repeatCards[cardIndex];
    const currentCard = repeatCards[cardIndex];

    const clearWeights = () => {
        saveRepeatingCardsState(schedule.userId, schedule.id, phaseIndex, collection.id, date, getDefaultState());
    };

    const onSuccessFinish = () => {
        if (mutationData) {
            const now = dayjs();
            const date = dayjs(mutationData.nextRepeatDate);
            const diffMinutes = date.diff(now, 'minutes');

            if (diffMinutes <= 1) {
                setShowMoveToRepeatModal(true);
                return;
            }
        }

        navigate('/learning');
    };

    const onGoToLearning = () => {
        navigate('/learning');
    };

    const onGoToRepeating = () => {
        if (!mutationData?.nextRepeatDate) return;

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
    };

    const onFinish = async (fromAssertionModal: boolean) => {
        if (isLoading || isSuccess) {
            if (isSuccess) {
                onSuccessFinish();
            }
            return;
        }

        const resultWeights = Object.entries(state.rememberWeights);

        if (resultWeights.some(([_, weight]) => weight === undefined || weight === null)) {
            console.error('remember weights are incorrect');
            return;
        }

        if (state.rememberWeights[card.id] === undefined || state.rememberWeights[card.id] === null) {
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
            rememberItems: resultWeights
                .filter(([cardId, form]) => {
                    if (_.isNil(form?.weight) || deletedCards.includes(cardId)) {
                        return false;
                    }

                    return true;
                })
                .map(([cardId, form]) => ({
                    cardId,
                    weight: form?.weight as number,
                    comment: form?.comment || null,
                })),
        };
        try {
            setSkipLoading(true);
            await rememberCards({ userId, collectionId, request });
            clearWeights();
        } catch (e) {
            setSkipLoading(false);
            showRetryModal(() => onFinish(true));
        }
    };

    const stepForwardRepeatedCardIndex = (newState: State) => {
        if (newState.currentCardIndex > newState.repeatedCardIndex) {
            newState.repeatedCardIndex = newState.currentCardIndex;
        }
    };

    const stepBackRepeatedCardIndex = (newState: State) => {
        if (newState.currentCardIndex === newState.repeatedCardIndex) {
            newState.repeatedCardIndex--;
        }
    };

    const onChange = (weight: number | undefined, comment: string | undefined | null) =>
        updateState((newState) => {
            newState.rememberWeights[card.id] = { weight: weight, comment: comment };

            if (weight !== undefined) {
                stepForwardRepeatedCardIndex(newState);
            } else {
                stepBackRepeatedCardIndex(newState);
            }
        });

    const onDeleteCardFromRepeating = (cardId: string) => {
        setDeletedCards([...deletedCards, cardId]);

        if (cardId in state.rememberWeights) {
            updateState((newState) => {
                delete newState.rememberWeights[cardId];
                newState.repeatedCardIndex--;
            });
        }

        if (state.currentCardIndex + 1 >= maxCards) {
            onPrevious();
        }
    };

    const onNext = () => {
        const newIndex = cardIndex + 1;

        if (newIndex >= maxCards) {
            return;
        }

        updateState((newState) => (newState.currentCardIndex = newIndex));
    };

    const onPrevious = () => {
        const newIndex = cardIndex - 1;

        if (newIndex < 0) {
            return;
        }

        updateState((newState) => (newState.currentCardIndex = newIndex));
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
                        <Button variant="outlined" onClick={() => (isSuccess ? onSuccessFinish() : onFinish(false))}>
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
                        onAssert={() => onGoToRepeating()}
                        onCancel={() => onGoToLearning()}
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
                                    cardMovementInfos={mutationData.cardMovementInfos}
                                    wordsLearned={state.repeatedCardIndex + 1}
                                    nextRepeatDate={mutationData.nextRepeatDate}
                                    rememberedWeights={
                                        Object.values(state.rememberWeights)
                                            .filter(Boolean)
                                            .map((v) => v?.weight) as number[]
                                    }
                                    onEndButtonClick={() => onSuccessFinish()}
                                />
                            )}
                            {!isSuccess && currentCard && (
                                <RepeatCard
                                    value={state.rememberWeights[card.id] ?? null}
                                    card={currentCard}
                                    schedule={schedule}
                                    onCardDeletedFromRepeating={(cardId) => onDeleteCardFromRepeating(cardId)}
                                    showNext={cardIndex < maxCards - 1}
                                    showPrevious={cardIndex !== 0}
                                    canDropAnswer={state.repeatedCardIndex === cardIndex}
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
                                activeValue={state.repeatedCardIndex + 1}
                                finishMode={isSuccess}
                                onValueChange={(v) => {
                                    if (v > state.repeatedCardIndex + 1) return;
                                    updateState((s) => (s.currentCardIndex = v));
                                }}
                                getHoverTitle={(index) => repeatCards[index].backSideText}
                                getColor={(index) => {
                                    const card = repeatCards[index];
                                    const rememberForm = state.rememberWeights[card.id];

                                    if (_.isNil(rememberForm?.weight)) {
                                        return SliderPointColor.Green;
                                    }

                                    const weight = rememberForm.weight;

                                    if (weight < 0.4) {
                                        return SliderPointColor.Red;
                                    }

                                    if (weight >= 0.4 && weight < 0.8) {
                                        return SliderPointColor.Yellow;
                                    }

                                    return SliderPointColor.Green;
                                }}
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
