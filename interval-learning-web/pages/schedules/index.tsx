import { Button } from '@mui/material';
import { FC, useState } from 'react';
import { CreateScheduleModal } from '../../src/controls/Modals/CreateScheduleModal';
import { PageContainer } from '../../src/controls/PageContainer/PageContainer';
import { withQueryResolver } from '../../src/hoc/withQueryResolver';
import useTypedSelector from '../../src/hooks/useTypedSelector';
import { useGetSchedulesQuery } from '../../src/redux/schedulesSlice';
import { selectSchedules } from '../../src/redux/slices/scheduleSlice';

const SchedulePage: FC = () => {
    const [showCreateScheduleModal, setShowCreateScheduleModal] = useState(false);
    const schedules = useTypedSelector(selectSchedules);

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
