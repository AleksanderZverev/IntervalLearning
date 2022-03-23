import { Button } from '@mui/material';
import { FC, useState } from 'react';
import { CreateCollectionModal } from '../../src/controls/Modals/CreateCollectionModal';
import { PageContainer } from '../../src/controls/PageContainer/PageContainer';
import { withQueryResolver } from '../../src/hoc/withQueryResolver';
import useTypedSelector from '../../src/hooks/useTypedSelector';
import { useGetCollectionsQuery } from '../../src/redux/collectionApi';
import { selectCollections } from '../../src/redux/slices/collectionsSlice';
import style from './collections.module.css';

const CollectionsPageContent: FC = () => {
    const [showCreateCollectionModal, setShowCreateCollectionModal] = useState(false);
    const collections = useTypedSelector(selectCollections);

    return (
        <PageContainer>
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
        </PageContainer>
    );
};

const CollectionsPage = withQueryResolver(useGetCollectionsQuery)(CollectionsPageContent);
//const AuthorizationPage = withAuthorization(CollectionsPage);

(CollectionsPage as any).auth = true;

export default CollectionsPage;
