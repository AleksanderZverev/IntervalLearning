import { FC } from 'react';
import styles from './styles.module.css';
import { Remember } from '../../../../../types/Collection';
import classNames from 'classnames';
import { LightTooltip } from '../../../../../controls/LightTooltip/LightTooltip';
import { DateHelper } from '../../../../../helpers/DateHelper';
import dayjs from 'dayjs';

interface RememberListProps {
    remembers: Remember[];
}

export const RememberList: FC<RememberListProps> = ({ remembers }) => {
    if (!remembers || remembers.length == 0) {
        return <></>;
    }

    const firstRememberDate = dayjs(remembers[0].repeatedDate);

    return (
        <ul className={styles.rememberList}>
            {remembers.map((r, i) => {
                const isWarn = r.weight >= 0.3 && r.weight < 0.8;
                const isBad = r.weight < 0.3;
                const isGood = r.weight >= 0.8;

                return (
                    <LightTooltip
                        key={r.id}
                        title={
                            i === 0
                                ? ''
                                : `Через ${DateHelper.getDifferenceString(firstRememberDate, dayjs(r.repeatedDate))}`
                        }
                        placement="top"
                        style={{ cursor: i === 0 ? undefined : 'pointer' }}
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
    );
};
