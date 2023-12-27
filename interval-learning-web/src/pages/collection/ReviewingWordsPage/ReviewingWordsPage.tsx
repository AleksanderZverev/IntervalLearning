import { FC, useState } from 'react';
import { PageContent } from '../../../controls/PageContent/PageContent';
import { PageContainer } from '../../../controls/PageContainer/PageContainer';
import { PageHeader } from '../../../controls/PageHeader/PageHeader';
import useTypedSelector from '../../../hooks/useTypedSelector';
import { selectCollectionById } from '../../../redux/slices/collectionsSlice';
import { selectTheme } from '../../../redux/slices/themeSlice';
import { useParams } from 'react-router-dom';
import { useGetCardsQuery } from '../../../redux/cardsApi';
import { CenterContainer } from '../../../controls/CenterContainer/CenterContainer';
import { PaperCard } from '../../../controls/PaperCard/PaperCard';
import { Button, CircularProgress, IconButton, Portal, Stack, Typography } from '@mui/material';
import { InfoOutlined, KeyboardReturn } from '@mui/icons-material';
import { HintButton } from '../../../controls/HintButton/HintButton';
import { HidableText } from '../../../controls/HidableText/HidableText';
import { ShowCardModal } from '../../../controls/Modals/ShowCardModal';

interface ReviewingWordsPageContentProps {
    userId: string;
    collectionId: string;
}

interface State {
    page: number;
    cardIndex: number;
    isFinished: boolean;
}

const ReviewingWordsPageContent: FC<ReviewingWordsPageContentProps> = ({ userId, collectionId }) => {
    const collection = useTypedSelector((state) => selectCollectionById(state, userId, collectionId));

    if (!collection) {
        throw new Error();
    }

    const theme = useTypedSelector((state) => selectTheme(state, collection.themeId));

    if (!theme) {
        throw new Error();
    }

    const [state, setState] = useState<State>({ page: 1, cardIndex: 0, isFinished: false });
    const [showCardInfoModal, setShowCardInfoModal] = useState(false);

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

    const BigPaperCardText = (text: string) => {
        return (
            <PageContainer transparent>
                <PageHeader title={collection.title} subTitle={theme.name} />
                <div>
                    <CenterContainer>
                        <PaperCard>
                            <Typography variant="h3" fontSize={32}>
                                {text}
                            </Typography>
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
        return BigPaperCardText("You've repeated all cards");
    }

    if (cards.length == 0) {
        return BigPaperCardText('No words in the collection');
    }

    const onBack = () => {
        if (state.isFinished) return;

        const nextIndex = state.cardIndex - 1;

        if (nextIndex >= 0) {
            setState({ ...state, cardIndex: nextIndex });
            return;
        }
    };

    const onNext = () => {
        if (state.isFinished) return;

        const nextIndex = state.cardIndex + 1;

        if (nextIndex < cards.length) {
            setState({ ...state, cardIndex: nextIndex });
            return;
        }

        if (!isLastPage) {
            setState({ ...state, cardIndex: 0, page: state.page });
            return;
        }

        if (isLastPage) {
            setState({ ...state, isFinished: true });
            return;
        }
    };

    const onLearnWord = async () => {
        // TODO: API CALL

        //
        onNext();
    };

    const currentCard = cards[state.cardIndex];

    return (
        <PageContainer transparent>
            <PageHeader title={collection.title} subTitle={theme.name} />
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
                            <Button variant="outlined" onClick={onLearnWord}>
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
                                {currentCard.backSideText}
                            </Typography>
                            <HidableText size="small" text={currentCard.frontSideText} />
                        </Stack>
                    </PaperCard>
                </CenterContainer>
            </div>
        </PageContainer>
    );
};

export const ReviewingWordsPage: FC = () => {
    const { userId, collectionId } = useParams();

    if (!collectionId || !userId) {
        throw new Error();
    }

    return <ReviewingWordsPageContent userId={userId} collectionId={collectionId} />;
};
