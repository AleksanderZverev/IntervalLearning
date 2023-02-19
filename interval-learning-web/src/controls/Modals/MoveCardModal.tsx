import { FC, useState } from 'react';
import { Autocomplete, Button, Dialog, DialogActions, DialogContent, DialogTitle, Stack } from '@mui/material';
import useTypedSelector from '../../hooks/useTypedSelector';
import { selectCollections } from '../../redux/slices/collectionsSlice';
import { Form, FormField } from '../Form/Form';
import { Card, Collection } from '../../types/Collection';
import { useMoveCardMutation } from '../../redux/cardsApi';
import { withMutationResolver, WithMutationResolverProps } from '../../hoc/withQueryResolver';
import { Label } from '../Label/Label';
import { KeyboardArrowRight } from '@mui/icons-material';

interface MoveCardModalProps extends WithMutationResolverProps<typeof useMoveCardMutation> {
    isOpen: boolean;
    card: Card;
    onClose: () => void;
}

const MoveCardModalComponent: FC<MoveCardModalProps> = ({
    mutationProps: { mutate: moveCard, showRetryModal, isLoading },
    ...props
}) => {
    const availableCollections = useTypedSelector(selectCollections).filter((x) => x.id !== props.card.collectionId);
    const [selectedCollection, setSelectedCollection] = useState<Collection | null>(null);
    const onMove = async () => {
        if (!selectedCollection) return;
        try {
            await moveCard({
                collectionId: props.card.collectionId,
                userId: props.card.userId,
                request: {
                    destinationCollectionId: selectedCollection.id,
                    cardId: props.card.id,
                },
            });
        } catch {
            showRetryModal(onMove);
        }
    };

    return (
        <Dialog open={props.isOpen} fullWidth maxWidth={'sm'} onClose={props.onClose}>
            <DialogTitle>
                Перемещение карточки «{props.card.backSideText} ({props.card.frontSideText})»
            </DialogTitle>
            <DialogContent>
                <Label label="Выберите коллекцию">
                    <Autocomplete
                        renderInput={(params) => <FormField {...params} withoutErrorMessage />}
                        options={availableCollections}
                        renderOption={(props, option, state) => <li {...props}>{option.title}</li>}
                        getOptionLabel={(option) => option.title}
                        value={selectedCollection}
                        onChange={(event, newValue) => setSelectedCollection(newValue ?? null)}
                    />
                </Label>
                <DialogActions>
                    <Button variant="contained" onClick={onMove} disabled={!Boolean(selectedCollection) || isLoading}>
                        Переместить
                    </Button>
                </DialogActions>
            </DialogContent>
        </Dialog>
    );
};

export const MoveCardModal = withMutationResolver(
    useMoveCardMutation,
    'Не удалось переместить карточку'
)(MoveCardModalComponent);
