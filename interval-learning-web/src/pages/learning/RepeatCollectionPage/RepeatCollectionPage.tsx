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
    mutationProps: { mutate: rememberCards, data, showRetryModal, isLoading, isSuccess },
}) => {
    const collection = useTypedSelector((state) => selectCollectionById(state, userId, collectionId));
    const schedule = useTypedSelector((state) => selectScheduleById(state, getScheduleId(scheduleUserId, scheduleId)));

    if (!collection || !schedule) {
        throw new Error();
    }

    const navigate = useNavigate();
    const theme = useTypedSelector((state) => selectTheme(state, collection.themeId));
    const repeatCards = useTypedSelector((state) => selectCardsByIds(state, userId, collectionId, cardIds));
    const [showAssertionModal, setShowAssertionModal] = useState(false);
    const [showCurrentCardError, setShowCurrentCardError] = useState(false);
    const [showStartModal, setShowStartModal] = useState(true);

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

    const onExit = () => {
        navigate('/learning');
    };

    const onFinish = async (fromAssertionModal: boolean) => {
        if (isLoading || isSuccess) {
            return;
        }

        const resultWeights = Object.entries(rememberWeights);

        if (resultWeights.some(([_, weight]) => weight === undefined || weight === null)) {
            console.error('remember weights are incorrect');
            return;
        }

        if (!rememberWeights[card.id]) {
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

        LocalStorageHelper.saveRepeatingCardsWeights(
            schedule.userId,
            schedule.id,
            phaseIndex,
            collection.id,
            date,
            rememberWeights
        );
    };

    const onNext = () => {
        setCardIndex(cardIndex + 1);
    };

    const onPrevious = () => {
        setCardIndex(cardIndex - 1);
    };

    const isEmptyCollection = repeatCards.length === 0;

    const sortedPhases = [...schedule.phases].sort((f, s) => f.id.localeCompare(s.id));
    const phase = sortedPhases[phaseIndex];

    return (
        <PageContainer transparent>
            <PageHeader
                title={collection?.title || ''}
                subTitle={theme?.name || ''}
                subMenu={
                    !isEmptyCollection && (
                        <Button variant="outlined" onClick={() => (isSuccess ? onExit() : onFinish(false))}>
                            Завершить
                        </Button>
                    )
                }
            />
            {showStartModal && phase && phase.description && (
                <AssertionModal
                    open
                    title="Учебный план"
                    message={phase.description}
                    assertTitle="OK"
                    onClose={() => setShowStartModal(false)}
                    forbidShowingKey={`${scheduleUserId}-${scheduleId}`}
                />
            )}
            {showCurrentCardError && (
                <AssertionModal
                    open
                    title="Значение не выбрано"
                    message="Выберите значение текущей карточки"
                    assertTitle="OK"
                    onClose={() => setShowCurrentCardError(false)}
                />
            )}
            {showAssertionModal && (
                <AssertionModal
                    open
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
            <div style={{ marginTop: 10, cursor: 'pointer', color: '#b7b7b7' }}>
                {phase && phase.description && (
                    <LightTooltip
                        placement="bottom-start"
                        title={
                            <div style={{ padding: 5, fontSize: 18, fontWeight: 'normal' }}>{phase.description}</div>
                        }
                    >
                        <Stack direction={'row'} alignItems={'center'} columnGap={'5px'}>
                            <HelpOutline />
                            <span>Спустя {dayjs.duration(phase.secondsFromLastPhase, 's').humanize()}</span>
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
                            width: 650,
                            display: 'flex',
                            flexDirection: 'column',
                            rowGap: 35,
                        }}
                    >
                        {isSuccess && data && (
                            <CardResult
                                wordsLearned={notActiveIndex > maxCards ? maxCards : notActiveIndex}
                                nextRepeatDate={data.nextRepeatDate}
                                onEndButtonClick={onExit}
                            />
                        )}
                        {!isSuccess && currentCard && (
                            <RepeatCard
                                value={rememberWeights[card.id] ?? null}
                                card={currentCard}
                                showNext={cardIndex < maxCards - 1}
                                showPrevious={cardIndex !== 0}
                                isActive={notActiveIndex - 1 === cardIndex}
                                onFinish={() => onFinish(false)}
                                onNext={onNext}
                                onChange={onChange}
                                onPrevious={onPrevious}
                                forceShowError={showCurrentCardError}
                                errorMessage={'Помните слово?'}
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
                            getHoverTitle={(index) => repeatCards[index].frontSideText}
                        />
                    </div>
                )}
            </CenterContainer>
        </PageContainer>
    );
};

const ConnectedRepeatCollectionPage = withQueryResolver(useGetRepeatCardsQuery)(RepeatCollectionPageContent);
const CollectionQueryResolver = withOtherQueryResolver(useGetCollectionQuery)(ConnectedRepeatCollectionPage);
const ConnectedMutationResolver = withMutationResolver(
    usePatchRememberCardsMutation,
    'Не удалось завершить повторение'
)(CollectionQueryResolver);

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
        phaseIndexString === undefined ||
        phaseIndexString == null ||
        parseInt(phaseIndexString) < 0
    ) {
        return <div>Incorrect link</div>;
    }

    const phaseIndex = parseInt(phaseIndexString);

    return (
        <ConnectedMutationResolver
            queryArg={{ userId, collectionId, request: { scheduleUserId, scheduleId, phaseIndex } }}
            disableLoading={skipLoading}
            scheduleUserId={scheduleUserId}
            scheduleId={scheduleId}
            phaseIndex={phaseIndex}
            date={date}
            setSkipLoading={(skip) => setSkipLoading(skip)}
        />
    );
};
