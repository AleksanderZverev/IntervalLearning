import { FC } from 'react';
import { Button, Dialog, DialogActions, DialogContent, DialogTitle, Stack } from '@mui/material';
import { Card } from '../../types/Collection';
import { useDeleteCardMutation } from '../../redux/cardsApi';
import { withMutationResolver, WithMutationResolverProps } from '../../hoc/withQueryResolver';
import { KeyboardArrowRight } from '@mui/icons-material';
import { Label } from '../Label/Label';

interface DeleteCardModalProps extends WithMutationResolverProps<typeof useDeleteCardMutation> {
    isOpen: boolean;
    card: Card;
    onClose: () => void;
    onDeleted?: (card: Card) => void;
}

const DeleteCardModalComponent: FC<DeleteCardModalProps> = ({
    mutationProps: { mutate: deleteCard, showRetryModal, isLoading },
    card,
    ...props
}) => {
    const onDelete = async () => {
        try {
            const deletingCard = card;
            await deleteCard({
                collectionId: card.collectionId,
                userId: card.userId,
                request: { cardId: card.id },
            });
            props.onDeleted ? props.onDeleted(deletingCard) : props.onClose();
        } catch (e) {
            console.debug('deleting card failure', e);
            showRetryModal(onDelete);
        }
    };

    return (
        <Dialog open={props.isOpen} fullWidth maxWidth={'sm'} onClose={props.onClose}>
            <DialogTitle>Удаление карточки</DialogTitle>
            <DialogContent>
                <div style={{ display: 'flex', flexDirection: 'column', rowGap: 15 }}>
                    <Label label="Запомнить">{card.frontSideText}</Label>
                    <Label label="Значение">{card.backSideText}</Label>
                    {card.description ? <Label label="Описание">{card.description}</Label> : null}
                    {card.examples && card.examples.length > 0 && (
                        <Label label="Примеры">
                            <Stack component={'ul'} spacing={'5px'}>
                                {card.examples.map((e) => {
                                    return (
                                        <li key={e} style={{ display: 'flex', alignItems: 'center' }}>
                                            <KeyboardArrowRight color={'primary'} />
                                            <span>{e}</span>
                                        </li>
                                    );
                                })}
                            </Stack>
                        </Label>
                    )}
                </div>
            </DialogContent>
            <DialogActions>
                <Button variant="contained" color="error" onClick={onDelete}>
                    Подтвердить
                </Button>
            </DialogActions>
        </Dialog>
    );
};

export const DeleteCardModal = withMutationResolver(
    useDeleteCardMutation,
    'Не удалось переместить карточку'
)(DeleteCardModalComponent);
