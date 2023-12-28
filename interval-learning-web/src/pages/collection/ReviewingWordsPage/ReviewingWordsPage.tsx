import { FC, useState } from 'react';
import { PageContent } from '../../../controls/PageContent/PageContent';
import { PageContainer } from '../../../controls/PageContainer/PageContainer';
import { PageHeader } from '../../../controls/PageHeader/PageHeader';
import useTypedSelector from '../../../hooks/useTypedSelector';
import { selectCollectionById } from '../../../redux/slices/collectionsSlice';
import { selectTheme } from '../../../redux/slices/themeSlice';
import { useNavigate, useParams } from 'react-router-dom';
import { useGetCardsQuery, useRelearnCardMutation } from '../../../redux/cardsApi';
import { CenterContainer } from '../../../controls/CenterContainer/CenterContainer';
import { PaperCard } from '../../../controls/PaperCard/PaperCard';
import { Button, CircularProgress, IconButton, Portal, Stack, Typography } from '@mui/material';
import { InfoOutlined, KeyboardReturn } from '@mui/icons-material';
import { HidableText } from '../../../controls/HidableText/HidableText';
import { ShowCardModal } from '../../../controls/Modals/ShowCardModal';
import { WithMutationResolverProps, withMutationResolver, withQueryResolver } from '../../../hoc/withQueryResolver';
import { useGetCollectionQuery } from '../../../redux/collectionApi';
import { useLocalStorageValue } from '../../../hooks/useLocalStorageValue';

interface ReviewingWordsPageContentProps extends WithMutationResolverProps<typeof useRelearnCardMutation> {
    userId: string;
    collectionId: string;
}

interface State {
    page: number;
    cardIndex: number;
    isFinished: boolean;
    cardsAdded: number;
    watchedCards: number;
}

const getDefaultState = (): State => ({
    page: 1,
    cardIndex: 0,
    isFinished: false,
    cardsAdded: 0,
    watchedCards: 0,
});

const ReviewingWordsPageContent: FC<ReviewingWordsPageContentProps> = ({
    userId,
    collectionId,
    mutationProps: { mutate: relearnCardAsync, ...relearnState },
}) => {
    const collection = useTypedSelector((state) => selectCollectionById(state, userId, collectionId));

    if (!collection) {
        throw new Error();
    }

    const theme = useTypedSelector((state) => selectTheme(state, collection.themeId));

    if (!theme) {
        throw new Error();
    }

    const navigate = useNavigate();

    const [state, setState] = useLocalStorageValue<State>(
        `ReviewingWordsPageContent-${collectionId}`,
        getDefaultState()
    );
    const [showCardInfoModal, setShowCardInfoModal] = useState(false);

    if (!state) throw new Error();

    const count = 30;
    const totalCards = collection.cardsCount;
    const totalPages = Math.ceil(totalCards / count);
    const isLastPage = state.page + 1 > totalPages;
    const {
        isFetching,
        isError,
        data: cards,
    } = useGetCardsQuery({
        userId: userId,
        collectionId: collectionId,
        request: {
            page: state.page,
            count: count,
        },
    });

    const BigPaperCardText = (text: string, buttonText: string, onClick: () => void) => {
        return (
            <PageContainer transparent>
                <PageHeader title={collection.title} subTitle={theme.name} />
                <div>
                    <CenterContainer>
                        <PaperCard>
                            <Stack gap={'8px'} alignItems={'center'}>
                                <Typography variant="h3" fontSize={32}>
                                    {text}
                                </Typography>
                                <div>
                                    <Button variant="outlined" onClick={onClick}>
                                        {buttonText}
                                    </Button>
                                </div>
                            </Stack>
                        </PaperCard>
                    </CenterContainer>
                </div>
            </PageContainer>
        );
    };

    if (isFetching) {
        return <PageContainer>{isFetching && <CircularProgress />}</PageContainer>;
    }

    if (isError || !cards) {
        throw new Error();
    }

    if (state.isFinished) {
        return BigPaperCardText("You've repeated all cards", 'Review cards', () => {
            setState(getDefaultState());
        });
    }

    if (cards.length == 0) {
        return BigPaperCardText('No words in the collection', 'Back', () => {
            navigate(`/collections/${userId}-${collectionId}`);
        });
    }

    const onBack = () => {
        if (state.isFinished) return;

        const nextIndex = state.cardIndex - 1;

        if (nextIndex >= 0) {
            state.watchedCards--;
            setState({ ...state, cardIndex: nextIndex });
            return;
        }
    };

    const onNext = () => {
        if (state.isFinished) return;

        const nextIndex = state.cardIndex + 1;
        state.watchedCards++;

        if (nextIndex < cards.length) {
            setState({ ...state, cardIndex: nextIndex });
            return;
        }

        if (!isLastPage) {
            setState({ ...state, cardIndex: 0, page: state.page + 1 });
            return;
        }

        if (isLastPage) {
            setState({ ...state, isFinished: true });
            return;
        }
    };

    const onLearnWord = async (cardId: string) => {
        if (relearnState.isLoading) return;

        try {
            await relearnCardAsync({ userId: userId, collectionId: collectionId, request: { cardId: cardId } });
            state.cardsAdded++;
            onNext();
        } catch {
            relearnState.showRetryModal(() => onLearnWord(cardId));
        }
    };

    const currentCard = cards[state.cardIndex];

    return (
        <PageContainer transparent>
            <PageHeader
                title={collection.title}
                subTitle={theme.name}
                subMenu={
                    <Button variant="outlined" onClick={() => setState(getDefaultState())}>
                        С начала
                    </Button>
                }
            />
            <Portal>
                {showCardInfoModal && (
                    <ShowCardModal
                        open
                        onClose={() => setShowCardInfoModal(false)}
                        userId={currentCard.userId}
                        collectionId={currentCard.collectionId}
                        cardId={currentCard.id}
                    />
                )}
            </Portal>
            <div>
                <div>Added cards: {state.cardsAdded}</div>
                <div>
                    Watched cards: {state.watchedCards} / {totalCards}
                </div>
                <CenterContainer>
                    <PaperCard
                        topRightControl={
                            <IconButton onClick={() => setShowCardInfoModal(true)}>
                                <InfoOutlined />
                            </IconButton>
                        }
                        topLeftControl={
                            <IconButton onClick={onBack}>
                                <KeyboardReturn />
                            </IconButton>
                        }
                        leftButton={
                            <Button variant="outlined" onClick={() => onLearnWord(currentCard.id)}>
                                Изучить
                            </Button>
                        }
                        rightButton={
                            <Button variant="contained" onClick={onNext}>
                                Далее
                            </Button>
                        }
                        justifyButtons="center"
                    >
                        <Stack direction={'column'} gap="6px" alignItems={'center'} style={{ marginBottom: '20px' }}>
                            <Typography variant="h3" fontSize={32}>
                                {currentCard.frontSideText}
                            </Typography>
                            <HidableText size="small" text={currentCard.backSideText} />
                        </Stack>
                    </PaperCard>
                </CenterContainer>
            </div>
        </PageContainer>
    );
};

const WithRelearnMutation = withMutationResolver(
    useRelearnCardMutation,
    'Не удалось добавить карточку'
)(ReviewingWordsPageContent);
const CollectionConnected = withQueryResolver(useGetCollectionQuery)(WithRelearnMutation);

export const ReviewingWordsPage: FC = () => {
    const { userId, collectionId } = useParams();

    if (!collectionId || !userId) {
        throw new Error();
    }

    return <CollectionConnected userId={userId} queryArg={{ collectionId: collectionId }} />;
};
