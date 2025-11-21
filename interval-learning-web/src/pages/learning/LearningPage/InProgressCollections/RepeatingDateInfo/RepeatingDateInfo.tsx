import { FC, Fragment } from 'react';
import _ from 'lodash';
import styles from '../styles.module.css';
import { RepeatingCollectionInfoDto, RepeatingInfoByDateDto } from '../../../../../redux/collectionApi';
import dayjs from 'dayjs';
import { TableCell, TableRow } from '../../../../../controls/Table/Table';
import classNames from 'classnames';
import { DateHelper } from '../../../../../helpers/DateHelper';
import { getRepeatingCards } from '../../../RepeatCollectionPage/RepeatCollectionPage.logic';
import { Schedule } from '../../../../../types/schedule';
import { CollectionRow } from '../CollectionRow/CollectionRow';
import { useNavigate } from 'react-router-dom';

export const getRepeatingNavigationLink = (
    collectionUserId: string,
    collectionId: string,
    scheduleUserId: string,
    scheduleId: string,
    dateString: string,
    isRepeatingForgottenWords: boolean
): string => {
    const searchParams = new URLSearchParams({
        scheduleUserId,
        scheduleId,
        date: dateString,
        isRepeatingForgottenWords: isRepeatingForgottenWords.toString(),
    });

    const link = `/learning/repeat/${collectionUserId}-${collectionId}?` + searchParams.toString();
    return link;
};

const isRepeatingInProgress = (
    scheduleUserId: string,
    scheduleId: string,
    date: string,
    collectionId: string
): boolean => {
    const state = getRepeatingCards(scheduleUserId, scheduleId, date, collectionId);
    return Boolean(
        state?.rememberWeights &&
            Object.values(state.rememberWeights).filter((w) => typeof w?.weight === 'number').length > 0
    );
};

interface RepeatingDateInfoProps {
    schedule: Schedule;
    date: string;
    repeatingCollections: RepeatingCollectionInfoDto[];
    shouldRenderCollection?: (info: RepeatingCollectionInfoDto) => boolean;
    isLateCollections?: boolean;
    isRepeatingForgottenWordsCollections?: boolean;
}

export const RepeatingDateInfo: FC<RepeatingDateInfoProps> = ({
    schedule,
    date: dateString,
    repeatingCollections: collections,
    shouldRenderCollection,
    isLateCollections,
    isRepeatingForgottenWordsCollections,
}) => {
    const navigate = useNavigate();
    const allowFutureSelect = process.env.NODE_ENV === 'development';

    if (!collections || collections.length === 0) return null;

    const now = dayjs();
    const date = dayjs(dateString);
    const isToday = !isLateCollections && now.isSame(date, 'd');
    const isTomorrow = now.add(dayjs.duration(1, 'd')).isSame(date, 'd');

    const totalWords = _.sumBy(collections, (c) => c.cardsCount);

    return (
        <Fragment key={`${dateString}-date-info`}>
            <TableRow borderless>
                <TableCell
                    className={classNames(styles.subLabel, {
                        [styles.warn]: isLateCollections,
                        [styles.today]: isToday,
                    })}
                    colSpan={4}
                    fontSize={14}
                >
                    {isToday
                        ? `Сегодня (${totalWords})`
                        : isLateCollections
                        ? 'Просрочено'
                        : isTomorrow
                        ? 'Завтра'
                        : `${date.format('L')} (${DateHelper.getDifferenceString(date, now, 'Через')})`}
                </TableCell>
            </TableRow>
            {collections.map((c) => {
                if (shouldRenderCollection && !shouldRenderCollection(c)) return null;

                const repeatingLink = getRepeatingNavigationLink(
                    c.collectionUserId,
                    c.collectionId,
                    schedule.userId,
                    schedule.id,
                    dateString,
                    c.isRepeatingForgottenWords
                );

                const isInProgress = isRepeatingInProgress(schedule.userId, schedule.id, dateString, c.collectionId);

                const isRepeatable = Boolean(c.isRepeatable);

                return (
                    <Fragment key={`${dateString}-${c.collectionId}`}>
                        <CollectionRow
                            key={c.collectionId}
                            themeId={c.themeId}
                            hover={allowFutureSelect || isRepeatable}
                            collectionTitle={c.collectionTitle}
                            cardsToRepeatCount={c.cardsCount}
                            notFinished={isInProgress}
                            onClick={() => navigate(repeatingLink)}
                        />
                    </Fragment>
                );
            })}
        </Fragment>
    );
};
