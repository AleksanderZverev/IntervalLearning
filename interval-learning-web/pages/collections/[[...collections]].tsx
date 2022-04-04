import React, { FC } from 'react';
import { Route, Routes } from 'react-router-dom';
import { CollectionPage } from '../../src/pages/collection/CollectionPage/CollectionPage';
import { CollectionsPage } from '../../src/pages/collection/CollectionsPage/CollectionsPage';

const CollectionsPageRouter: FC = () => {
    return (
        <Routes>
            <Route path="/collections" element={<CollectionsPage queryArg={undefined} />} />
            <Route path="/collections/:userId-:collectionId" element={<CollectionPage />} />
        </Routes>
    );
};

export default CollectionsPageRouter;
