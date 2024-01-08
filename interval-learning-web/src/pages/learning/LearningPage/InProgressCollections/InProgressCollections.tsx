import classNames from 'classnames';
import dayjs from 'dayjs';
import React, { FC, Fragment, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { ScheduleKey, SelectSchedule } from '../../../../controls/SelectSchedule/SelectSchedule';
import { Table, TableBody, TableCell, TableHead, TableHeaderCell, TableRow } from '../../../../controls/Table/Table';
import { DateHelper } from '../../../../helpers/DateHelper';
import { LocalStorageHelper } from '../../../../helpers/localStorageHelper';
import {
    withOtherQueryResolver,
    WithOtherQueryResolverData,
    withQueryResolver,
    WithQueryResolverData,
} from '../../../../hoc/withQueryResolver';
import { useLocalStorageValue } from '../../../../hooks/useLocalStorageValue';
import { RepeatingPhaseDto, useGetQueueCollectionsQuery } from '../../../../redux/collectionApi';
import { Schedule } from '../../../../types/schedule';
import { CollectionRow } from './CollectionRow/CollectionRow';
import styles from './styles.module.css';
import { SelectTheme } from '../../../../controls/SelectTheme/SelectTheme';
import { Theme } from '../../../../types/global';
import useTypedSelector from '../../../../hooks/useTypedSelector';
import { selectThemes } from '../../../../redux/slices/themeSlice';
import { useGetStatisticQuery } from '../../../../redux/api/statisticsApi';
import { getRepeatingCards, isRepeatingInProgress } from '../../RepeatCollectionPage/RepeatCollectionPage.logic';
import _ from 'lodash';

export const getRepeatingNavigationLink = (
    collectionUserId: string,
    collectionId: string,
    scheduleUserId: string,
    scheduleId: string,
    phaseIndex: number,
    dateString: string
): string => {
    const searchParams = new URLSearchParams({
        scheduleUserId,
        scheduleId,
        phaseIndex: phaseIndex.toString(),
        date: dateString,
    });

    const link = `/learning/repeat/${collectionUserId}-${collectionId}?` + searchParams.toString();
    return link;
};

const getScheduleKeys = (dateToRepeatingPhases: Record<string, RepeatingPhaseDto[]>): ScheduleKey[] => {
    const scheduleIds = new Set<string>();
    for (const phases of Object.values(dateToRepeatingPhases)) {
        for (const p of phases) {
            const key = `${p.scheduleUserId}-${p.scheduleId}`;
            scheduleIds.add(key);
        }
    }

    return Array.from(scheduleIds.values()).map((p) => {
        const [scheduleUserId, scheduleId] = p.split('-');
        const key: ScheduleKey = {
            scheduleUserId,
            scheduleId,
        };
        return key;
    });
};

function CountWordsByThemes(
    schedule: Schedule | undefined,
    dateToCollectionsQueue: [string, RepeatingPhaseDto[]][]
): Map<number, number> {
    const result: Map<number, number> = new Map();

    const addToTheme = (themeId: number, cardsCount: number) => {
        if (!result.has(themeId)) {
            result.set(themeId, 0);
        }

        const oldCount = result.get(themeId) || 0;
        result.set(themeId, oldCount + cardsCount);
    };

    dateToCollectionsQueue.forEach((pair) => {
        const [dateString, phase] = pair;

        const now = dayjs();
        const date = dayjs(dateString);
        const isFuture = date.isAfter(now, 'd');

        if (isFuture) {
            return;
        }

        phase.forEach((p) => {
            if (schedule && (p.scheduleId !== schedule.id || p.scheduleUserId !== schedule.userId)) {
                return;
            }

            p.repeatingCollections.forEach((c) => {
                addToTheme(c.collection.themeId, c.cardsToRepeatCount);
            });
        });
    });

    return result;
}

type WithResolvers = WithQueryResolverData<typeof useGetQueueCollectionsQuery> &
    WithOtherQueryResolverData<typeof useGetStatisticQuery>;

interface InProgressCollectionsProps extends WithResolvers {}

interface LocalStorageState {
    schedule: Schedule | undefined;
    theme: Theme | null;
}

const InProgressCollectionsContent: FC<InProgressCollectionsProps> = ({ queryData, extendedData }) => {
    const navigate = useNavigate();
    const dateToCollectionsQueue = Object.entries(queryData.dateToRepeatingPhases);
    const now = dayjs();
    const allowFutureSelect = process.env.NODE_ENV === 'development';

    const themes = useTypedSelector((state) => selectThemes(state));
    const [localState, setLocalState] = useLocalStorageValue<LocalStorageState>('InProgressCollectionsContent-state', {
        schedule: undefined,
        theme: null,
    });

    if (!localState) throw new Error();

    const { schedule, theme } = localState;

    const setSchedule = (newSchedule: Schedule | undefined) => setLocalState({ ...localState, schedule: newSchedule });
    const setTheme = (newTheme: Theme | null) => setLocalState({ ...localState, theme: newTheme });

    const availableScheduleKeys = useMemo(
        () => getScheduleKeys(queryData.dateToRepeatingPhases),
        [queryData.dateToRepeatingPhases]
    );

    const [defaultScheduleUserId, defaultScheduleId] = useMemo(() => {
        const repeatingPhasesArray = Object.values(queryData.dateToRepeatingPhases);
        const scheduleIds = new Set<string>();

        for (const repeatingPhases of repeatingPhasesArray) {
            for (const phase of repeatingPhases) {
                const scheduleKey = `${phase.scheduleUserId}-${phase.scheduleId}`;
                scheduleIds.add(scheduleKey);
            }
        }

        const schedulesIdsArray = Array.from(scheduleIds);

        return schedulesIdsArray.length === 1 ? schedulesIdsArray[0].split('-') : [undefined, undefined];
    }, [queryData.dateToRepeatingPhases]);

    const dateToCollectionsQueueFilteredByTheme = dateToCollectionsQueue.filter((pair) => {
        const [dateString, repeatingPhases] = pair;
        const phases = repeatingPhases.filter((p) => {
            const collections = p.repeatingCollections.filter((c) => {
                if (!theme) return true;
                return c.collection.themeId === theme.id;
            });
            return collections.length > 0;
        });
        return phases.length > 0;
    });
    const dateToCollectionsSortedByDate = _.sortBy(dateToCollectionsQueueFilteredByTheme, (t) => new Date(t[0]));

    const wordByThemes = CountWordsByThemes(schedule, dateToCollectionsQueue);

    const hasStatistic = Boolean(extendedData?.learnedCards) || Boolean(extendedData?.repeatedCards);
    const dateToCollectionsToRender = dateToCollectionsSortedByDate;

    return (
        <div style={{ display: 'grid', gridTemplateRows: 'auto 1fr' }}>
            {availableScheduleKeys.length > 0 && (
                <div style={{ display: 'flex', columnGap: 20, marginTop: 10, fontSize: '20px' }}>
                    <label style={{ marginTop: 2 }}>Сортировать по учебному плану:</label>
                    <SelectSchedule
                        width="250px"
                        scheduleUserId={schedule?.userId}
                        scheduleId={schedule?.id}
                        onChange={(newSchedule) => setSchedule(newSchedule)}
                        availableSchedules={availableScheduleKeys}
                        defaultScheduleUserId={defaultScheduleUserId}
                        defaultScheduleId={defaultScheduleId}
                    />
                    <label>Тема:</label>
                    <SelectTheme value={theme} onChange={setTheme} />
                </div>
            )}
            {hasStatistic && (
                <div className={styles.statisticRow}>
                    {extendedData?.learnedCards !== undefined && extendedData.learnedCards > 0 && (
                        <span className={styles.learned}>Изучено {extendedData.learnedCards}</span>
                    )}
                    {extendedData?.repeatedCards !== undefined && extendedData.repeatedCards > 0 && (
                        <span className={styles.repeated}>Повторено {extendedData.repeatedCards}</span>
                    )}
                </div>
            )}
            <Table>
                <TableHead>
                    <TableHeaderCell></TableHeaderCell>
                    <TableHeaderCell align="center">Осталось слов</TableHeaderCell>
                    <TableHeaderCell align="center">Тип</TableHeaderCell>
                </TableHead>
                <TableBody>
                    {theme &&
                        dateToCollectionsQueue.length > 0 &&
                        dateToCollectionsQueueFilteredByTheme.length === 0 && (
                            <Fragment key={`nothing for theme ${theme.id} `}>
                                <TableRow borderless>
                                    <TableCell colSpan={4} fontSize={14} align="center">
                                        Для данной темы не осталось повторений
                                    </TableCell>
                                </TableRow>
                            </Fragment>
                        )}
                    {theme &&
                        Array.from(wordByThemes.entries()).map((pair) => {
                            const [themeId, wordsCount] = pair;

                            if (themeId === theme.id || wordsCount === 0) {
                                return null;
                            }

                            const currentTheme = themes.find((t) => t.id === themeId);
                            return (
                                <Fragment key={`not repeated cards by other theme ${themeId}`}>
                                    <TableRow borderless>
                                        <TableCell colSpan={4} fontSize={14}>
                                            В теме «{currentTheme?.name}» ждут повторения {wordsCount} карточек.
                                        </TableCell>
                                    </TableRow>
                                </Fragment>
                            );
                        })}
                    {dateToCollectionsToRender.length > 0 &&
                        dateToCollectionsToRender.map((pair) => {
                            const [dateString, repeatingPhases] = pair;
                            const date = dayjs(dateString);
                            const isWarn = now.isAfter(date, 'd');
                            const isToday = now.isSame(date, 'd');
                            const isTomorrow = now.add(dayjs.duration(1, 'd')).isSame(date, 'd');

                            const filteredPhases = repeatingPhases
                                .filter(
                                    (p) =>
                                        !schedule ||
                                        (p.scheduleUserId === schedule.userId && p.scheduleId === schedule.id)
                                )
                                .sort((first, second) => {
                                    const isLessThanHour = (p: RepeatingPhaseDto) =>
                                        p.secondsFromLastPhase < 1 * 60 * 60;

                                    const isFirstRepeatingPhase = isLessThanHour(first);
                                    const isSecondRepeatingPhase = isLessThanHour(second);

                                    if (isFirstRepeatingPhase && isSecondRepeatingPhase) {
                                        return first.phaseIndex - second.phaseIndex;
                                    }

                                    if (!isFirstRepeatingPhase && !isSecondRepeatingPhase) {
                                        return first.secondsFromLastPhase - second.secondsFromLastPhase;
                                    }

                                    return isFirstRepeatingPhase ? 1 : -1;
                                });

                            const phasesToRender = filteredPhases;

                            if (phasesToRender.length === 0) {
                                return false;
                            }

                            return (
                                <Fragment key={dateString}>
                                    <TableRow borderless>
                                        <TableCell
                                            align="center"
                                            className={classNames(styles.subLabel, {
                                                [styles.warn]: isWarn,
                                                [styles.today]: isToday,
                                            })}
                                            colSpan={4}
                                            fontSize={14}
                                        >
                                            {isToday
                                                ? 'Сегодня'
                                                : isWarn
                                                ? `${DateHelper.getDifferenceString(
                                                      now,
                                                      date,
                                                      'Просрочено на'
                                                  )} (${date.format('L')})`
                                                : isTomorrow
                                                ? 'Завтра'
                                                : `${DateHelper.getDifferenceString(date, now, 'Через')} (${date.format(
                                                      'L'
                                                  )})`}
                                        </TableCell>
                                    </TableRow>
                                    {phasesToRender.map((p) => {
                                        if (!p.repeatingCollections || p.repeatingCollections.length === 0) {
                                            console.error('repeating collections not found!');
                                            return false;
                                        }

                                        if (
                                            schedule &&
                                            (p.scheduleUserId !== schedule.userId || p.scheduleId !== schedule.id)
                                        ) {
                                            return false;
                                        }

                                        const duration = dayjs.duration(p.secondsFromLastPhase, 's');

                                        const collectionsToRepeat = p.repeatingCollections.filter((c) => {
                                            if (!theme) return true;
                                            return c.collection.themeId === theme.id;
                                        });

                                        if (!collectionsToRepeat || collectionsToRepeat.length === 0) {
                                            return (
                                                <Fragment key={`${p.scheduleUserId}-${p.scheduleId}-${p.phaseIndex}`}>
                                                    <TableRow borderless>
                                                        <TableCell
                                                            // className={classNames(styles.subLabel)}
                                                            colSpan={4}
                                                            fontSize={14}
                                                        >
                                                            Для данной темы не осталось повторений
                                                        </TableCell>
                                                    </TableRow>
                                                </Fragment>
                                            );
                                        }

                                        return (
                                            <Fragment key={`${p.scheduleUserId}-${p.scheduleId}-${p.phaseIndex}`}>
                                                <TableRow borderless>
                                                    <TableCell
                                                        className={classNames(styles.subLabel)}
                                                        colSpan={4}
                                                        fontSize={14}
                                                    >
                                                        Спустя {duration.humanize()}
                                                    </TableCell>
                                                </TableRow>
                                                {p.repeatingCollections.map((c) => {
                                                    const repeatingLink = getRepeatingNavigationLink(
                                                        c.collection.userId,
                                                        c.collection.id,
                                                        p.scheduleUserId,
                                                        p.scheduleId,
                                                        p.phaseIndex,
                                                        dateString
                                                    );

                                                    const isInProgress = isRepeatingInProgress(
                                                        p.scheduleUserId,
                                                        p.scheduleId,
                                                        p.phaseIndex,
                                                        dateString,
                                                        c.collection.id
                                                    );

                                                    return (
                                                        <CollectionRow
                                                            key={c.collection.id}
                                                            collection={c.collection}
                                                            cardsToRepeatCount={c.cardsToRepeatCount}
                                                            hover={allowFutureSelect || isToday || isWarn}
                                                            notFinished={isInProgress}
                                                            onClick={() => navigate(repeatingLink)}
                                                        />
                                                    );
                                                })}
                                            </Fragment>
                                        );
                                    })}
                                </Fragment>
                            );
                        })}
                </TableBody>
            </Table>
        </div>
    );
};

const ConnectedInProgressCollections = withQueryResolver(useGetQueueCollectionsQuery)(InProgressCollectionsContent);
const ConnectedStatisticsData = withOtherQueryResolver(useGetStatisticQuery)(ConnectedInProgressCollections);

export const InProgressCollections: FC = () => {
    return <ConnectedStatisticsData queryArg={{ date: dayjs().toISOString() }} />;
};
