import { FC, useState } from 'react';
import { Button, IconButton, Paper, Portal, Typography } from '@mui/material';
import { Card } from '../../../../types/Collection';
import styles from './styles.module.css';
import { Edit, KeyboardArrowRight } from '@mui/icons-material';
import { CreateCardModal } from '../../../../controls/Modals/CreateCardModal';
import useTypedSelector from '../../../../hooks/useTypedSelector';
import { selectCardById } from '../../../../redux/slices/cardsSlice';

interface LearnCardProps {
    userId: string;
    collectionId: string;
    cardId: string;
    showNext: boolean;
    showPrevious: boolean;
    onNext: () => void;
    onPrevious: () => void;
    onFinish: () => void;
}

export const LearnCard: FC<LearnCardProps> = ({
    userId,
    collectionId,
    cardId,
    showNext,
    showPrevious,
    onNext,
    onPrevious,
    onFinish: onEndButtonClick,
}) => {
    const card = useTypedSelector((state) => selectCardById(state, userId, collectionId, cardId));

    if (!card) {
        throw new Error();
    }

    const containsDescriptionAndExamples = Boolean(card.description || (card.examples && card.examples.length > 0));

    const [showEditCardModal, setShowEditCardModal] = useState(false);

    return (
        <Paper
            className={styles.container}
            style={{ justifyContent: containsDescriptionAndExamples ? 'flex-start' : 'center' }}
        >
            <Portal>
                {showEditCardModal && (
                    <CreateCardModal
                        open
                        onClose={() => setShowEditCardModal(false)}
                        collectionId={card.collectionId}
                        collectionUserId={card.userId}
                        cardId={card.id}
                    />
                )}
            </Portal>
            <IconButton className={styles.editButton} onClick={() => setShowEditCardModal(true)}>
                <Edit fontSize="small" />
            </IconButton>

            <div className={styles.headerContainer}>
                <Typography variant="h3" fontSize={32}>
                    {card.frontSideText}
                </Typography>
                <div className={styles.backContainer}>{card.backSideText}</div>
            </div>
            {card.description && (
                <div>
                    <div className={styles.label}>Описание</div>
                    <div>{card.description}</div>
                </div>
            )}
            {card.examples && card.examples.length > 0 && (
                <div>
                    <div className={styles.label}>Примеры</div>
                    <div>
                        {card.examples.map((e) => {
                            return (
                                <div key={e} style={{ display: 'flex', alignItems: 'center' }}>
                                    <KeyboardArrowRight />
                                    <span>{e}</span>
                                </div>
                            );
                        })}
                    </div>
                </div>
            )}
            <div className={styles.buttonsContainer}>
                {showPrevious ? (
                    <Button tabIndex={1} variant="outlined" onClick={() => onPrevious()}>
                        Назад
                    </Button>
                ) : (
                    <div />
                )}
                {showNext ? (
                    <Button tabIndex={2} variant="outlined" onClick={() => onNext()}>
                        Далее
                    </Button>
                ) : (
                    <Button tabIndex={2} variant="contained" onClick={() => onEndButtonClick()}>
                        Завершить
                    </Button>
                )}
            </div>
        </Paper>
    );
};
