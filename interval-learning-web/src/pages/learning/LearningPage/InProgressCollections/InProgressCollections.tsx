import classNames from 'classnames';
import dayjs, { Dayjs } from 'dayjs';
import React, { FC, Fragment } from 'react';
import { useNavigate } from 'react-router-dom';
import { Table, TableBody, TableCell, TableHead, TableHeaderCell, TableRow } from '../../../../controls/Table/Table';
import { DateHelper } from '../../../../helpers/DateHelper';
import { withQueryResolver, WithQueryResolverData } from '../../../../hoc/withQueryResolver';
import { useGetQueueCollectionsQuery } from '../../../../redux/collectionApi';
import { CollectionRow } from './CollectionRow/CollectionRow';
import styles from './styles.module.css';

interface InProgressCollectionsProps extends WithQueryResolverData<typeof useGetQueueCollectionsQuery> {}

const InProgressCollectionsContent: FC<InProgressCollectionsProps> = ({ queryData }) => {
    const navigate = useNavigate();
    const dateToCollectionsQueue = queryData.dateToCollectionsQueue;
    const now = dayjs();

    return (
        <Table>
            <TableHead>
                <TableHeaderCell></TableHeaderCell>
                {/* <TableHeaderCell align="center">Изучено</TableHeaderCell> */}
                <TableHeaderCell align="center">Осталось слов</TableHeaderCell>
                <TableHeaderCell align="center">Тип</TableHeaderCell>
            </TableHead>
            <TableBody>
                {Object.entries(dateToCollectionsQueue)
                    .sort((f, s) => f[0].localeCompare(s[0]))
                    .map((pair) => {
                        const [dateString, collectionsQueue] = pair;
                        const date = dayjs(dateString);
                        const isWarn = now.isAfter(date, 'd');
                        const isToday = now.isSame(date, 'd');
                        const isTomorrow = now.add(dayjs.duration(1, 'd')).isSame(date, 'd');

                        return (
                            <Fragment key={dateString}>
                                <TableRow borderless>
                                    <TableCell
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
                                {collectionsQueue.map((c) => {
                                    return (
                                        <CollectionRow
                                            key={c.collection.id}
                                            collection={c.collection}
                                            cardsToRepeatCount={c.cardsToRepeatCount}
                                            hover={isToday || isWarn}
                                            onClick={() =>
                                                navigate(
                                                    `/learning/repeat/${c.collection.userId}-${c.collection.id}?date=${dateString}`
                                                )
                                            }
                                        />
                                    );
                                })}
                            </Fragment>
                        );
                    })}
            </TableBody>
        </Table>
    );
};

export const InProgressCollections = withQueryResolver(useGetQueueCollectionsQuery)(InProgressCollectionsContent);
