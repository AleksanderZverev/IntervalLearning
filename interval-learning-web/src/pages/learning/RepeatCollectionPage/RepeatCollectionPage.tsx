import { FC, useState } from 'react';
import useTypedSelector from '../../../hooks/useTypedSelector';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { selectCollectionById } from '../../../redux/slices/collectionsSlice';
import { RepeatCard } from './RepeatCard/RepeatCard';
import { withOtherQueryResolver, withQueryResolver, WithQueryResolverData } from '../../../hoc/withQueryResolver';
import { RememberRequest, useGetRepeatCardsQuery, usePatchRememberCardsMutation } from '../../../redux/cardsApi';
import { PageContainer } from '../../../controls/PageContainer/PageContainer';
import { PageHeader } from '../../../controls/PageHeader/PageHeader';
import { selectTheme } from '../../../redux/slices/themeSlice';
import { CenterContainer } from '../../../controls/CenterContainer/CenterContainer';
import { Slider } from '../../../controls/Slider/Slider';
import { Button, Paper } from '@mui/material';
import { LocalStorageHelper } from '../../../helpers/localStorageHelper';
import { useGetCollectionQuery } from '../../../redux/collectionApi';
import { selectCardsByIds } from '../../../redux/slices/cardsSlice';

interface RepeatCollectionPageContentProps extends WithQueryResolverData<{ cardIds: string[] }> {}

export const RepeatCollectionPageContent: FC<RepeatCollectionPageContentProps> = ({ resolverData: { cardIds } }) => {
    const { userId, collectionId } = useParams();
    const params = new URLSearchParams(location.search);
    const date = params.get('date');

    if (!collectionId || !userId || !date) {
        throw new Error();
    }

    const collection = useTypedSelector((state) => selectCollectionById(state, userId, collectionId));

    if (collection === undefined) {
        throw new Error();
    }

    const navigate = useNavigate();
    const theme = useTypedSelector((state) => selectTheme(state, collection.themeId));
    const repeatCards = useTypedSelector((state) => selectCardsByIds(state, userId, collectionId, cardIds));

    const [rememberCards, { isLoading, isError, isSuccess }] = usePatchRememberCardsMutation();

    const [rememberWeights, setRememberWeights] = useState<Record<string, number | undefined>>(
        () =>
            LocalStorageHelper.getLearningCards(
                collection.id,
                repeatCards.map((c) => c.id)
            ) ?? {}
    );

    const [cardIndex, setCardIndex] = useState(0);
    const card = repeatCards[cardIndex];
    const maxCards = repeatCards.length;

    let notActiveIndex = repeatCards.map((c) => rememberWeights[c.id]).indexOf(undefined);
    notActiveIndex = notActiveIndex < 0 ? maxCards : notActiveIndex;

    const currentCard = repeatCards[cardIndex];

    const onFinish = async () => {
        if (isLoading || isSuccess) {
            return;
        }

        const resultWeights = Object.entries(rememberWeights);
        if (resultWeights.some(([_, weight]) => weight === undefined || weight === null)) {
            console.error('remember weights are incorrect');
            return;
        }

        const request: RememberRequest = {
            date,
            rememberItems: resultWeights.map(([cardId, weight]) => ({ cardId, weight: weight ?? 0 })),
        };
        try {
            await rememberCards({ userId, collectionId, request }).unwrap();
        } catch (e) {
            console.log('error remember', e);
        }
    };

    const onChange = (weight: number | undefined) => {
        rememberWeights[card.id] = weight;
        setRememberWeights({ ...rememberWeights });
        LocalStorageHelper.saveLearningCardsWeights(
            collectionId,
            repeatCards.map((c) => c.id),
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

const ConnectedRepeatCollectionPage = withQueryResolver(useGetRepeatCardsQuery)(RepeatCollectionPageContent);

const CollectionQueryResolver = withOtherQueryResolver(useGetCollectionQuery)(ConnectedRepeatCollectionPage);

export const RepeatCollection: FC = () => {
    const { userId, collectionId } = useParams();

    const location = useLocation();
    const params = new URLSearchParams(location.search);
    const date = params.get('date');

    if (!collectionId || !userId || !date) {
        return <div>INCORRECT LINK</div>;
    }

    return <CollectionQueryResolver queryArg={{ userId, collectionId, request: { date } }} />;
};
