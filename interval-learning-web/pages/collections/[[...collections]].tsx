import { FC, useState } from 'react';
import { BrowserRouter, Route, Routes } from 'react-router-dom';
import { CollectionPage } from '../../src/pages/collection/CollectionPage/CollectionPage';
import { CollectionsPage } from '../../src/pages/collection/CollectionsPage/CollectionsPage';

const CollectionsPageRouter: FC = () => {
    if (typeof window === 'undefined') {
        return <div></div>;
    }

    return (
        <BrowserRouter>
            <Routes>
                <Route path="/collections" element={<CollectionsPage queryArg={undefined} />} />
                <Route path="/collections/:collectionId" element={<CollectionPage />} />
            </Routes>
        </BrowserRouter>
    );
};

export default CollectionsPageRouter;
