import React, { FC } from 'react';
import { Route, Routes } from 'react-router-dom';
import { RepeatCollection } from '../../src/pages/learning/RepeatCollectionPage/RepeatCollectionPage';
import { LearningPage } from '../../src/pages/learning/LearningPage/LearningPage';
import { LearnCollection } from '../../src/pages/learning/LearnCollectionPage/LearnCollectionPage';
import { NextComponentProps } from '../_app';

const LearningPageRouter: FC<NextComponentProps> = ({ isServerSide }) => {
    return (
        <Routes>
            <Route path="/learning" element={<LearningPage />} />
            <Route path="/learning/learn/:userId-:collectionId" element={<LearnCollection />} />
            <Route path="/learning/repeat/:userId-:collectionId" element={<RepeatCollection />} />
        </Routes>
    );
};

export default LearningPageRouter;
