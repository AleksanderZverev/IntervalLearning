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
import { AssertionModal } from '../../../controls/Modals/AssertionModal';
import { ErrorModal } from '../../../controls/Modals/ErrorModal';
import { CardResult } from '../CardResult/CardResult';

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
    const [showAssertionModal, setShowAssertionModal] = useState(false);
    const [showCurrentCardError, setShowCurrentCardError] = useState(false);
    const [showLoadingError, setShowLoadingError] = useState(false);

    const [rememberCards, { data, isLoading, isError, isSuccess }] = usePatchRememberCardsMutation();

    const [rememberWeights, setRememberWeights] = useState<Record<string, number | undefined>>(
        () =>
            LocalStorageHelper.getLearningCards(
                collection.id,
                repeatCards.map((c) => c.id)
            ) ?? {}
    );

    const maxCards = repeatCards.length;

    let notActiveIndex = repeatCards.map((c) => rememberWeights[c.id]).indexOf(undefined);
    notActiveIndex = notActiveIndex < 0 ? maxCards : notActiveIndex;

    const [cardIndex, setCardIndex] = useState(notActiveIndex >= maxCards ? maxCards - 1 : notActiveIndex);
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
            date,
            rememberItems: resultWeights.map(([cardId, weight]) => ({ cardId, weight: weight ?? 0 })),
        };
        try {
            await rememberCards({ userId, collectionId, request }).unwrap();
        } catch (e) {
            setShowLoadingError(true);
        }
    };

    const onChange = (weight: number | undefined) => {
        if (weight !== undefined) {
            rememberWeights[card.id] = weight;
        } else {
            delete rememberWeights[card.id];
        }

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
                        <Button variant="outlined" onClick={() => (isSuccess ? onExit() : onFinish(false))}>
                            Завершить
                        </Button>
                    )
                }
            />
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
            {showLoadingError && (
                <ErrorModal
                    open
                    errorMessage="Не удалось завершить повторение"
                    onClose={() => setShowLoadingError(false)}
                    onRetry={() => {
                        setShowLoadingError(false);
                        onFinish(true);
                    }}
                />
            )}
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
                        {isSuccess && data && (
                            <CardResult
                                wordsLearned={notActiveIndex}
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
