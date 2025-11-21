import { TableCell } from '@mui/material';
import { FC } from 'react';
import { TableRow } from '../../../../../controls/Table/Table';
import useTypedSelector from '../../../../../hooks/useTypedSelector';
import { selectTheme } from '../../../../../redux/slices/themeSlice';

interface CollectionRowProps {
    themeId: number;
    collectionTitle: string;
    cardsToRepeatCount: number;
    onClick: () => void;
    hover: boolean;
    notFinished: boolean;
}

export const CollectionRow: FC<CollectionRowProps> = ({
    themeId,
    collectionTitle,
    cardsToRepeatCount,
    hover,
    notFinished,
    onClick,
}) => {
    const theme = useTypedSelector((state) => selectTheme(state, themeId));

    return (
        <TableRow hover={hover} onClick={() => hover && onClick()}>
            <TableCell>{collectionTitle}</TableCell>
            {/* <TableCell align="center">{}</TableCell> */}
            <TableCell align="center">
                {notFinished ? <span style={{ color: '#DC5D5D' }}>В процессе</span> : cardsToRepeatCount}
            </TableCell>
            <TableCell align="center">{theme?.name}</TableCell>
        </TableRow>
    );
};
