import { FC } from 'react';
import { Button, Paper, Typography } from '@mui/material';
import { Card } from '../../../../types/Collection';
import styles from './styles.module.css';
import { KeyboardArrowRight } from '@mui/icons-material';

interface LearnCardProps {
    card: Card;
    showNext: boolean;
    showPrevious: boolean;
    onNext: () => void;
    onPrevious: () => void;
    onFinish: () => void;
}

export const LearnCard: FC<LearnCardProps> = ({
    card,
    showNext,
    showPrevious,
    onNext,
    onPrevious,
    onFinish: onEndButtonClick,
}) => {
    const containsDescriptionAndExamples = Boolean(card.description || (card.examples && card.examples.length > 0));

    return (
        <Paper
            className={styles.container}
            style={{ justifyContent: containsDescriptionAndExamples ? 'flex-start' : 'center' }}
        >
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
