import { ArrowForward, Construction, Delete, Edit, KeyboardArrowRight, Replay } from '@mui/icons-material';
import {
    CircularProgress,
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
import { useRelearnCardMutation } from '../../../redux/cardsApi';
import { MoveCardModal } from '../../../controls/Modals/MoveCardModal';
import { DeleteCardModal } from '../../../controls/Modals/DeleteCardModal';
import { AssertionModal } from '../../../controls/Modals/AssertionModal';

interface CardRowProps extends WithMutationResolverProps<typeof useRelearnCardMutation> {
    card: Card;
}

const CardRowComponent: FC<CardRowProps> = ({ mutationProps: { mutate: relearnCard, ...mutateProps }, card }) => {
    const [showDetails, setShowDetails] = useState(false);
    const [showEditCardModal, setShowEditCardModal] = useState(false);
    const [showMoveCardModal, setShowMoveCardModal] = useState(false);
    const [showDeleteCardModal, setShowDeleteCardModal] = useState(false);
    const [showAssertRelearnCardModal, setAssertRelearnCardModal] = useState(false);
    const [isAddedToRelearn, setAddedToRelearn] = useState(false);

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

    const onRelearnCard = async () => {
        setAssertRelearnCardModal(false);

        try {
            await relearnCard({
                userId: card.userId,
                collectionId: card.collectionId,
                request: {
                    cardId: card.id,
                },
            });
            setAddedToRelearn(true);
            onCloseMenu();
        } catch {
            setAddedToRelearn(false);
            mutateProps.showRetryModal(() => onRelearnCard());
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
                {showAssertRelearnCardModal && (
                    <AssertionModal
                        title={`Добавление карточки в список переизучения`}
                        message={`Будет добавлена карточка «${card.backSideText} - ${card.frontSideText}»`}
                        onClose={() => setAssertRelearnCardModal(false)}
                        assertTitle="Подтвердить"
                        onAssert={() => onRelearnCard()}
                    />
                )}
            </Portal>
            <TableRow borderless hover={Boolean(hasExamples)} onClick={() => onShowDetails()}>
                <TableCell>{card.frontSideText}</TableCell>
                <TableCell>{card.promptText}</TableCell>
                <TableCell>{card.backSideText}</TableCell>
                <TableCell>{card.description}</TableCell>
                <TableCell width={1} onClick={(e) => e.stopPropagation()}>
                    <IconButton onClick={(e) => onShowMenu(e.currentTarget)}>
                        <Construction />
                    </IconButton>
                    <Menu
                        anchorEl={anchorEl}
                        open={open}
                        onClose={onCloseMenu}
                        transformOrigin={{ horizontal: 'center', vertical: 'top' }}
                    >
                        <MenuItem
                            onClick={() => {
                                setShowEditCardModal(true);
                                onCloseMenu();
                            }}
                        >
                            <ListItemIcon>
                                <Edit fontSize="small" />
                            </ListItemIcon>
                            <ListItemText>Изменить</ListItemText>
                        </MenuItem>

                        <MenuItem
                            onClick={() => {
                                setShowMoveCardModal(true);
                                onCloseMenu();
                            }}
                        >
                            <ListItemIcon>
                                <ArrowForward fontSize={'small'} />
                            </ListItemIcon>
                            <ListItemText>Переместить</ListItemText>
                        </MenuItem>
                        <MenuItem
                            disabled={isAddedToRelearn || mutateProps.isLoading}
                            onClick={() => setAssertRelearnCardModal(true)}
                        >
                            <ListItemIcon>
                                {mutateProps.isLoading ? <CircularProgress size={16} /> : <Replay fontSize={'small'} />}
                            </ListItemIcon>
                            <ListItemText>Изучить снова</ListItemText>
                        </MenuItem>
                        <Divider />
                        <MenuItem
                            onClick={() => {
                                setShowDeleteCardModal(true);
                                onCloseMenu();
                            }}
                        >
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

export const CardRow = withMutationResolver(
    useRelearnCardMutation,
    'Не добавить карточку в список переизучения'
)(CardRowComponent);
