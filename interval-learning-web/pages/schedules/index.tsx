import { Button } from '@mui/material';
import { FC, useState } from 'react';
import { CreateScheduleModal } from '../../src/controls/Modals/CreateScheduleModal';
import { PageContainer } from '../../src/controls/PageContainer/PageContainer';
import { withQueryResolver } from '../../src/hoc/withQueryResolver';
import { useGetSchedulesQuery } from '../../src/redux/schedulesSlice';
import { Schedule } from '../../src/types/schedule';

const SchedulePage: FC<{ data: Schedule[] }> = ({ data: schedules }) => {
    const [showCreateScheduleModal, setShowCreateScheduleModal] = useState(false);
    return (
        <PageContainer>
            {showCreateScheduleModal && (
                <CreateScheduleModal open={showCreateScheduleModal} onClose={() => setShowCreateScheduleModal(false)} />
            )}
            <div>
                {schedules.map((s) => (
                    <div key={s.userId + s.id}>{s.title}</div>
                ))}
            </div>
            <Button onClick={() => setShowCreateScheduleModal(true)}>Create schedule</Button>
        </PageContainer>
    );
};

const withLoader = withQueryResolver(useGetSchedulesQuery)(SchedulePage);

export default withLoader;
