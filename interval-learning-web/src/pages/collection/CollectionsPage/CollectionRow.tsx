import { FC } from 'react';
import { Link } from 'react-router-dom';
import { TableRow, TableCell } from '../../../controls/Table/Table';
import { Collection } from '../../../types/Collection';

interface CollectionRowProps {
    collection: Collection;
}

export const CollectionRow: FC<CollectionRowProps> = ({ collection }) => {
    const date = new Date(collection.createdAt);

    return (
        // <TableRow onClick={}>
        <Link to={`${collection.userId}-${collection.id}`} style={{ display: 'table-row' }}>
            <TableCell>{collection.title}</TableCell>
            <TableCell align="center">-</TableCell>
            <TableCell align="center">{collection.cards.length}</TableCell>
            <TableCell align="center">{date.toLocaleDateString()}</TableCell>
        </Link>
        // </TableRow>
    );
};
