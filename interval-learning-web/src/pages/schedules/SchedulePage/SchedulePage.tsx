import { Button } from '@mui/material';
import { FC, useState } from 'react';
import { CreateScheduleModal } from '../../../controls/Modals/CreateScheduleModal';
import { PageContainer } from '../../../controls/PageContainer/PageContainer';
import { withQueryResolver } from '../../../hoc/withQueryResolver';
import useTypedSelector from '../../../hooks/useTypedSelector';
import { useGetSchedulesQuery } from '../../../redux/schedulesSlice';
import { selectSchedules } from '../../../redux/slices/scheduleSlice';

const SchedulePageContent: FC = () => {
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

const withLoader = withQueryResolver(useGetSchedulesQuery)(SchedulePageContent);

export const SchedulePage = withLoader;
