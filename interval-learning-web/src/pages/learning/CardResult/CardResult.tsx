import { Button, Typography } from '@mui/material';
import dayjs from 'dayjs';
import { FC } from 'react';
import { PaperCard } from '../../../controls/PaperCard/PaperCard';
import { DateHelper } from '../../../helpers/DateHelper';
import styles from './styles.module.css';
import { CardMovementInfo } from '../../../redux/cardsApi';
import _ from 'lodash';
import { RememberAnswer } from '../../../types/global';

interface CardResultProps {
    wordsLearned: number;
    nextRepeatDate: string | null;
    cardMovementInfos: CardMovementInfo[];
    rememberedWeights?: number[];
    onEndButtonClick: () => void;
    hasNextPage?: boolean;
    onNextPageButtonClicked?: () => void;
}

interface Statistic {
    total: number;
    rememberedCount: number;
    notSureCount: number;
    forgottenCount: number;
}

function GetStatistic(rememberWeights?: number[] | null): Statistic | null {
    if (_.isNil(rememberWeights) || rememberWeights.length === 0) return null;

    const total = rememberWeights.length;
    let rememberedCount = 0;
    let notSureCount = 0;
    let forgottenCount = 0;

    for (const weight of rememberWeights) {
        const answer = new RememberAnswer(weight);

        if (answer.IsRemembered()) {
            rememberedCount++;
        }

        if (answer.IsNotSure()) {
            notSureCount++;
        }

        if (answer.IsForgotten()) {
            forgottenCount++;
        }
    }

    return {
        total: total,
        rememberedCount: rememberedCount,
        notSureCount: notSureCount,
        forgottenCount: forgottenCount,
    };
}

function Percent(count: number, total: number) {
    const percentValue = (count / total) * 100;
    return percentValue;
}

export const CardResult: FC<CardResultProps> = ({
    nextRepeatDate,
    wordsLearned,
    onEndButtonClick,
    cardMovementInfos,
    rememberedWeights,
    hasNextPage,
    onNextPageButtonClicked,
}) => {
    const date = dayjs(nextRepeatDate);
    const now = dayjs();
    const diffMinutes = date.diff(now, 'minutes');

    const statistic = GetStatistic(rememberedWeights);

    const renderStatistic = (title: string, count: number, total: number) => {
        if (count === 0) return false;

        return (
            <div>
                {title}: {count} - {Percent(count, total).toFixed(2)}%
            </div>
        );
    };

    return (
        <PaperCard
            rightButton={
                hasNextPage && onNextPageButtonClicked ? (
                    <Button variant="outlined" onClick={onNextPageButtonClicked}>
                        Продолжить
                    </Button>
                ) : (
                    <Button variant="contained" onClick={onEndButtonClick}>
                        Завершить
                    </Button>
                )
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
                {statistic && (
                    <div style={{ display: 'flex', flexDirection: 'column' }}>
                        {renderStatistic('Запомнено', statistic.rememberedCount, statistic.total)}
                        {renderStatistic('Частично', statistic.notSureCount, statistic.total)}
                        {renderStatistic('Забыто', statistic.forgottenCount, statistic.total)}
                    </div>
                )}
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
