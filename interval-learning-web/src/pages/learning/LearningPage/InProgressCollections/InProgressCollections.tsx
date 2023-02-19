import classNames from 'classnames';
import dayjs from 'dayjs';
import React, { FC, Fragment, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { ScheduleKey, SelectSchedule } from '../../../../controls/SelectSchedule/SelectSchedule';
import { Table, TableBody, TableCell, TableHead, TableHeaderCell, TableRow } from '../../../../controls/Table/Table';
import { DateHelper } from '../../../../helpers/DateHelper';
import { LocalStorageHelper } from '../../../../helpers/localStorageHelper';
import { withQueryResolver, WithQueryResolverData } from '../../../../hoc/withQueryResolver';
import { useLocalStorageValue } from '../../../../hooks/useLocalStorageValue';
import { RepeatingPhaseDto, useGetQueueCollectionsQuery } from '../../../../redux/collectionApi';
import { Schedule } from '../../../../types/schedule';
import { CollectionRow } from './CollectionRow/CollectionRow';
import styles from './styles.module.css';

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

interface InProgressCollectionsProps extends WithQueryResolverData<typeof useGetQueueCollectionsQuery> {}

const InProgressCollectionsContent: FC<InProgressCollectionsProps> = ({ queryData }) => {
    const navigate = useNavigate();
    const dateToCollectionsQueue = Object.entries(queryData.dateToRepeatingPhases);
    const now = dayjs();
    const allowFutureSelect = process.env.NODE_ENV === 'development';
    const [schedule, setSchedule] = useLocalStorageValue<Schedule | undefined>('InProgressCollectionsContent-schedule');
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
                        showWordsPerPhase
                    />
                </div>
            )}
            <Table>
                <TableHead>
                    <TableHeaderCell></TableHeaderCell>
                    {/* <TableHeaderCell align="center">Изучено</TableHeaderCell> */}
                    <TableHeaderCell align="center">Осталось слов</TableHeaderCell>
                    <TableHeaderCell align="center">Тип</TableHeaderCell>
                </TableHead>
                <TableBody>
                    {dateToCollectionsQueue
                        .sort((f, s) => f[0].localeCompare(s[0]))
                        .map((pair) => {
                            const [dateString, repeatingPhases] = pair;
                            const date = dayjs(dateString);
                            const isWarn = now.isAfter(date, 'd');
                            const isToday = now.isSame(date, 'd');
                            const isTomorrow = now.add(dayjs.duration(1, 'd')).isSame(date, 'd');

                            const filteredPhases = repeatingPhases.filter(
                                (p) =>
                                    !schedule || (p.scheduleUserId === schedule.userId && p.scheduleId === schedule.id)
                            );

                            if (filteredPhases.length === 0) {
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
                                                ? `Просрочено на ${DateHelper.getDifferenceString(
                                                      now,
                                                      date
                                                  )} (${date.format('L')})`
                                                : isTomorrow
                                                ? 'Завтра'
                                                : `Через ${DateHelper.getDifferenceString(date, now)} (${date.format(
                                                      'L'
                                                  )})`}
                                        </TableCell>
                                    </TableRow>
                                    {filteredPhases.map((p) => {
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

                                                    const weights = LocalStorageHelper.getRepeatingCards(
                                                        p.scheduleUserId,
                                                        p.scheduleId,
                                                        p.phaseIndex,
                                                        dateString,
                                                        c.collection.id
                                                    );

                                                    const hasSavings = Boolean(
                                                        weights && Object.values(weights).length > 0
                                                    );

                                                    return (
                                                        <CollectionRow
                                                            key={c.collection.id}
                                                            collection={c.collection}
                                                            cardsToRepeatCount={c.cardsToRepeatCount}
                                                            hover={allowFutureSelect || isToday || isWarn}
                                                            notFinished={hasSavings}
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

export const InProgressCollections: FC = () => {
    return <ConnectedInProgressCollections queryArg={undefined} />;
};
