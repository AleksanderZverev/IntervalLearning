import { KeyboardArrowDown, KeyboardArrowRight, KeyboardArrowUp } from '@mui/icons-material';
import { Collapse, IconButton, Stack } from '@mui/material';
import { FC, useState } from 'react';
import { TableCell, TableRow } from '../../../controls/Table/Table';
import { Card } from '../../../types/Collection';

interface CardRowProps {
    card: Card;
}

export const CardRow: FC<CardRowProps> = ({ card }) => {
    const [showDetails, setShowDetails] = useState(false);

    const onShowDetails = () => {
        if (card.examples && card.examples.length > 0) {
            setShowDetails(!showDetails);
        }
    };

    return (
        <>
            <TableRow borderless>
                <TableCell>{card.frontSideText}</TableCell>
                <TableCell>{card.backSideText}</TableCell>
                <TableCell sx={{ position: 'relative', paddingRight: 5 }}>
                    <div> {card.description}</div>

                    {card.examples && card.examples.length > 0 && (
                        <IconButton sx={{ position: 'absolute', right: 0, top: 10 }} onClick={onShowDetails}>
                            {showDetails ? <KeyboardArrowUp /> : <KeyboardArrowDown />}
                        </IconButton>
                    )}
                </TableCell>
            </TableRow>
            <TableRow>
                <TableCell colSpan={4} sx={{ padding: 0 }}>
                    <Collapse in={showDetails} timeout="auto" unmountOnExit>
                        <div style={{ padding: '0 16px 16px' }}>
                            <Stack component={'ul'} spacing={'5px'}>
                                {card.examples?.map((e) => {
                                    return (
                                        <li key={e} style={{ display: 'flex', alignItems: 'center' }}>
                                            <KeyboardArrowRight color={'primary'} />
                                            <span>{e}</span>
                                        </li>
                                    );
                                })}
                            </Stack>
                        </div>
                    </Collapse>
                </TableCell>
            </TableRow>
        </>
    );
};
