import { TableCell } from '@mui/material';
import { FC } from 'react';
import { TableRow } from '../../../../../controls/Table/Table';
import useTypedSelector from '../../../../../hooks/useTypedSelector';
import { selectTheme } from '../../../../../redux/slices/themeSlice';
import { Collection } from '../../../../../types/Collection';

interface CollectionRowProps {
    collection: Collection;
    isRelearning?: boolean;
    onClick: (collection: Collection, isRelearning: boolean) => void;
}

export const CollectionRow: FC<CollectionRowProps> = ({ collection, isRelearning, ...props }) => {
    const theme = useTypedSelector((state) => selectTheme(state, collection.themeId));

    var total = isRelearning ? collection.canRelearnCardCount : collection.notStartedCards;
    const learnedCards = isRelearning ? 0 : collection.cardsCount - collection.notStartedCards;
    const clickable = learnedCards < collection.cardsCount;

    const onClick = () => {
        if (clickable) {
            props.onClick(collection, Boolean(isRelearning));
        }
    };

    return (
        <TableRow hover onClick={onClick}>
            <TableCell>{collection.title}</TableCell>
            <TableCell align="center">
                {collection.cardsCount > 0
                    ? `${learnedCards}/${isRelearning ? collection.canRelearnCardCount : collection.cardsCount}`
                    : 'пустая коллекция'}
            </TableCell>
            <TableCell align="center">{theme?.name}</TableCell>
        </TableRow>
    );
};
