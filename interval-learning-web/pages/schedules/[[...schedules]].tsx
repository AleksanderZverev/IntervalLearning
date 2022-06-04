import { FC } from 'react';
import { Route, Routes } from 'react-router-dom';
import { ScheduleCreatePage } from '../../src/pages/schedules/ScheduleCreatePage/ScheduleCreatePage';
import { ScheduleEditPage } from '../../src/pages/schedules/ScheduleEditPage/ScheduleEditPage';
import { ScheduleListPage } from '../../src/pages/schedules/ScheduleListPage/ScheduleListPage';

const SchedulePageRouter: FC = () => {
    return (
        <Routes>
            <Route path="/schedules" element={<ScheduleListPage />} />
            <Route path="/schedules/new" element={<ScheduleCreatePage />} />
            <Route path="/schedules/:scheduleId/edit" element={<ScheduleEditPage />} />
        </Routes>
    );
};

export default SchedulePageRouter;
