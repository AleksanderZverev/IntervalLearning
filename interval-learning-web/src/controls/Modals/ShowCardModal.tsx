import { KeyboardArrowRight } from '@mui/icons-material';
import { Dialog, DialogTitle, DialogContent, DialogActions, Stack, Button } from '@mui/material';
import { FC, useState } from 'react';
import useTypedSelector from '../../hooks/useTypedSelector';
import { selectCardById } from '../../redux/slices/cardsSlice';
import { Label } from '../Label/Label';
import { CreateCardModal } from './CreateCardModal';
import { Tag } from '../Tags/TagView/Tag';

interface ShowCardModalProps {
    open: boolean;
    onClose: () => void;
    userId: string;
    collectionId: string;
    cardId: string;
}

export const ShowCardModal: FC<ShowCardModalProps> = ({ open, onClose, userId, collectionId, cardId }) => {
    const card = useTypedSelector((state) => selectCardById(state, userId, collectionId, cardId));

    if (!card) {
        throw new Error();
    }

    const [showEditModal, setShowEditModal] = useState(false);

    return (
        <>
            {showEditModal && (
                <CreateCardModal
                    open
                    onClose={() => setShowEditModal(false)}
                    collectionUserId={userId}
                    collectionId={collectionId}
                    cardId={cardId}
                />
            )}
            <Dialog open={open} onClose={onClose} maxWidth={'sm'} fullWidth>
                <DialogTitle>{}</DialogTitle>
                <DialogContent>
                    <div style={{ display: 'flex', flexDirection: 'column', rowGap: 15 }}>
                        <Label label="Запомнить">{card.frontSideText}</Label>
                        <Label label="Подсказка">{card.promptText}</Label>
                        <Label label="Значение">{card.backSideText}</Label>
                        <Label label="Теги">
                            {card.tags && card.tags.length > 0 && (
                                <Stack component={'ul'} spacing={'5px'}>
                                    {card.tags.map((t) => {
                                        return (
                                            <li key={t}>
                                                <Tag>{t}</Tag>
                                            </li>
                                        );
                                    })}
                                </Stack>
                            )}
                        </Label>
                        <Label label="Описание">{card.description}</Label>
                        <Label label="Примеры">
                            {card.examples && card.examples.length > 0 && (
                                <Stack component={'ul'} spacing={'5px'}>
                                    {card.examples.map((e) => {
                                        return (
                                            <li key={e} style={{ display: 'flex' }}>
                                                <KeyboardArrowRight color={'primary'} style={{ marginTop: '4px' }} />
                                                <span>{e}</span>
                                            </li>
                                        );
                                    })}
                                </Stack>
                            )}
                        </Label>
                    </div>
                </DialogContent>
                <DialogActions>
                    <div style={{ display: 'flex', justifyContent: 'space-between', width: '100%', padding: 10 }}>
                        <Button variant="outlined" onClick={() => setShowEditModal(true)}>
                            Изменить
                        </Button>
                        <Button variant="contained" onClick={onClose}>
                            Закрыть
                        </Button>
                    </div>
                </DialogActions>
            </Dialog>
        </>
    );
};
