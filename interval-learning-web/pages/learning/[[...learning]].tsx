import React, { FC } from 'react';
import { Route, Routes } from 'react-router-dom';
import { RepeatCollection } from '../../src/pages/learning/RepeatCollectionPage/RepeatCollectionPage';
import { LearningPage } from '../../src/pages/learning/LearningPage/LearningPage';

const LearningPageRouter: FC = () => {
    return (
        <Routes>
            <Route path="/learning" element={<LearningPage />} />
            <Route path="/learning/repeat/:userId-:collectionId" element={<RepeatCollection />} />
        </Routes>
    );
};

export default LearningPageRouter;
