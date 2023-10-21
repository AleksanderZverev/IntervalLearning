import { FC, useState } from 'react';
import { PageContainer } from '../../../controls/PageContainer/PageContainer';
import { PageHeader } from '../../../controls/PageHeader/PageHeader';
import { ToggleButton, ToggleButtonGroup } from '@mui/material';
import { InProgressCollections } from './InProgressCollections/InProgressCollections';
import { CanStartCollections } from './CanStartCollections/CanStartCollections';
import Head from 'next/head';
import { CalendarStatisticPage } from './CalendarStatistic/CalendarStatistic';

enum PageType {
    Learning = 1,
    Repeating = 2,
    Statistic = 3,
}

export const LearningPage: FC = () => {
    const [page, setPage] = useState(PageType.Repeating);

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
                            value={page}
                            onChange={(e, v: PageType | null | undefined) => {
                                if (v === undefined || v === null) return;
                                setPage(v);
                            }}
                            exclusive
                        >
                            <ToggleButton value={PageType.Repeating}>Повторить</ToggleButton>
                            <ToggleButton value={PageType.Learning}>Изучить</ToggleButton>
                            <ToggleButton value={PageType.Statistic}>Статистика</ToggleButton>
                        </ToggleButtonGroup>
                    }
                />
                <div>
                    {page === PageType.Repeating && <InProgressCollections />}
                    {page === PageType.Learning && <CanStartCollections />}
                    {page === PageType.Statistic && <CalendarStatisticPage />}
                </div>
            </PageContainer>
        </>
    );
};
