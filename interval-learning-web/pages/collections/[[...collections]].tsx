import React, { FC } from 'react';
import { Route, Routes } from 'react-router-dom';
import { CollectionPage } from '../../src/pages/collection/CollectionPage/CollectionPage';
import { CollectionsPage } from '../../src/pages/collection/CollectionsPage/CollectionsPage';
import { RandomWordsPage } from '../../src/pages/collection/RandomWordsPage/RandomWordsPage';
import { ReviewingWordsPage } from '../../src/pages/collection/ReviewingWordsPage/ReviewingWordsPage';

const CollectionsPageRouter: FC = () => {
    return (
        <Routes>
            <Route path="/collections" element={<CollectionsPage />} />
            <Route path="/collections/:userId-:collectionId" element={<CollectionPage />} />
            <Route path="/collections/:userId-:collectionId/words/random" element={<RandomWordsPage />} />
            <Route path="/collections/:userId-:collectionId/words/review" element={<ReviewingWordsPage />} />
        </Routes>
    );
};

export default CollectionsPageRouter;
