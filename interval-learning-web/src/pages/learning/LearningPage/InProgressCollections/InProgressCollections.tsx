import { CircularProgress } from '@mui/material';
import classNames from 'classnames';
import dayjs, { Dayjs } from 'dayjs';
import React, { FC, Fragment } from 'react';
import { useNavigate } from 'react-router-dom';
import { Table, TableBody, TableCell, TableHead, TableHeaderCell, TableRow } from '../../../../controls/Table/Table';
import { useGetQueueCollectionsQuery } from '../../../../redux/collectionApi';
import { CollectionRow } from './CollectionRow/CollectionRow';
import styles from './styles.module.css';

function getDifferenceString(now: Dayjs, date: Dayjs) {
    const diffMilliseconds = now.diff(date);
    const timeStamp = dayjs.duration(diffMilliseconds);

    const diffString = timeStamp.humanize();
    return `${diffString} (${date.format('L')})`;
}

export const InProgressCollections: FC = () => {
    const { data, isFetching, isError, isSuccess } = useGetQueueCollectionsQuery();

    const navigate = useNavigate();

    if (isFetching) {
        return <CircularProgress />;
    }

    if (isError || !isSuccess) {
        return <div>ERROR</div>;
    }
    const dateToCollectionsQueue = data.dateToCollectionsQueue;
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
                {Object.entries(dateToCollectionsQueue).map((pair) => {
                    const [dateString, collectionsQueue] = pair;
                    const date = dayjs(dateString);
                    const isWarn = now.isAfter(date, 'd');
                    const isToday = now.isSame(date, 'd');
                    const isTomorrow = now.add(dayjs.duration(1, 'd')).isSame(date, 'd');

                    return (
                        <Fragment key={dateString}>
                            <TableRow borderless>
                                <TableCell
                                    className={classNames({ [styles.warn]: isWarn, [styles.today]: isToday })}
                                    colSpan={4}
                                    fontSize={14}
                                >
                                    {isToday
                                        ? 'Сегодня'
                                        : isWarn
                                        ? `Просрочено на ${getDifferenceString(now, date)}`
                                        : isTomorrow
                                        ? 'Завтра'
                                        : `Через ${getDifferenceString(date, now)}`}
                                </TableCell>
                            </TableRow>
                            {collectionsQueue.map((c) => {
                                return (
                                    <CollectionRow
                                        key={c.collection.id}
                                        collection={c.collection}
                                        cardsToRepeatCount={c.cardsToRepeatCount}
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
