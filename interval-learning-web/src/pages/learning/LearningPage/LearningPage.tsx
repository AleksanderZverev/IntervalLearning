import { FC, useState } from 'react';
import { PageContainer } from '../../../controls/PageContainer/PageContainer';
import { PageHeader } from '../../../controls/PageHeader/PageHeader';
import { Button, ToggleButton, ToggleButtonGroup } from '@mui/material';
import { PlayCircle } from '@mui/icons-material';

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
                        <ToggleButton value={false}>Неначатые</ToggleButton>
                    </ToggleButtonGroup>
                }
            />
        </PageContainer>
    );
};
