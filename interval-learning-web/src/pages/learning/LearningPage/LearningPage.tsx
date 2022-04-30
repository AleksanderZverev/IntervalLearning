import { FC, useState } from 'react';
import { PageContainer } from '../../../controls/PageContainer/PageContainer';
import { PageHeader } from '../../../controls/PageHeader/PageHeader';
import { ToggleButton, ToggleButtonGroup } from '@mui/material';
import { InProgressCollections } from './InProgressCollections/InProgressCollections';
import { CanStartCollections } from './CanStartCollections/CanStartCollections';

export const LearningPage: FC = () => {
    const [isInProcess, setIsInProcess] = useState(true);

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
                        <ToggleButton value={false}>Начать</ToggleButton>
                    </ToggleButtonGroup>
                }
            />
            {isInProcess ? <InProgressCollections /> : <CanStartCollections />}
        </PageContainer>
    );
};
