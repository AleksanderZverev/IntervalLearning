import {FC, MouseEventHandler, useMemo, useState} from "react";
import {Autocomplete, Button, Dialog, DialogActions, DialogContent, DialogTitle, TextField} from "@mui/material";
import useTypedSelector from "../../hooks/useTypedSelector";
import {selectCollections} from "../../redux/slices/collectionsSlice";
import {FormField} from "../Form/Form";
import {Card, Collection} from "../../types/Collection";
import {useMoveCardMutation} from "../../redux/cardsApi";
import {withMutationResolver, WithMutationResolverProps} from "../../hoc/withQueryResolver";

interface MoveCardModalProps extends WithMutationResolverProps<typeof useMoveCardMutation> {
    isOpen: boolean;
    card: Card;
    onClose: () => void;
}

const MoveCardModalComponent: FC<MoveCardModalProps> = (
    {
        mutationProps: {mutate: moveCard, showRetryModal, isLoading},
        ...props
    }) => {
    const availableCollections = useTypedSelector(selectCollections)
        .filter(x => x.id !== props.card.collectionId);
    const [value, setValue] = useState<Collection | null>(null);
    const onMove = async () => {
        if (!value)
            return;
        try {
            await moveCard({
                collectionId: props.card.collectionId, userId: props.card.userId, request: {
                    destinationCollectionId: value.id,
                    cardId: props.card.id
                }
            })
        } catch {
            await onMove();
        }
    }

    return (
        <Dialog open={props.isOpen} fullWidth maxWidth={"sm"} onClose={props.onClose}>
            <DialogTitle>{"Переместить карточку в"}</DialogTitle>
            <DialogContent>
                <Autocomplete
                    renderInput={(params) => <FormField {...params} withoutErrorMessage/>}
                    options={availableCollections}
                    renderOption={(props, option, state) => (<li {...props}>{option.title}</li>)}
                    getOptionLabel={option => option.title}
                    value={value}
                    onChange={(event, newValue) => setValue(newValue ?? null)}
                />
                <DialogActions>
                    <Button variant="contained" onClick={onMove}
                            disabled={!Boolean(value) || isLoading}>{"Переместить"}</Button>
                </DialogActions>
            </DialogContent>
        </Dialog>)
}

export const MoveCardModal = withMutationResolver(useMoveCardMutation,
    "Не удалось переместить карточку")(MoveCardModalComponent);