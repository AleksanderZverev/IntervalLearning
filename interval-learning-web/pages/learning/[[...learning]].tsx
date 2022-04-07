import React, { FC } from 'react';
import { Route, Routes } from 'react-router-dom';
import { LearningPage } from '../../src/pages/learning/LearningPage/LearningPage';

const LearningPageRouter: FC = () => {
    return (
        <Routes>
            <Route path="/learning" element={<LearningPage />} />
        </Routes>
    );
};

export default LearningPageRouter;
