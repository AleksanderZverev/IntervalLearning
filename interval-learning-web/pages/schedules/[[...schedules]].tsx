import { FC } from 'react';
import { Route, Routes } from 'react-router-dom';
import { ScheduleListPage } from '../../src/pages/schedules/ScheduleListPage/ScheduleListPage';

const SchedulePageRouter: FC = () => {
    return (
        <Routes>
            <Route path="/schedules" element={<ScheduleListPage />} />
        </Routes>
    );
};

export default SchedulePageRouter;
