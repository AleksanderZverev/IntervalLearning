import { Button, CircularProgress, Container } from '@mui/material';
import { FC, useEffect, useLayoutEffect, useState } from 'react';
import { CreateCollectionModal } from '../../src/controls/Modals/CreateCollectionModal';
import { withQueryResolver } from '../../src/hoc/withQueryResolver';
import { useGetCollectionsQuery } from '../../src/redux/collectionSlice';
import { Collection } from '../../src/types/Collection';
import style from './collections.module.css';

const CollectionsPageContent: FC<{ data: Collection[] }> = ({ data: collections }) => {
    const [showCreateCollectionModal, setShowCreateCollectionModal] = useState(false);

    return (
        <div>
            {showCreateCollectionModal && (
                <CreateCollectionModal
                    open={showCreateCollectionModal}
                    onClose={() => setShowCreateCollectionModal(false)}
                />
            )}
            <div className={style.headerContainer}>
                <h2 className={style.header}>Collections page</h2>
                <Button onClick={() => setShowCreateCollectionModal(true)}>Создать коллекцию</Button>
            </div>
            <div>
                {collections.map((c) => (
                    <div key={c.id}>{c.title}</div>
                ))}
            </div>
        </div>
    );
};

const CollectionsPage = withQueryResolver(useGetCollectionsQuery)(CollectionsPageContent);

(CollectionsPage as any).auth = true;

export default CollectionsPage;
