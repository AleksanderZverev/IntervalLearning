import { FC, useState } from 'react';
import useTypedSelector from '../../../hooks/useTypedSelector';
import { selectNotStartedCardsIds } from '../../../redux/slices/notStartedCardsSlice';
import { useNavigate, useParams } from 'react-router-dom';
import { selectCollectionById } from '../../../redux/slices/collectionsSlice';
import { RepeatCard } from './RepeatCard/RepeatCard';
import { withOtherQueryResolver, withQueryResolver } from '../../../hoc/withQueryResolver';
import { cardsApi, useGetNotStartedCardsQuery, useStartCardsMutation } from '../../../redux/cardsApi';
import { PageContainer } from '../../../controls/PageContainer/PageContainer';
import { PageHeader } from '../../../controls/PageHeader/PageHeader';
import { selectTheme } from '../../../redux/slices/themeSlice';
import { CenterContainer } from '../../../controls/CenterContainer/CenterContainer';
import { Slider } from '../../../controls/Slider/Slider';
import { Button, Paper } from '@mui/material';
import { LocalStorageHelper } from '../../../helpers/localStorageHelper';
import { useGetCollectionQuery } from '../../../redux/collectionApi';

interface LearnCollectionPageContentProps {}

export const RepeatCollectionPageContent: FC<LearnCollectionPageContentProps> = () => {
    const { userId, collectionId } = useParams();

    if (!collectionId || !userId) {
        throw new Error();
    }

    const collection = useTypedSelector((state) => selectCollectionById(state, userId, collectionId));

    if (collection === undefined) {
        throw new Error();
    }

    const navigate = useNavigate();
    const theme = useTypedSelector((state) => selectTheme(state, collection.themeId));
    const notStartedCards = useTypedSelector(selectNotStartedCardsIds);

    const [rememberWeights, setRememberWeights] = useState<Record<string, number | undefined>>(
        () =>
            LocalStorageHelper.getLearningCards(
                collection.id,
                notStartedCards.map((c) => c.id)
            ) ?? {}
    );

    const [cardIndex, setCardIndex] = useState(0);
    const card = notStartedCards[cardIndex];
    const maxCards = notStartedCards.length;

    let notActiveIndex = notStartedCards.map((c) => rememberWeights[c.id]).indexOf(undefined);
    notActiveIndex = notActiveIndex < 0 ? maxCards : notActiveIndex;

    const currentCard = notStartedCards[cardIndex];

    const onFinish = async () => {
        console.log('ok', rememberWeights);
        // try {
        //     await startCards()
        // }
    };

    const onChange = (weight: number | undefined) => {
        rememberWeights[card.id] = weight;
        setRememberWeights({ ...rememberWeights });
        LocalStorageHelper.saveLearningCardsWeights(
            collectionId,
            notStartedCards.map((c) => c.id),
            rememberWeights
        );
    };

    const onNext = () => {
        setCardIndex(cardIndex + 1);
    };

    const onPrevious = () => {
        setCardIndex(cardIndex - 1);
    };

    console.log('notActiveIndex', notActiveIndex, rememberWeights);

    const isEmptyCollection = notStartedCards.length === 0;
    return (
        <PageContainer transparent>
            <PageHeader
                title={collection?.title || ''}
                subTitle={theme?.name || ''}
                subMenu={
                    !isEmptyCollection && (
                        <Button variant="outlined" onClick={onFinish}>
                            Завершить
                        </Button>
                    )
                }
            />
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
                            rowGap: 25,
                        }}
                    >
                        {currentCard && (
                            <RepeatCard
                                value={rememberWeights[card.id] ?? null}
                                card={currentCard}
                                showNext={cardIndex < maxCards - 1}
                                showPrevious={cardIndex !== 0}
                                isActive={notActiveIndex - 1 === cardIndex}
                                onFinish={onFinish}
                                onNext={onNext}
                                onChange={onChange}
                                onPrevious={onPrevious}
                                errorMessage={'Помните слово?'}
                            />
                        )}

                        <Slider
                            value={cardIndex}
                            min={0}
                            max={maxCards - 1}
                            activeValue={notActiveIndex}
                            onValueChange={(v) => {
                                if (v > notActiveIndex) return;
                                setCardIndex(v);
                            }}
                        />
                    </div>
                )}
            </CenterContainer>
        </PageContainer>
    );
};

const ConnectedRepeatCollectionPage = withQueryResolver(useGetNotStartedCardsQuery)(RepeatCollectionPageContent);

const CollectionQueryResolver = withOtherQueryResolver(useGetCollectionQuery)(ConnectedRepeatCollectionPage);

export const RepeatCollection: FC = () => {
    const { userId, collectionId } = useParams();

    if (!collectionId || !userId) {
        throw new Error();
    }

    return <CollectionQueryResolver queryArg={{ userId, collectionId, request: undefined }} />;
};
