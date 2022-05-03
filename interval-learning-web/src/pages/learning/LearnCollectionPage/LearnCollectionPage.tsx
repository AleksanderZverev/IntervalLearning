import { FC, useState } from 'react';
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
import { CenterContainer } from '../../../controls/CenterContainer/CenterContainer';
import { Slider } from '../../../controls/Slider/Slider';
import { Button, Stack } from '@mui/material';
import { LearnCard } from './LearnCard/LearnCard';
import { useGetCollectionQuery } from '../../../redux/collectionApi';
import { AssertionModal } from '../../../controls/Modals/AssertionModal';
import { CardResult } from '../CardResult/CardResult';
import { getScheduleId, selectScheduleById } from '../../../redux/slices/scheduleSlice';
import { LightTooltip } from '../../../controls/LightTooltip/LightTooltip';
import { HelpOutline } from '@mui/icons-material';
import { selectCards } from '../../../redux/slices/cardsSlice';

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
    queryData: notStartedCardIds,
    mutationProps: { mutate: startCards, showRetryModal, isLoading: isMutationLoading, isSuccess, data: mutationData },
}) => {
    const collection = useTypedSelector((state) => selectCollectionById(state, userId, collectionId));
    const schedule = useTypedSelector((state) => selectScheduleById(state, getScheduleId(scheduleUserId, scheduleId)));
    const cards = useTypedSelector((state) => selectCards(state, userId, collectionId));

    if (!collection || !schedule || !cards) {
        throw new Error();
    }

    const theme = useTypedSelector((state) => selectTheme(state, collection.themeId));

    const navigate = useNavigate();
    const [showAssetModal, setShowAssertModal] = useState(false);
    const [showStartModal, setShowStartModal] = useState(true);
    const [activeCardIndex, setActiveCardIndex] = useState(0);
    const [cardIndex, setCardIndex] = useState(0);

    if (notStartedCardIds.length === 0) {
        return <div>No cards</div>;
    }

    const maxCards = notStartedCardIds.length;

    const currentCardId = notStartedCardIds[cardIndex];

    const onSuccessFinish = () => {
        navigate('/learning');
    };

    const onFinish = async (fromAssertionModal: boolean) => {
        if (isMutationLoading || isSuccess) {
            onSuccessFinish();
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
            <div style={{ marginTop: 10, cursor: 'pointer', color: '#b7b7b7' }}>
                {schedule.description && (
                    <LightTooltip
                        placement="bottom-start"
                        title={
                            <div style={{ padding: 5, fontSize: 18, fontWeight: 'normal' }}>{schedule.description}</div>
                        }
                    >
                        <Stack direction={'row'} alignItems={'center'} columnGap={'5px'}>
                            <HelpOutline />
                            <span>Что делать?</span>
                        </Stack>
                    </LightTooltip>
                )}
            </div>
            {showStartModal && schedule && schedule.description && (
                <AssertionModal
                    open
                    title="Учебный план"
                    message={schedule.description}
                    assertTitle="OK"
                    onClose={() => setShowStartModal(false)}
                    forbidShowingKey={`${scheduleUserId}-${scheduleId}`}
                />
            )}
            {showAssetModal && (
                <AssertionModal
                    open
                    title="Не все карточки изучены"
                    message="Завершить изучение на текущей карточке?"
                    assertTitle="Да"
                    cancelTitle="Отмена"
                    onAssert={() => onFinish(true)}
                    onClose={() => setShowAssertModal(false)}
                />
            )}
            <CenterContainer>
                <div
                    style={{
                        margin: '10px 0',
                        display: 'flex',
                        alignItems: 'center',
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
                            onEndButtonClick={onSuccessFinish}
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
            </CenterContainer>
        </PageContainer>
    );
};

const ConnectedLearnCollectionPage = withQueryResolver(useGetNotStartedCardsQuery)(LearnCollectionPageContent);
const ConnectedOtherResolver = withOtherQueryResolver(useGetCollectionQuery)(ConnectedLearnCollectionPage);
const ConnectedMutationResolver = withMutationResolver(
    useStartCardsMutation,
    'Не удалось завершить изучение коллекции'
)(ConnectedOtherResolver);

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

    if (!collectionId || !userId || !scheduleUserId || !scheduleId) {
        throw new Error();
    }

    const [disableLoading, setDisableLoading] = useState(false);

    return (
        <ConnectedMutationResolver
            queryArg={{ userId, collectionId, request: { scheduleUserId, scheduleId } }}
            disableLoading={disableLoading}
            scheduleUserId={scheduleId}
            scheduleId={scheduleUserId}
            setDisableLoading={(disable) => setDisableLoading(disable)}
        />
    );
};
