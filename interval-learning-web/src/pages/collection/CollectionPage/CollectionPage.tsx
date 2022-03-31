import { FC } from 'react';
import { useParams } from 'react-router-dom';
import { PageContainer } from '../../../controls/PageContainer/PageContainer';
import { PageHeader } from '../../../controls/PageHeader/PageHeader';
import useTypedSelector from '../../../hooks/useTypedSelector';
import { selectCollectionById } from '../../../redux/slices/collectionsSlice';

export const CollectionPage: FC = () => {
    const { collectionId } = useParams();

    if (!collectionId) {
        throw new Error();
    }

    const collection = useTypedSelector((state) => selectCollectionById(state, collectionId));

    if (!collection) {
        throw new Error();
    }

    return (
        <PageContainer>
            <PageHeader title={collection.title} />
        </PageContainer>
    );
};
