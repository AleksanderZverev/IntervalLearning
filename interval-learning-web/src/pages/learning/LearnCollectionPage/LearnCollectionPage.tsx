import { FC, useState } from 'react';
import useTypedSelector from '../../../hooks/useTypedSelector';
import { selectNotStartedCardsIds } from '../../../redux/slices/notStartedCardsSlice';
import { useNavigate, useParams } from 'react-router-dom';
import { selectCollectionById } from '../../../redux/slices/collectionsSlice';
import { withQueryResolver } from '../../../hoc/withQueryResolver';
import { CardsItem, useGetNotStartedCardsQuery, useStartCardsMutation } from '../../../redux/cardsApi';
import { PageContainer } from '../../../controls/PageContainer/PageContainer';
import { PageHeader } from '../../../controls/PageHeader/PageHeader';
import { selectTheme } from '../../../redux/slices/themeSlice';
import { CenterContainer } from '../../../controls/CenterContainer/CenterContainer';
import { Slider } from '../../../controls/Slider/Slider';
import { Button } from '@mui/material';
import { LearnCard } from './LearnCard/LearnCard';
import { ErrorModal } from '../../../controls/Modals/ErrorModal';

interface LearnCollectionPageContentProps {}

export const LearnCollectionPageContent: FC<LearnCollectionPageContentProps> = () => {
    const { userId, collectionId } = useParams();

    if (!collectionId || !userId) {
        throw new Error();
    }

    const collection = useTypedSelector((state) => selectCollectionById(state, userId, collectionId));

    if (collection === undefined) {
        throw new Error();
    }

    const theme = useTypedSelector((state) => selectTheme(state, collection.themeId));
    const notStartedCards = useTypedSelector(selectNotStartedCardsIds);

    if (notStartedCards.length === 0) {
        throw new Error();
    }

    const navigate = useNavigate();
    const [startCards, { isLoading, isSuccess }] = useStartCardsMutation();
    const [showErrorModal, setShowErrorModal] = useState(false);
    const [cardIndex, setCardIndex] = useState(0);
    const maxCards = notStartedCards.length;

    const currentCard = notStartedCards[cardIndex];

    const onFinish = async () => {
        if (isLoading || isSuccess) {
            return;
        }

        const item: CardsItem = {
            cardIds: notStartedCards.map((i) => i.id),
        };

        try {
            await startCards({ userId, collectionId, request: item }).unwrap();
            navigate('/learning');
        } catch {
            setShowErrorModal(true);
        }
    };

    const onNext = () => {
        setCardIndex(cardIndex + 1);
    };

    const onPrevious = () => {
        setCardIndex(cardIndex - 1);
    };

    return (
        <PageContainer transparent>
            <PageHeader
                title={collection?.title || ''}
                subTitle={theme?.name || ''}
                subMenu={
                    <Button variant="outlined" onClick={onFinish}>
                        Завершить
                    </Button>
                }
            />
            {showErrorModal && (
                <ErrorModal
                    errorMessage="Не удалось завершить изучение коллекции"
                    open
                    onClose={() => setShowErrorModal(false)}
                    onRetry={onFinish}
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
                        activeValue={-1}
                        onValueChange={(v) => setCardIndex(v)}
                        vertical
                    />

                    {currentCard && (
                        <LearnCard
                            card={currentCard}
                            showNext={cardIndex < maxCards - 1}
                            showPrevious={cardIndex !== 0}
                            onFinish={onFinish}
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

export const LearnCollection: FC = () => {
    const { userId, collectionId } = useParams();

    if (!collectionId || !userId) {
        throw new Error();
    }

    return <ConnectedLearnCollectionPage queryArg={{ userId, collectionId, request: undefined }} />;
};
