import { TableCell } from '@mui/material';
import { FC } from 'react';
import { TableRow } from '../../../../../controls/Table/Table';
import useTypedSelector from '../../../../../hooks/useTypedSelector';
import { selectTheme } from '../../../../../redux/slices/themeSlice';
import { Collection } from '../../../../../types/Collection';

interface CollectionRowProps {
    collection: Collection;
    onClick: (collection: Collection) => void;
}

export const CollectionRow: FC<CollectionRowProps> = ({ collection, ...props }) => {
    const theme = useTypedSelector((state) => selectTheme(state, collection.themeId));

    const learnedCards = collection.cardsCount - collection.notStartedCards;
    const clickable = learnedCards < collection.cardsCount;

    const onClick = () => {
        if (clickable) {
            props.onClick(collection);
        }
    };

    return (
        <TableRow hover onClick={onClick}>
            <TableCell>{collection.title}</TableCell>
            <TableCell>
                {collection.cardsCount > 0 ? `${learnedCards}/${collection.cardsCount}` : 'пустая коллекция'}
            </TableCell>
            <TableCell>{}</TableCell>
            <TableCell>{theme?.name}</TableCell>
        </TableRow>
    );
};
