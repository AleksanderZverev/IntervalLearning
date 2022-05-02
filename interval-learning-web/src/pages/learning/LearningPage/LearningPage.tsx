import { FC, useState } from 'react';
import { PageContainer } from '../../../controls/PageContainer/PageContainer';
import { PageHeader } from '../../../controls/PageHeader/PageHeader';
import { SelectSchedule } from '../../../controls/SelectSchedule/SelectSchedule';
import { ToggleButton, ToggleButtonGroup } from '@mui/material';
import { InProgressCollections } from './InProgressCollections/InProgressCollections';
import { CanStartCollections } from './CanStartCollections/CanStartCollections';
import { Schedule } from '../../../types/schedule';
import { AssertionModal } from '../../../controls/Modals/AssertionModal';

export const LearningPage: FC = () => {
    const [isInProcess, setIsInProcess] = useState(true);
    // const [schedule, setSchedule] = useState<Schedule | undefined>();

    return (
        <PageContainer>
            <PageHeader
                title="Изучение"
                subMenu={
                    <>
                        {/* <SelectSchedule
                            scheduleUserId={schedule?.userId}
                            scheduleId={schedule?.id}
                            onChange={(s) => setSchedule(s)}
                        /> */}
                        <ToggleButtonGroup
                            color="primary"
                            value={isInProcess}
                            onChange={(e, v: boolean) => setIsInProcess(v)}
                            exclusive
                        >
                            <ToggleButton value={true}>В процессе</ToggleButton>
                            <ToggleButton value={false}>Начать</ToggleButton>
                        </ToggleButtonGroup>
                    </>
                }
            />
            {/* {!isInProcess && !schedule && (
                <AssertionModal
                    open
                    title="Внимание"
                    message="Выберите учебный план"
                    onClose={() => setIsInProcess(true)}
                    assertTitle="OK"
                />
            )} */}

            {isInProcess && <InProgressCollections />}

            {!isInProcess && <CanStartCollections />}
        </PageContainer>
    );
};
