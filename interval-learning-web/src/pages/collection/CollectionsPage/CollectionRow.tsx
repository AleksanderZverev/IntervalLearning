import dayjs from 'dayjs';
import { FC } from 'react';
import { useNavigate } from 'react-router-dom';
import { TableCell, TableRow } from '../../../controls/Table/Table';
import { Collection } from '../../../types/Collection';

interface CollectionRowProps {
    collection: Collection;
}

export const CollectionRow: FC<CollectionRowProps> = ({ collection }) => {
    const date = dayjs(collection.createdAt);
    const navigate = useNavigate();

    return (
        <TableRow hover onClick={() => navigate(`${collection.userId}-${collection.id}`)}>
            <TableCell>{collection.title}</TableCell>
            <TableCell align="center">-</TableCell>
            <TableCell align="center">{collection.cardsCount}</TableCell>
            <TableCell align="center">{date.format('L')}</TableCell>
        </TableRow>
    );
};
