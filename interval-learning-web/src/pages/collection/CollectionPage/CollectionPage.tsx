import { Add } from '@mui/icons-material';
import { Button } from '@mui/material';
import { FC, useState } from 'react';
import { useParams } from 'react-router-dom';
import { CreateCardModal } from '../../../controls/Modals/CreateCardModal';
import { PageContainer } from '../../../controls/PageContainer/PageContainer';
import { PageHeader } from '../../../controls/PageHeader/PageHeader';
import useTypedSelector from '../../../hooks/useTypedSelector';
import { selectCollectionById } from '../../../redux/slices/collectionsSlice';
import { getScheduleId, selectScheduleById } from '../../../redux/slices/scheduleSlice';

export const CollectionPage: FC = () => {
    const { collectionId } = useParams();

    if (!collectionId) {
        throw new Error();
    }

    const collection = useTypedSelector((state) => selectCollectionById(state, collectionId));
    const [showCreateCardModal, setShowCreateCardModal] = useState(false);
    const defaultSchedule = useTypedSelector(
        (state) =>
            collection &&
            selectScheduleById(state, getScheduleId(collection?.defaultScheduleUserId, collection?.defaultScheduleId))
    );

    if (!collection) {
        return (
            <PageContainer>
                <PageHeader title="Коллекция не найдена" />
            </PageContainer>
        );
    }

    return (
        <PageContainer>
            <PageHeader
                title={collection.title}
                subMenu={
                    <Button variant="contained" endIcon={<Add />} onClick={() => setShowCreateCardModal(true)}>
                        Слово
                    </Button>
                }
            />
            {showCreateCardModal && (
                <CreateCardModal
                    open={showCreateCardModal}
                    onClose={() => setShowCreateCardModal(false)}
                    defaultSchedule={defaultSchedule}
                />
            )}
            <div>
                {collection.cards.map((c) => (
                    <div key={c.id}>{c.frontSideText + ' - ' + c.backSideText}</div>
                ))}
            </div>
        </PageContainer>
    );
};
