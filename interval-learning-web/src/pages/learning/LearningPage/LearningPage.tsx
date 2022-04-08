import { FC, useState } from 'react';
import { PageContainer } from '../../../controls/PageContainer/PageContainer';
import { PageHeader } from '../../../controls/PageHeader/PageHeader';
import { Button, ToggleButton, ToggleButtonGroup } from '@mui/material';
import { PlayCircle } from '@mui/icons-material';
import { withQueryResolver } from '../../../hoc/withQueryResolver';
import { useGetLearningCollectionsQuery } from '../../../redux/api/learningApi';
import useTypedSelector from '../../../hooks/useTypedSelector';
import { selectCollections } from '../../../redux/slices/collectionsSlice';
import { selectQueueCards } from '../../../redux/slices/queueLearnSlice';

const LearningPageContent: FC = () => {
    const [isInProcess, setIsInProcess] = useState(true);
    const queueCards = useTypedSelector(selectQueueCards);

    return (
        <PageContainer>
            <PageHeader
                title="Изучение"
                subMenu={
                    <ToggleButtonGroup
                        color="primary"
                        value={isInProcess}
                        onChange={(e, v: boolean) => setIsInProcess(v)}
                        exclusive
                    >
                        <ToggleButton value={true}>В процессе</ToggleButton>
                        <ToggleButton value={false}>Неначатые</ToggleButton>
                    </ToggleButtonGroup>
                }
            />
            {queueCards.map((c) => (
                <div key={c.repeatDate.toLocaleString() + c.card.id.toString()}>
                    {c.card.frontSideText}-{c.card.backSideText}-{c.repeatDate.toLocaleDateString()}
                </div>
            ))}
        </PageContainer>
    );
};

export const LearningPage = withQueryResolver(useGetLearningCollectionsQuery)(LearningPageContent);
