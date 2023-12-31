import { FC } from 'react';
import styles from './styles.module.css';
import { Remember } from '../../../../../types/Collection';
import classNames from 'classnames';
import { LightTooltip } from '../../../../../controls/LightTooltip/LightTooltip';
import { DateHelper } from '../../../../../helpers/DateHelper';
import dayjs from 'dayjs';
import _ from 'lodash';

interface RememberListProps {
    remembers: Remember[];
}

export const RememberList: FC<RememberListProps> = ({ remembers }) => {
    if (!remembers || remembers.length == 0) {
        return <></>;
    }

    const getLastRemember = (remember: Remember): Remember | null => {
        const currentIndex = remembers.findIndex((r) => r.id === remember.id);
        const previousIndex = currentIndex - 1;
        return previousIndex >= 0 && previousIndex < remembers.length ? remembers[previousIndex] : null;
    };

    const orderedByDates = _.chain(remembers)
        .groupBy((r) => dayjs(r.repeatedDate).format('DD.MM.YYYY'))
        .value();

    const dates = _.keys(orderedByDates);

    return (
        <ul className={styles.rememberList}>
            {dates.map((date) => {
                const remembers = orderedByDates[date];
                return (
                    <li key={date}>
                        <ul className={styles.dateList}>
                            {remembers.map((r) => {
                                const isWarn = r.weight >= 0.3 && r.weight < 0.8;
                                const isBad = r.weight < 0.3;
                                const isGood = r.weight >= 0.8;

                                const date = dayjs(r.repeatedDate);
                                const lastRemember = getLastRemember(r);
                                const lastRememberDate = lastRemember ? dayjs(lastRemember.repeatedDate) : null;

                                return (
                                    <LightTooltip
                                        key={r.id}
                                        title={
                                            <div className={styles.tooltip}>
                                                <div>
                                                    <span>{date.format('DD.MM.YYYY HH:mm')}</span>
                                                    {lastRememberDate && (
                                                        <span>{` (${DateHelper.getDifferenceString(
                                                            lastRememberDate,
                                                            dayjs(r.repeatedDate),
                                                            ''
                                                        )})`}</span>
                                                    )}
                                                </div>

                                                {r.comment && <b>{r.comment}</b>}
                                            </div>
                                        }
                                        placement="right"
                                        style={{ cursor: Boolean(lastRemember) ? 'pointer' : undefined }}
                                    >
                                        <li
                                            className={classNames({
                                                [styles.good]: isGood,
                                                [styles.warn]: isWarn,
                                                [styles.bad]: isBad,
                                            })}
                                        ></li>
                                    </LightTooltip>
                                );
                            })}
                        </ul>
                    </li>
                );
            })}
        </ul>
    );
};
