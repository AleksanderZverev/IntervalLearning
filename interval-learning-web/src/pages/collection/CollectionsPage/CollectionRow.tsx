import { FC } from 'react';
import { TableCell, LinkTableRow } from '../../../controls/Table/Table';
import { Collection } from '../../../types/Collection';

interface CollectionRowProps {
    collection: Collection;
}

export const CollectionRow: FC<CollectionRowProps> = ({ collection }) => {
    const date = new Date(collection.createdAt);

    return (
        <LinkTableRow to={`${collection.userId}-${collection.id}`}>
            <TableCell>{collection.title}</TableCell>
            <TableCell align="center">-</TableCell>
            <TableCell align="center">{collection.cardsCount}</TableCell>
            <TableCell align="center">{date.toLocaleDateString()}</TableCell>
        </LinkTableRow>
    );
};
