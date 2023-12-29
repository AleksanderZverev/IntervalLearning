import { ArrowForward, Construction, Delete, Edit, KeyboardArrowRight } from '@mui/icons-material';
import {
    Collapse,
    Divider,
    IconButton,
    ListItemIcon,
    ListItemText,
    Menu,
    MenuItem,
    Portal,
    Stack,
} from '@mui/material';
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

    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
    const open = Boolean(anchorEl);

    const onShowMenu = (element: HTMLElement) => {
        setAnchorEl(element);
    };

    const onCloseMenu = () => {
        setAnchorEl(null);
    };

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
                <TableCell width={1}>
                    <IconButton onClick={(e) => onShowMenu(e.currentTarget)}>
                        <Construction />
                    </IconButton>
                    <Menu
                        anchorEl={anchorEl}
                        open={open}
                        onClose={onCloseMenu}
                        onClick={onCloseMenu}
                        transformOrigin={{ horizontal: 'center', vertical: 'top' }}
                    >
                        <MenuItem onClick={() => setShowEditCardModal(true)}>
                            <ListItemIcon>
                                <Edit fontSize="small" />
                            </ListItemIcon>
                            <ListItemText>Изменить</ListItemText>
                        </MenuItem>
                        <MenuItem onClick={() => setShowMoveCardModal(true)}>
                            <ListItemIcon>
                                <ArrowForward fontSize={'small'} />
                            </ListItemIcon>
                            <ListItemText>Переместить</ListItemText>
                        </MenuItem>
                        <Divider />
                        <MenuItem onClick={() => setShowDeleteCardModal(true)}>
                            <ListItemIcon>
                                <Delete fontSize="small" color={'error'} />
                            </ListItemIcon>
                            <ListItemText>Удалить</ListItemText>
                        </MenuItem>
                    </Menu>
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
