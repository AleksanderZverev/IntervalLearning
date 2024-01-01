import { Button, Typography } from '@mui/material';
import dayjs from 'dayjs';
import { FC } from 'react';
import { PaperCard } from '../../../controls/PaperCard/PaperCard';
import { DateHelper } from '../../../helpers/DateHelper';
import styles from './styles.module.css';
import { CardMovementInfo } from '../../../redux/cardsApi';

interface CardResultProps {
    wordsLearned: number;
    nextRepeatDate: string | null;
    cardMovementInfos: CardMovementInfo[];
    onEndButtonClick: () => void;
}

export const CardResult: FC<CardResultProps> = ({
    nextRepeatDate,
    wordsLearned,
    onEndButtonClick,
    cardMovementInfos,
}) => {
    const date = dayjs(nextRepeatDate);
    const now = dayjs();
    const diffMinutes = date.diff(now, 'minutes');

    return (
        <PaperCard
            rightButton={
                <Button variant="contained" onClick={onEndButtonClick}>
                    Завершить
                </Button>
            }
        >
            <div className={styles.container}>
                <div className={styles.headerContainer}>
                    <Typography variant="h3" fontSize={32}>
                        Вы выучили слов: {wordsLearned}
                    </Typography>
                </div>
                <div>
                    Следующее повторение:{' '}
                    {!nextRepeatDate
                        ? 'Неизвестно'
                        : diffMinutes < 10
                        ? 'Сегодня'
                        : `${date.format('L')} (${DateHelper.getDifferenceString(now, date, 'через')})`}
                </div>
                <div>
                    {cardMovementInfos &&
                        cardMovementInfos.length > 1 &&
                        cardMovementInfos.map((c) => {
                            const cardIds = c.cardIds;
                            const date = dayjs(c.nextRepetitionDate);
                            const isFinished = now.diff(date, 'year') > 1;

                            return (
                                <div key={c.nextRepetitionDate}>
                                    {isFinished ? (
                                        <>
                                            {cardIds.length} → {'Завершено'}
                                        </>
                                    ) : (
                                        <>
                                            {cardIds.length} → {date.format('DD.MM.YYYY')}{' '}
                                            {`(${DateHelper.getDifferenceString(now, date, 'через')})`}
                                        </>
                                    )}
                                </div>
                            );
                        })}
                </div>
            </div>
        </PaperCard>
    );
};
