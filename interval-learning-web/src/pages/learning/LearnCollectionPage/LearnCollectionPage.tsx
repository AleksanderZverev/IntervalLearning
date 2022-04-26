import { FC, useState } from 'react';
import useTypedSelector from '../../../hooks/useTypedSelector';
import { selectNotStartedCardsIds } from '../../../redux/slices/notStartedCardsSlice';
import { useNavigate, useParams } from 'react-router-dom';
import { selectCollectionById } from '../../../redux/slices/collectionsSlice';
import { withOtherQueryResolver, withQueryResolver } from '../../../hoc/withQueryResolver';
import { CardsItem, useGetNotStartedCardsQuery, useStartCardsMutation } from '../../../redux/cardsApi';
import { PageContainer } from '../../../controls/PageContainer/PageContainer';
import { PageHeader } from '../../../controls/PageHeader/PageHeader';
import { selectTheme } from '../../../redux/slices/themeSlice';
import { CenterContainer } from '../../../controls/CenterContainer/CenterContainer';
import { Slider } from '../../../controls/Slider/Slider';
import { Button } from '@mui/material';
import { LearnCard } from './LearnCard/LearnCard';
import { ErrorModal } from '../../../controls/Modals/ErrorModal';
import { useGetCollectionQuery } from '../../../redux/collectionApi';
import { AssertionModal } from '../../../controls/Modals/AssertionModal';
import { CardResult } from '../CardResult/CardResult';

interface LearnCollectionPageContentProps {
    userId: string;
    collectionId: string;
    setDisableLoading: (disable: boolean) => void;
}

export const LearnCollectionPageContent: FC<LearnCollectionPageContentProps> = ({
    userId,
    collectionId,
    setDisableLoading,
}) => {
    const collection = useTypedSelector((state) => selectCollectionById(state, userId, collectionId));

    if (collection === undefined) {
        throw new Error();
    }

    const theme = useTypedSelector((state) => selectTheme(state, collection.themeId));
    const notStartedCards = useTypedSelector(selectNotStartedCardsIds);

    const navigate = useNavigate();
    const [startCards, { data, isLoading: isMutationLoading, isSuccess }] = useStartCardsMutation();
    const [showAssetModal, setShowAssertModal] = useState(false);
    const [showErrorModal, setShowErrorModal] = useState(false);
    const [activeCardIndex, setActiveCardIndex] = useState(0);
    const [cardIndex, setCardIndex] = useState(0);

    if (notStartedCards.length === 0) {
        return <div>No cards</div>;
    }

    const maxCards = notStartedCards.length;

    const currentCard = notStartedCards[cardIndex];

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

        const cardsToStart = [...notStartedCards];

        if (cardIndex + 1 < maxCards) {
            cardsToStart.splice(cardIndex + 1);
        }

        const item: CardsItem = {
            cardIds: cardsToStart.map((i) => i.id),
        };

        try {
            setDisableLoading(true);
            await startCards({ userId, collectionId, request: item }).unwrap();
            setActiveCardIndex(activeCardIndex + 1);
        } catch {
            setDisableLoading(false);
            setShowErrorModal(true);
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
            {showErrorModal && (
                <ErrorModal
                    errorMessage="Не удалось завершить изучение коллекции"
                    open
                    onClose={() => setShowErrorModal(false)}
                    onRetry={() => onFinish(true)}
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
                        margin: '20px 0',
                        display: 'flex',
                        alignItems: 'center',
                        columnGap: 25,
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
                        vertical
                    />

                    {isSuccess && data && (
                        <CardResult
                            nextRepeatDate={data.nextRepeatDate}
                            wordsLearned={cardIndex + 1}
                            onEndButtonClick={onSuccessFinish}
                        />
                    )}

                    {currentCard && !isSuccess && (
                        <LearnCard
                            card={currentCard}
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

interface CardResult {
    nextRepeatDate: string | null;
    learnedCardsCount: number;
}

export const LearnCollection: FC = () => {
    const { userId, collectionId } = useParams();

    if (!collectionId || !userId) {
        throw new Error();
    }

    const [disableLoading, setDisableLoading] = useState(false);

    return (
        <ConnectedOtherResolver
            queryArg={{ userId, collectionId, request: undefined }}
            disableLoading={disableLoading}
            setDisableLoading={(disable) => setDisableLoading(disable)}
        />
    );
};
