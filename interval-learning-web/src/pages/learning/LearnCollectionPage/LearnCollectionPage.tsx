import { FC, PropsWithChildren, useRef, useState } from 'react';
import { FormProvider, SubmitHandler, useForm } from 'react-hook-form';
import useTypedSelector from '../../../hooks/useTypedSelector';
import { isNotStartedCardsIdsAdded, selectNotStartedCardsIds } from '../../../redux/slices/notStartedCardsSlice';
import * as yup from 'yup';
import { yupResolver } from '@hookform/resolvers/yup';
import { useParams } from 'react-router-dom';
import { selectCollectionById } from '../../../redux/slices/collectionsSlice';
import { LearnCard } from './LearnCard/LearnCard';
import { withQueryResolver } from '../../../hoc/withQueryResolver';
import { useGetNotStartedCardsQuery } from '../../../redux/cardsApi';
import { PageContainer } from '../../../controls/PageContainer/PageContainer';
import { PageHeader } from '../../../controls/PageHeader/PageHeader';
import { selectTheme } from '../../../redux/slices/themeSlice';
import { CenterContainer } from '../../../controls/CenterContainer/CenterContainer';
import { Slider } from '../../../controls/Slider/Slider';
import { Button } from '@mui/material';

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

    const maxCards = notStartedCards.length;
    const [rememberWeights, setRememberWeights] = useState<(number | null)[]>(Array(maxCards).fill(null));
    const [cardIndex, setCardIndex] = useState(0);

    let notActiveIndex = rememberWeights.indexOf(null);
    notActiveIndex = notActiveIndex < 0 ? maxCards : notActiveIndex;

    const currentCard = notStartedCards[cardIndex];

    const onFinish = () => {
        console.log('ok', rememberWeights);
    };

    const onChange = (weight: number | null) => {
        rememberWeights[cardIndex] = weight;
        setRememberWeights([...rememberWeights]);
    };

    const onNext = () => {
        const nextIndex = cardIndex + 1;
        setCardIndex(nextIndex);
    };

    const onPrevious = () => {
        setCardIndex(cardIndex - 1);
    };

    console.log('notActiveIndex', notActiveIndex, rememberWeights);

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
            <CenterContainer>
                <div
                    style={{
                        width: 650,
                        display: 'flex',
                        flexDirection: 'column',
                        rowGap: 25,
                    }}
                >
                    {currentCard && (
                        <LearnCard
                            value={rememberWeights[cardIndex]}
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
            </CenterContainer>
        </PageContainer>
    );
};

const ConnectedLearnCollectionPage = withQueryResolver(
    useGetNotStartedCardsQuery,
    (state) => !isNotStartedCardsIdsAdded(state)
)(LearnCollectionPageContent);

export const LearnCollection: FC = () => {
    const { userId, collectionId } = useParams();

    if (!collectionId || !userId) {
        throw new Error();
    }

    return <ConnectedLearnCollectionPage queryArg={{ userId, collectionId, request: undefined }} />;
};
