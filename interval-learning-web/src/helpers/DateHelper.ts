import dayjs, { Dayjs } from 'dayjs';

export class DateHelper {
    static getDifferenceString(now: Dayjs, date: Dayjs, prefix: string) {
        const diffMilliseconds = now.diff(date);
        const timeStamp = dayjs.duration(diffMilliseconds);

        if (Math.abs(timeStamp.asSeconds()) < 10) {
            return 'сразу';
        }

        const diffString = timeStamp.humanize();
        return prefix + ' ' + diffString;
    }
}
