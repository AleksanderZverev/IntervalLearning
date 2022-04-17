import { CircularProgress } from '@mui/material';
import classNames from 'classnames';
import dayjs, { Dayjs } from 'dayjs';
import React, { FC, useState } from 'react';
import { Table, TableBody, TableCell, TableHead, TableHeaderCell, TableRow } from '../../../../controls/Table/Table';
import { useGetQueueCollectionsQuery } from '../../../../redux/collectionApi';
import styles from './styles.module.css';

function getDifferenceString(now: Dayjs, date: Dayjs) {
    const diffMilliseconds = now.diff(date);
    const timeStamp = dayjs.duration(diffMilliseconds);

    const diffString = timeStamp.humanize();
    return `${diffString} (${date.format('L')})`;
}

export const InProgressCollections: FC = () => {
    const { data, isFetching, isError, isSuccess } = useGetQueueCollectionsQuery();

    if (isFetching) {
        return <CircularProgress />;
    }

    if (isError || !isSuccess) {
        return <div>ERROR</div>;
    }

    const dateToCollectionsQueue = data.dateToCollectionsQueue;
    const now = dayjs();

    return (
        <div style={{ padding: '20px 50px 0' }}>
            <div className={styles.table}>
                <div className={styles.tableHeaderCell}>Название</div>
                <div className={styles.tableHeaderCell}>Изучено</div>
                <div className={styles.tableHeaderCell}>Слов в этапе</div>
                <div className={styles.tableHeaderCell}>Тип</div>
                {Object.entries(dateToCollectionsQueue).map((pair) => {
                    const [dateString, collectionsQueue] = pair;
                    const date = dayjs(dateString);
                    const isWarn = now.isAfter(date);
                    const warnMessage = getDifferenceString(now, date);
                    return (
                        <>
                            <div className={classNames(styles.subLabel, { [styles.warn]: isWarn })}>
                                {isWarn ? `Просрочено на ${warnMessage}` : date.format('dd MM')}
                            </div>
                            {collectionsQueue.map((c) => {
                                return (
                                    <React.Fragment key={c.collection.id}>
                                        <div>{c.collection.title}</div>
                                        <div>{c.cardsToRepeatCount}</div>
                                        <div>{}</div>
                                        <div>{}</div>
                                    </React.Fragment>
                                );
                            })}
                        </>
                    );
                })}
            </div>
        </div>
    );
};
