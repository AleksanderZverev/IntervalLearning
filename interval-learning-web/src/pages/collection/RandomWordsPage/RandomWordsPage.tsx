import { Button, Stack, Typography } from '@mui/material';
import { FC, useState } from 'react';
import { useParams } from 'react-router-dom';
import { CenterContainer } from '../../../controls/CenterContainer/CenterContainer';
import { HintButton } from '../../../controls/HintButton/HintButton';
import { CreateCardModal } from '../../../controls/Modals/CreateCardModal';
import { PageContainer } from '../../../controls/PageContainer/PageContainer';
import { PageHeader } from '../../../controls/PageHeader/PageHeader';
import { PaperCard } from '../../../controls/PaperCard/PaperCard';
import { withOtherQueryResolver, withQueryResolver, WithQueryResolverData } from '../../../hoc/withQueryResolver';
import { useEventListener } from '../../../hooks/useEventListener';
import useTypedSelector from '../../../hooks/useTypedSelector';
import { useGetCollectionQuery, useGetRandomWordsQuery } from '../../../redux/collectionApi';
import { selectCollectionById } from '../../../redux/slices/collectionsSlice';
import { selectTheme } from '../../../redux/slices/themeSlice';
import styles from './styles.module.css';

interface RandomWordsPageContentProps extends WithQueryResolverData<typeof useGetRandomWordsQuery> {
    userId: string;
    collectionId: string;
    forceRefetch: () => void;
}

const RandomWordsPageContent: FC<RandomWordsPageContentProps> = ({
    queryData: { language, words: randomWords },
    userId,
    collectionId,
    forceRefetch,
}) => {
    const collection = useTypedSelector((state) => selectCollectionById(state, userId, collectionId));

    if (!collection) {
        throw new Error();
    }

    const theme = useTypedSelector((state) => selectTheme(state, collection.themeId));

    if (!theme) {
        throw new Error();
    }

    const [addedWordsCount, setAddedWordsCount] = useState(0);
    const [currentWordIndex, setCurrentWordIndex] = useState(0);
    const [showAddCardModal, setShowAddCardModal] = useState(false);

    const currentWord = randomWords[currentWordIndex];

    const onAdd = () => {
        setShowAddCardModal(true);
    };

    const onNext = () => {
        if (currentWordIndex + 1 < randomWords.length) {
            setCurrentWordIndex(currentWordIndex + 1);
            return;
        }

        forceRefetch();
    };

    useEventListener('keypress', (e) => {
        e.key === ' ' && onNext();
        e.key === 'Enter' && onAdd();
    });

    if (!randomWords || randomWords.length === 0) {
        return (
            <PageContainer transparent>
                <PageHeader title={collection.title} subTitle={theme.name} />
                <CenterContainer>Все слова изучены</CenterContainer>
            </PageContainer>
        );
    }

    return (
        <PageContainer transparent>
            <PageHeader title={collection.title} subTitle={theme.name} />
            <div style={{ position: 'relative', marginTop: '5px' }}>
                <div style={{ position: 'absolute', top: 0, left: 0 }}>
                    <div style={{ color: '#b7b7b7' }}>Добавлено: {addedWordsCount}</div>
                    <div style={{ color: '#b7b7b7' }}>Всего: {collection.cardsCount}</div>
                </div>
                <CenterContainer>
                    {showAddCardModal && (
                        <CreateCardModal
                            open
                            onClose={() => {
                                setShowAddCardModal(false);
                            }}
                            onAdded={() => {
                                setShowAddCardModal(false);
                                setAddedWordsCount(addedWordsCount + 1);
                                onNext();
                            }}
                            collectionUserId={userId}
                            collectionId={collectionId}
                            defaultFrontText={currentWord.word}
                            defaultPromptText={currentWord.pronunciation ?? undefined}
                        />
                    )}
                    <PaperCard
                        leftButton={
                            <HintButton
                                hint="enter"
                                hintPosition="bottom left"
                                hintSpace
                                variant="contained"
                                onClick={onAdd}
                            >
                                Добавить
                            </HintButton>
                        }
                        rightButton={
                            <HintButton
                                hint="space"
                                hintPosition="bottom right"
                                hintSpace
                                variant="outlined"
                                onClick={onNext}
                            >
                                Пропустить
                            </HintButton>
                        }
                        justifyButtons="center"
                    >
                        <Stack direction={'column'} gap="6px" alignItems={'center'} style={{ marginBottom: '20px' }}>
                            <Typography variant="h3" fontSize={32}>
                                {currentWord.word}
                            </Typography>
                            {currentWord.pronunciation}
                        </Stack>
                    </PaperCard>
                </CenterContainer>
            </div>
        </PageContainer>
    );
};

const RandomWordsPageConnected = withQueryResolver(useGetRandomWordsQuery)(RandomWordsPageContent);
const RandomWordsPageConnected2 = withOtherQueryResolver(useGetCollectionQuery)(RandomWordsPageConnected);

export const RandomWordsPage: FC = () => {
    const { userId, collectionId } = useParams();

    if (!collectionId || !userId) {
        throw new Error();
    }

    const [refetchToggle, setRefetchToggle] = useState(1);

    const toggle = () => setRefetchToggle(refetchToggle + 1);

    return (
        <RandomWordsPageConnected2 queryArg={{ collectionId, refetchToggle }} userId={userId} forceRefetch={toggle} />
    );
};
