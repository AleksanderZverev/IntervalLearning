import { ArrowForward, Delete, Edit, KeyboardArrowRight } from '@mui/icons-material';
import { Collapse, IconButton, Portal, Stack } from '@mui/material';
import { FC, useState } from 'react';
import { CreateCardModal } from '../../../controls/Modals/CreateCardModal';
import { TableCell, TableRow } from '../../../controls/Table/Table';
import { Card } from '../../../types/Collection';
import { withMutationResolver, WithMutationResolverProps } from '../../../hoc/withQueryResolver';
import { useDeleteCardMutation } from '../../../redux/cardsApi';
import { MoveCardModal } from '../../../controls/Modals/MoveCardModal';
import { DeleteCardModal } from '../../../controls/Modals/DeleteCardModal';

interface CardRowProps extends WithMutationResolverProps<typeof useDeleteCardMutation> {
    card: Card;
}

const CardRowComponent: FC<CardRowProps> = ({ mutationProps: { mutate: deleteCard }, card }) => {
    const [showDetails, setShowDetails] = useState(false);
    const [showEditCardModal, setShowEditCardModal] = useState(false);
    const [showMoveCardModal, setShowMoveCardModal] = useState(false);
    const [showDeleteCardModal, setShowDeleteCardModal] = useState(false);

    const hasExamples = card.examples && card.examples.length > 0;

    const onShowDetails = () => {
        if (hasExamples) {
            setShowDetails(!showDetails);
        }
    };

    return (
        <>
            <Portal>
                {showEditCardModal && (
                    <CreateCardModal
                        open
                        collectionId={card.collectionId}
                        collectionUserId={card.userId}
                        cardId={card.id}
                        onClose={() => setShowEditCardModal(false)}
                    />
                )}
                {showMoveCardModal && (
                    <MoveCardModal isOpen={showMoveCardModal} card={card} onClose={() => setShowMoveCardModal(false)} />
                )}
                {showDeleteCardModal && (
                    <DeleteCardModal
                        isOpen={showDeleteCardModal}
                        card={card}
                        onClose={() => setShowDeleteCardModal(false)}
                    />
                )}
            </Portal>
            <TableRow borderless hover={Boolean(hasExamples)} onClick={() => onShowDetails()}>
                <TableCell>{card.frontSideText}</TableCell>
                <TableCell>{card.promptText}</TableCell>
                <TableCell>{card.backSideText}</TableCell>
                <TableCell>{card.description}</TableCell>
                <TableCell sx={{ position: 'relative', paddingRight: 5 }}>
                    <Stack direction={'row'} sx={{ position: 'absolute', right: 20, top: 10 }}>
                        <IconButton
                            onClick={(e) => {
                                e.stopPropagation();
                                setShowEditCardModal(true);
                            }}
                        >
                            <Edit fontSize="small" />
                        </IconButton>
                        <IconButton onClick={() => setShowMoveCardModal(true)}>
                            <ArrowForward fontSize={'small'} />
                        </IconButton>
                        <IconButton onClick={() => setShowDeleteCardModal(true)}>
                            <Delete fontSize="small" color={'error'} />
                        </IconButton>
                    </Stack>
                </TableCell>
            </TableRow>
            <TableRow>
                <TableCell colSpan={4} sx={{ padding: 0 }}>
                    <Collapse in={showDetails} timeout="auto" unmountOnExit>
                        <div style={{ padding: '16px' }}>
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

export const CardRow = withMutationResolver(useDeleteCardMutation, 'Не удалось удалить карточку')(CardRowComponent);
