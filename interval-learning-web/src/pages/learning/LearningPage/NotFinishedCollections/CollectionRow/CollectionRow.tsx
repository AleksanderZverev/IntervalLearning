import { TableCell } from '@mui/material';
import { FC } from 'react';
import { useNavigate } from 'react-router-dom';
import { TableRow } from '../../../../../controls/Table/Table';
import useTypedSelector from '../../../../../hooks/useTypedSelector';
import { getScheduleId, selectScheduleById } from '../../../../../redux/slices/scheduleSlice';
import { selectTheme } from '../../../../../redux/slices/themeSlice';
import { Collection } from '../../../../../types/Collection';

interface CollectionRowProps {
    collection: Collection;
}

export const CollectionRow: FC<CollectionRowProps> = ({ collection }) => {
    const navigate = useNavigate();
    const theme = useTypedSelector((state) => selectTheme(state, collection.themeId));
    const schedule = useTypedSelector((state) =>
        selectScheduleById(state, getScheduleId(collection.defaultScheduleUserId, collection.defaultScheduleId))
    );

    const learnedCards = collection.startedCards + collection.finishedCards;
    const clickable = learnedCards < collection.cardsCount;

    const onClick = () => {
        if (clickable) {
            navigate(`/learning/learn/${collection.userId}-${collection.id}`);
        }
    };

    return (
        <TableRow onClick={onClick} style={{ cursor: clickable ? 'pointer' : undefined }}>
            <TableCell>{collection.title}</TableCell>
            <TableCell>
                {collection.cardsCount > 0 ? `${learnedCards}/${collection.cardsCount}` : 'пустая коллекция'}
            </TableCell>
            <TableCell>{schedule?.cardsCountPerPhase}</TableCell>
            <TableCell>{theme?.name}</TableCell>
        </TableRow>
    );
};
