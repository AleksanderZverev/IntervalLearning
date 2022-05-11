import { Button, Paper, Typography } from '@mui/material';
import dayjs from 'dayjs';
import { FC } from 'react';
import { DateHelper } from '../../../helpers/DateHelper';
import styles from './styles.module.css';

interface CardResultProps {
    wordsLearned: number;
    nextRepeatDate: string | null;
    onEndButtonClick: () => void;
}

export const CardResult: FC<CardResultProps> = ({ nextRepeatDate, wordsLearned, onEndButtonClick }) => {
    const date = dayjs(nextRepeatDate);
    const now = dayjs();
    const diffMinutes = date.diff(now, 'minutes');

    return (
        <Paper className={styles.container}>
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
                    : `${date.format('L')} (через ${DateHelper.getDifferenceString(now, date)})`}
            </div>

            <div className={styles.buttonsContainer}>
                <div />
                <Button variant="contained" onClick={onEndButtonClick}>
                    Завершить
                </Button>
            </div>
        </Paper>
    );
};
