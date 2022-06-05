import { Edit } from '@mui/icons-material';
import { IconButton, Portal } from '@mui/material';
import dayjs from 'dayjs';
import { FC, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { CreateCollectionModal } from '../../../controls/Modals/CreateCollectionModal';
import { TableCell, TableRow } from '../../../controls/Table/Table';
import { Collection } from '../../../types/Collection';

interface CollectionRowProps {
    collection: Collection;
}

export const CollectionRow: FC<CollectionRowProps> = ({ collection }) => {
    const date = dayjs(collection.createdAt);
    const navigate = useNavigate();
    const [showEditCollectionModal, setShowEditCollectionModal] = useState(false);

    return (
        <>
            <Portal>
                {showEditCollectionModal && (
                    <CreateCollectionModal
                        open
                        onClose={() => setShowEditCollectionModal(false)}
                        userId={collection.userId}
                        collectionId={collection.id}
                    />
                )}
            </Portal>
            <TableRow
                hover
                onClick={() => navigate(`${collection.userId}-${collection.id}`)}
                style={{ position: 'relative' }}
            >
                <TableCell>{collection.title}</TableCell>
                <TableCell align="center">{collection.cardsCount}</TableCell>
                <TableCell align="center">{date.format('L')}</TableCell>
                <TableCell width={50}>
                    <IconButton
                        onClick={(e) => {
                            e.stopPropagation();
                            setShowEditCollectionModal(true);
                        }}
                    >
                        <Edit fontSize="small" />
                    </IconButton>
                </TableCell>
            </TableRow>
        </>
    );
};
