import { FC, useLayoutEffect } from 'react';
import { WithQueryResolverData, withQueryResolver } from '../../../../hoc/withQueryResolver';
import { useGetDetailedCalendarStatisticQuery } from '../../../../redux/api/statisticsApi';
import { Schedule } from '../../../../types/schedule';
import { useLocalStorageValue } from '../../../../hooks/useLocalStorageValue';
import dayjs, { Dayjs } from 'dayjs';
import { SelectSchedule } from '../../../../controls/SelectSchedule/SelectSchedule';
import { DatePicker } from '@mui/x-date-pickers';
import styles from './styles.module.css';
import classNames from 'classnames';
import { IconButton, Stack } from '@mui/material';
import { ArrowBackIos, ArrowBackIosNew, ArrowForwardIos } from '@mui/icons-material';
import { CenterContainer } from '../../../../controls/CenterContainer/CenterContainer';

type ResolverProps = WithQueryResolverData<typeof useGetDetailedCalendarStatisticQuery>;

interface Props extends ResolverProps {
    month: Dayjs;
}

function GetDates(month: Dayjs): Dayjs[] {
    let currentDate = month.startOf('month');
    const end = month.endOf('month');

    const result = [];

    while (!currentDate.isAfter(end)) {
        result.push(currentDate);
        currentDate = currentDate.add(1, 'day');
    }

    return result;
}

function GetDateOfWeekBefore(beforeDate: Dayjs, startDayOfWeek: number): Dayjs[] {
    const datesBefore: Dayjs[] = [];

    let start = beforeDate.subtract(1, 'day');

    while (start.day() >= startDayOfWeek) {
        datesBefore.push(start);
        start = start.subtract(1, 'day');
    }

    datesBefore.reverse();
    return datesBefore;
}

function GetDatesToCompleteWeek(fromDate: Dayjs, startDayOfWeek: number): Dayjs[] {
    const datesTillEnd: Dayjs[] = [];

    let start = fromDate.add(1, 'day');

    while (start.day() != startDayOfWeek) {
        datesTillEnd.push(start);
        start = start.add(1, 'day');
    }

    return datesTillEnd;
}

const StatisticCalendarContent: FC<Props> = ({ month, queryData }) => {
    const dates = GetDates(month);

    const getQueryDataByDate = <T,>(date: Dayjs, collection: Record<string, T>): T | undefined => {
        const key = date.format('YYYY-MM-DD') + 'T00:00:00Z';
        return collection[key];
    };

    const daysTillStart = GetDateOfWeekBefore(dates[0], 1);
    const daysAfterEnd = GetDatesToCompleteWeek(dates[dates.length - 1], 1);

    return (
        <div>
            <div className={styles.learned}>Всего изучено {queryData.learnedCards}</div>
            <div className={styles.calendarBody}>
                <div key={'monday'} className={styles.dayOfWeek}>
                    Пн
                </div>
                <div key={'tuesday'} className={styles.dayOfWeek}>
                    Вт
                </div>
                <div key={'wednesday'} className={styles.dayOfWeek}>
                    Ср
                </div>
                <div key={'thursday'} className={styles.dayOfWeek}>
                    Чт
                </div>
                <div key={'friday'} className={styles.dayOfWeek}>
                    Пт
                </div>
                <div key={'saturday'} className={styles.dayOfWeek}>
                    Сб
                </div>
                <div key={'sunday'} className={styles.dayOfWeek}>
                    Вс
                </div>

                {daysTillStart.map((d) => {
                    const dateIso = d.toISOString();

                    return (
                        <div className={classNames(styles.dayBody, styles.disabledDay)} key={dateIso}>
                            <span className={styles.date}>{d.format('D')}</span>
                        </div>
                    );
                })}

                {dates.map((d) => {
                    const dateIso = d.toISOString();
                    const now = dayjs();
                    const isToday = d.isSame(now, 'date');
                    const isPassed = d.isBefore(now, 'date');

                    const queuedCards = getQueryDataByDate(d, queryData.dateQueueCards);
                    const learnCards = getQueryDataByDate(d, queryData.dateToLearnedCards);
                    const recToLearn = getQueryDataByDate(d, queryData.dateToRecommendationToLearn);
                    const repeatedCards = getQueryDataByDate(d, queryData.dateToRepeatedCards);

                    let recommendationColor: string | undefined = undefined;

                    if (recToLearn !== undefined) {
                        if (recToLearn < 8) {
                            recommendationColor = '#dc5d5d';
                        } else if (recToLearn < 14) {
                            recommendationColor = '#dfa22d';
                        } else {
                            recommendationColor = '#4dab50';
                        }
                    }

                    return (
                        <div className={classNames(styles.dayBody)} key={dateIso}>
                            <span className={classNames(styles.date, isToday && styles.today)}>{d.format('D')}</span>

                            {queuedCards !== undefined && (
                                <span className={styles.waitingToRepeat}>Заплан. {queuedCards}</span>
                            )}
                            {learnCards !== undefined && <span className={styles.learned}>Изучено {learnCards}</span>}
                            {repeatedCards !== undefined && (
                                <span className={styles.repeated}>Пов. {repeatedCards}</span>
                            )}
                            {recToLearn !== undefined && (
                                <span
                                    className={styles.recommendation}
                                    style={{ backgroundColor: isPassed ? '#b7b7b7' : recommendationColor }}
                                >
                                    {recToLearn}
                                </span>
                            )}
                        </div>
                    );
                })}

                {daysAfterEnd.map((d) => {
                    const dateIso = d.toISOString();

                    return (
                        <div className={classNames(styles.dayBody, styles.disabledDay)} key={dateIso}>
                            <span className={styles.date}>{d.format('D')}</span>
                        </div>
                    );
                })}
            </div>
        </div>
    );
};

const ConnectedCalendarLearningStatistic = withQueryResolver(useGetDetailedCalendarStatisticQuery)(
    StatisticCalendarContent
);

interface Filter {
    month: string;
    schedule: Schedule | undefined;
}

export const CalendarStatisticPage: FC = () => {
    const [filter, setFilter] = useLocalStorageValue<Filter>('calendar_learning_statistic_page', {
        month: dayjs().startOf('month').toISOString(),
        schedule: undefined,
    });

    useLayoutEffect(() => {
        if (filter?.month) {
            const today = dayjs();
            const date = dayjs(filter.month);
            if (today.month() !== date.month()) {
                setFilter({ ...filter, month: today.toISOString() });
            }
        }
    }, []);

    if (!filter) throw new Error();

    const setSchedule = (newSchedule: Schedule | undefined) => {
        setFilter({ ...filter, schedule: newSchedule });
    };

    const monthDayjs = dayjs(filter.month);
    const isFilterValid = filter.schedule && monthDayjs.isValid();

    return (
        <div style={{ marginTop: '8px' }}>
            <div
                style={{
                    display: 'flex',
                    columnGap: '8px',
                    justifyContent: 'space-between',
                    alignItems: 'baseline',
                    fontSize: '20px',
                }}
            >
                <Stack direction={'row'} gap={'8px'}>
                    <label style={{ marginTop: '2px' }}>Учебный план:</label>
                    <SelectSchedule
                        scheduleId={filter?.schedule?.id}
                        scheduleUserId={filter?.schedule?.userId}
                        onChange={(s) => setSchedule(s)}
                    />
                </Stack>
                <div style={{ display: 'flex', columnGap: '8px', alignItems: 'baseline' }}>
                    <div>
                        <IconButton
                            onClick={() =>
                                setFilter({ ...filter, month: dayjs(filter.month).subtract(1, 'month').toISOString() })
                            }
                        >
                            <ArrowBackIosNew />
                        </IconButton>
                    </div>
                    <DatePicker
                        label="From"
                        value={monthDayjs}
                        onChange={(v) => v && setFilter({ ...filter, month: v.toISOString() })}
                        views={['year', 'month']}
                        openTo="month"
                    />
                    <div>
                        <IconButton
                            onClick={() =>
                                setFilter({ ...filter, month: dayjs(filter.month).add(1, 'month').toISOString() })
                            }
                        >
                            <ArrowForwardIos />
                        </IconButton>
                    </div>
                </div>
            </div>
            {!filter.schedule && <CenterContainer>Выберите учебный план</CenterContainer>}
            {isFilterValid && filter.schedule && (
                <ConnectedCalendarLearningStatistic
                    month={monthDayjs}
                    queryArg={{
                        from: monthDayjs.startOf('month').toISOString(),
                        to: monthDayjs.endOf('month').toISOString(),
                        timezoneOffsetInMinutes: monthDayjs.utcOffset(),
                        scheduleId: filter.schedule?.id,
                        scheduleUserId: filter.schedule?.userId,
                    }}
                />
            )}
        </div>
    );
};
