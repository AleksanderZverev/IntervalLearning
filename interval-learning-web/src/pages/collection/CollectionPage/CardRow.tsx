import { KeyboardArrowDown, KeyboardArrowRight, KeyboardArrowUp } from '@mui/icons-material';
import { Collapse, IconButton, TableRow } from '@mui/material';
import { FC, useState } from 'react';
import { TableCell } from '../../../controls/Table/Table';
import { Card } from '../../../types/Collection';

interface CardRowProps {
    card: Card;
}

export const CardRow: FC<CardRowProps> = ({ card }) => {
    const [showDetails, setShowDetails] = useState(false);
    return (
        <>
            <TableRow sx={{ '& > *': { borderBottom: '0' } }}>
                <TableCell>{card.frontSideText}</TableCell>
                <TableCell>{card.backSideText}</TableCell>
                <TableCell>{card.description}</TableCell>
                <TableCell align="right" onClick={() => setShowDetails(!showDetails)}>
                    <IconButton>{showDetails ? <KeyboardArrowUp /> : <KeyboardArrowDown />}</IconButton>
                </TableCell>
            </TableRow>
            <TableRow>
                <TableCell colSpan={4} sx={{ padding: 0 }}>
                    <Collapse in={showDetails} timeout="auto" unmountOnExit>
                        {card.examples?.map((e) => {
                            return (
                                <div key={e} style={{ display: 'flex', alignItems: 'center' }}>
                                    <KeyboardArrowRight />
                                    <span>{e}</span>
                                </div>
                            );
                        })}
                    </Collapse>
                </TableCell>
            </TableRow>
        </>
    );
};
