import classNames from 'classnames';
import dayjs from 'dayjs';
import React, { FC, Fragment } from 'react';
import { SelectSchedule } from '../../../../controls/SelectSchedule/SelectSchedule';
import { Table, TableBody, TableCell, TableHead, TableHeaderCell, TableRow } from '../../../../controls/Table/Table';
import { withQueryResolver, WithQueryResolverData } from '../../../../hoc/withQueryResolver';
import { useLocalStorageValue } from '../../../../hooks/useLocalStorageValue';
import { RepeatingInfoByDateDto, useGetQueueCollectionsV2Query } from '../../../../redux/collectionApi';
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

function CountWordsByThemes(
    schedule: Schedule | undefined,
    repeatingInfosByDate: RepeatingInfoByDateDto[]
): Map<number, number> {
    const result: Map<number, number> = new Map();

    const addToTheme = (themeId: number, cardsCount: number) => {
        if (!result.has(themeId)) {
            result.set(themeId, 0);
        }

        const oldCount = result.get(themeId) || 0;
        result.set(themeId, oldCount + cardsCount);
    };

    repeatingInfosByDate.forEach((infoByDate) => {
        const now = dayjs();
        const date = dayjs(infoByDate.date);
        const isFuture = date.isAfter(now, 'd');

        if (isFuture) {
            return;
        }

        infoByDate.repeatingCollections.forEach((c) => {
            addToTheme(c.themeId, c.cardsCount);
        });
    });

    return result;
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
    const repeatingInfos = _.chain(queryData.repeatingInfosByDate)
        .orderBy((i) => dayjs(i.date))
        .value();
    const lateCollections = queryData.lateCollections;
    const repeatingForgottenWordsCollections = queryData.repeatingForgottenWordsCollections;

    const now = dayjs();
    const themes = useTypedSelector((state) => selectThemes(state));

    const repeatingInfosByCurrentTheme = repeatingInfos.filter((collection) => {
        const phases = collection.repeatingCollections.filter((p) => {
            if (!theme) return true;
            return p.themeId === theme.id;
        });
        return phases.length > 0;
    });

    const wordByThemes = CountWordsByThemes(schedule, repeatingInfos);

    const todayAndFutureCollections = _.chain(repeatingInfos)
        .filter((info) => dayjs(info.date).isAfter(now, 'date'))
        .value();

    return (
        <Table>
            <TableHead>
                <TableHeaderCell></TableHeaderCell>
                <TableHeaderCell align="center">Осталось слов</TableHeaderCell>
                <TableHeaderCell align="center">Тип</TableHeaderCell>
            </TableHead>
            <TableBody>
                {theme && repeatingInfos.length > 0 && repeatingInfosByCurrentTheme.length === 0 && (
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
                        date={now.subtract(1, 'D').toISOString()}
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

    const { data: statistics } = useGetStatisticQuery({ date: dayjs().startOf('s').toISOString() });

    const [localState, setLocalState] = useLocalStorageValue<LocalStorageState>('InProgressCollectionsContent-state', {
        schedule: undefined,
        theme: null,
    });

    if (!localState) throw new Error();

    const { schedule, theme } = localState;

    const setSchedule = (newSchedule: Schedule | undefined) => setLocalState({ ...localState, schedule: newSchedule });
    const setTheme = (newTheme: Theme | null) => setLocalState({ ...localState, theme: newTheme });

    const hasStatistic = Boolean(statistics?.learnedCards) || Boolean(statistics?.repeatedCards);

    return (
        <div style={{ display: 'grid', gridTemplateRows: 'auto 1fr' }}>
            <div style={{ display: 'flex', columnGap: 20, marginTop: 10, fontSize: '20px' }}>
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
            {hasStatistic ? (
                <div className={styles.statisticRow}>
                    {statistics?.learnedCards !== undefined && statistics.learnedCards > 0 && (
                        <span className={styles.learned}>Изучено {statistics.learnedCards}</span>
                    )}
                    {statistics?.repeatedCards !== undefined && statistics.repeatedCards > 0 && (
                        <span className={styles.repeated}>Повторено {statistics.repeatedCards}</span>
                    )}
                </div>
            ) : (
                <div />
            )}

            {!schedule && <div>Не найдено</div>}
            {schedule && (
                <ConnectedInProgressCollections
                    schedule={schedule}
                    theme={theme}
                    queryArg={{
                        untilDate: untilDate,
                        scheduleUserId: schedule.userId,
                        scheduleId: schedule.id,
                        userCurrentDateTime: dayjs().startOf('D').format(),
                    }}
                />
            )}
        </div>
    );
};
