import classNames from 'classnames';
import dayjs from 'dayjs';
import React, { FC, Fragment, useEffect } from 'react';
import { SelectSchedule } from '../../../../controls/SelectSchedule/SelectSchedule';
import { Table, TableBody, TableCell, TableHead, TableHeaderCell, TableRow } from '../../../../controls/Table/Table';
import { withQueryResolver, WithQueryResolverData } from '../../../../hoc/withQueryResolver';
import { useLocalStorageValue } from '../../../../hooks/useLocalStorageValue';
import { RepeatingCollectionInfoDto, useGetQueueCollectionsV2Query } from '../../../../redux/collectionApi';
import { Schedule } from '../../../../types/schedule';
import styles from './styles.module.css';
import { SelectTheme } from '../../../../controls/SelectTheme/SelectTheme';
import { Theme } from '../../../../types/global';
import useTypedSelector from '../../../../hooks/useTypedSelector';
import { selectThemes } from '../../../../redux/slices/themeSlice';
import { useGetStatisticQuery } from '../../../../redux/api/statisticsApi';
import _ from 'lodash';
import { EnvironmentHelper } from '../../../../helpers/EnvironmentHelper';
import { RepeatingDateInfo } from './RepeatingDateInfo/RepeatingDateInfo';
import { PhaseHelper } from '../../../../helpers/Study/PhaseHelper';
import { Divider, Stack } from '@mui/material';

function CountWordsByThemes(repeatingCollectionInfos: RepeatingCollectionInfoDto[]): Map<number, number> {
    const result: Map<number, number> = new Map();

    const addToTheme = (themeId: number, cardsCount: number) => {
        if (!result.has(themeId)) {
            result.set(themeId, 0);
        }

        const oldCount = result.get(themeId) || 0;
        result.set(themeId, oldCount + cardsCount);
    };

    repeatingCollectionInfos.forEach((collectionInfo) => {
        const now = dayjs();
        const date = dayjs(collectionInfo.earliestDateToRepeat);
        const isFuture = date.isAfter(now, 'd');

        if (isFuture) {
            return;
        }

        addToTheme(collectionInfo.themeId, collectionInfo.cardsCount);
    });

    return result;
}

function IsCollectionsOfTheme(collectionInfos: RepeatingCollectionInfoDto[], theme: Theme | null): boolean {
    const phases = collectionInfos.filter((p) => {
        if (!theme) return true;
        return p.themeId === theme.id;
    });
    return phases.length > 0;
}

interface InProgressCollectionsProps extends WithQueryResolverData<typeof useGetQueueCollectionsV2Query> {
    schedule: Schedule;
    theme: Theme | null;
}

interface LocalStorageState {
    schedule: Schedule | undefined;
    theme: Theme | null;
}

const InProgressCollectionsContent: FC<InProgressCollectionsProps> = ({ queryData, schedule, theme }) => {
    const now = dayjs();
    const themes = useTypedSelector((state) => selectThemes(state));

    const repeatingInfos = _.chain(queryData.repeatingInfosByDate)
        .orderBy((i) => dayjs(i.date))
        .filter((i) => IsCollectionsOfTheme(i.repeatingCollections, theme))
        .value();
    const lateCollections = _.filter(queryData.lateCollections, (i) => i.themeId === theme?.id);
    const repeatingForgottenWordsCollections = _.filter(
        queryData.repeatingForgottenWordsCollections,
        (i) => i.themeId === theme?.id
    );

    const collectionsToRepeatTodayOrInPastByAllThemes = [
        ...queryData.lateCollections,
        ...queryData.repeatingForgottenWordsCollections,
        ..._.chain(queryData.repeatingInfosByDate)
            .filter((i) => dayjs(i.date).isSame(now, 'date'))
            .flatMap((t) => t.repeatingCollections)
            .value(),
    ];
    const wordByThemes = CountWordsByThemes(collectionsToRepeatTodayOrInPastByAllThemes);

    const todayAndFutureCollections = _.chain(repeatingInfos)
        .filter((info) => {
            const dateObj = dayjs(info.date);
            return dateObj.isSame(now, 'date') || dateObj.isAfter(now, 'date');
        })
        .value();

    return (
        <Table>
            <TableHead>
                <TableHeaderCell></TableHeaderCell>
                <TableHeaderCell align="center">Осталось слов</TableHeaderCell>
                <TableHeaderCell align="center">Тип</TableHeaderCell>
            </TableHead>
            <TableBody>
                {theme && repeatingInfos.length === 0 && collectionsToRepeatTodayOrInPastByAllThemes.length > 0 && (
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
                {lateCollections && lateCollections.length > 0 && (
                    <RepeatingDateInfo
                        schedule={schedule}
                        date={now.add(-1, 'd').toISOString()}
                        isLateCollections
                        repeatingCollections={lateCollections}
                    />
                )}
                {repeatingForgottenWordsCollections && repeatingForgottenWordsCollections.length > 0 && (
                    <RepeatingDateInfo
                        schedule={schedule}
                        isRepeatingForgottenWordsCollections
                        date={now.toISOString()}
                        repeatingCollections={repeatingForgottenWordsCollections}
                        shouldRenderCollection={(c) => c.isRepeatingForgottenWords}
                    />
                )}
                {todayAndFutureCollections &&
                    todayAndFutureCollections.length > 0 &&
                    todayAndFutureCollections.map((i) => (
                        <RepeatingDateInfo
                            key={`${i.date}-dated`}
                            schedule={schedule}
                            date={i.date}
                            repeatingCollections={i.repeatingCollections}
                        />
                    ))}
            </TableBody>
        </Table>
    );
};

const ConnectedInProgressCollections = withQueryResolver(useGetQueueCollectionsV2Query)(InProgressCollectionsContent);

export const InProgressCollections: FC = () => {
    const untilDate = EnvironmentHelper.IsProduction() ? dayjs().add(40, 'days').toISOString() : undefined;

    const [localState, setLocalState] = useLocalStorageValue<LocalStorageState>('InProgressCollectionsContent-state', {
        schedule: undefined,
        theme: null,
    });

    if (!localState) throw new Error();

    const { schedule, theme } = localState;

    const setSchedule = (newSchedule: Schedule | undefined) => setLocalState({ ...localState, schedule: newSchedule });
    const setTheme = (newTheme: Theme | null) => setLocalState({ ...localState, theme: newTheme });

    const {
        data: statistics,
        isLoading: isStatisticLoading,
        refetch,
    } = useGetStatisticQuery(
        {
            date: dayjs().startOf('s').toISOString(),
            userCurrentDateTime: dayjs().startOf('s').toISOString(),
            scheduleUserId: schedule?.userId || '',
            scheduleId: schedule?.id || '',
            themeId: theme?.id || 0,
        },
        { skip: _.isNil(schedule) || _.isNil(theme) }
    );

    useEffect(() => {
        if (_.isNil(schedule) || _.isNil(theme)) return;
        if (statistics) return;

        refetch();
    }, [schedule, theme]);

    const totalLateCards = !statistics
        ? 0
        : _.chain(statistics.phaseIdToStatistic)
              .values()
              .sumBy((s) => s.lateCards)
              .value();
    const totalNotLateCards = !statistics ? 0 : statistics.totalRepeatingCards - totalLateCards;
    return (
        <div style={{ display: 'grid', gridTemplateRows: 'auto 1fr', rowGap: '8px' }}>
            <div style={{ display: 'flex', columnGap: '20px', marginTop: 10, fontSize: '20px' }}>
                <label style={{ marginTop: 2 }}>Сортировать по учебному плану:</label>
                <SelectSchedule
                    width="250px"
                    scheduleUserId={schedule?.userId}
                    scheduleId={schedule?.id}
                    onChange={(newSchedule) => setSchedule(newSchedule)}
                />
                <label>Тема:</label>
                <SelectTheme value={theme} onChange={setTheme} />
            </div>
            <Stack gap={'4px'}>
                {statistics && statistics.totalRepeatingCards > 0 && schedule && (
                    <div className={styles.statisticTable} style={{ color: '#b7b7b7' }}>
                        {/* <span>(по этапам: </span> */}
                        <thead>
                            {_.entries(statistics.phaseIdToStatistic).map(([phaseId, cardsCount], i, a) => {
                                var phase = _.find(schedule.phases, (p) => p.id === phaseId);
                                if (!phase) {
                                    console.error(`Phase with id ${phaseId} was not found`);
                                    return null;
                                }

                                const daysDuration = dayjs.duration(phase.secondsFromLastPhase, 's').days();

                                return (
                                    <td
                                        className="cell"
                                        key={phaseId + '-stat'}
                                        style={{
                                            textAlign: 'center',
                                            border: '1px solid #b7b7b7',
                                        }}
                                    >
                                        {daysDuration} дн
                                    </td>
                                );
                            })}
                            <td
                                style={{
                                    textAlign: 'center',
                                    border: '1px solid #b7b7b7',
                                }}
                            >
                                Всего
                            </td>
                        </thead>
                        <tbody>
                            <tr>
                                {_.entries(statistics.phaseIdToStatistic).map(([phaseId, phaseStat], i, a) => {
                                    const todayAndFutureCards = phaseStat.todayCards + phaseStat.futureCards;
                                    return (
                                        <td
                                            key={phaseId + '-days'}
                                            style={{
                                                textAlign: 'center',
                                                border: '1px solid #b7b7b7',
                                            }}
                                        >
                                            {todayAndFutureCards > 0 && <span>{todayAndFutureCards}</span>}

                                            {todayAndFutureCards <= 0 && (
                                                <span className={styles.warn}>{phaseStat.lateCards}</span>
                                            )}
                                            {todayAndFutureCards > 0 && phaseStat.lateCards > 0 && (
                                                <span className={styles.warn}> ({phaseStat.lateCards})</span>
                                            )}
                                        </td>
                                    );
                                })}
                                <td
                                    key={'total'}
                                    style={{
                                        textAlign: 'center',
                                        border: '1px solid #b7b7b7',
                                    }}
                                >
                                    {totalNotLateCards > 0 && <span>{totalNotLateCards}</span>}
                                    {totalNotLateCards <= 0 && <span className={styles.warn}>{totalLateCards}</span>}
                                    {totalLateCards > 0 && totalNotLateCards > 0 && (
                                        <span className={styles.warn}> ({totalLateCards})</span>
                                    )}
                                </td>
                            </tr>
                        </tbody>
                    </div>
                )}
                <div className={styles.statisticRow}>
                    {statistics?.learnedCards !== undefined && statistics.learnedCards > 0 && (
                        <span className={styles.learned}>Изучено: {statistics.learnedCards}</span>
                    )}
                    {statistics?.repeatedCards !== undefined && statistics.repeatedCards > 0 && (
                        <Fragment>
                            {(statistics?.learnedCards !== undefined && statistics.learnedCards > 0) ||
                            statistics?.totalRepeatingCards !== undefined ? (
                                <Divider orientation="vertical" />
                            ) : null}
                            <span className={styles.repeated}>Повторено: {statistics.repeatedCards}</span>
                        </Fragment>
                    )}
                </div>
            </Stack>
            {!schedule && <div>Не найдено</div>}
            {schedule && (
                <ConnectedInProgressCollections
                    schedule={schedule}
                    theme={theme}
                    queryArg={{
                        untilDate: untilDate,
                        scheduleUserId: schedule.userId,
                        scheduleId: schedule.id,
                        userCurrentDateTime: dayjs().startOf('d').format(),
                    }}
                />
            )}
        </div>
    );
};
