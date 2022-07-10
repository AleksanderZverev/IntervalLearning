import { FC, useState } from 'react';
import { PageContainer } from '../../../controls/PageContainer/PageContainer';
import { PageHeader } from '../../../controls/PageHeader/PageHeader';
import { ToggleButton, ToggleButtonGroup } from '@mui/material';
import { InProgressCollections } from './InProgressCollections/InProgressCollections';
import { CanStartCollections } from './CanStartCollections/CanStartCollections';
import Head from 'next/head';

export const LearningPage: FC = () => {
    const [isInProcess, setIsInProcess] = useState(true);

    return (
        <>
            <Head>
                <title>📖 Изучение</title>
            </Head>
            <PageContainer>
                <PageHeader
                    title="Изучение"
                    subMenu={
                        <ToggleButtonGroup
                            color="primary"
                            value={isInProcess}
                            onChange={(e, v: boolean) => {
                                setIsInProcess(v ?? false);
                            }}
                            exclusive
                        >
                            <ToggleButton value={true}>Повторить</ToggleButton>
                            <ToggleButton value={false}>Изучить</ToggleButton>
                        </ToggleButtonGroup>
                    }
                />
                <div>
                    {isInProcess && <InProgressCollections />}
                    {!isInProcess && <CanStartCollections />}
                </div>
            </PageContainer>
        </>
    );
};
