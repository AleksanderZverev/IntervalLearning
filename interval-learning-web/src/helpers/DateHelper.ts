import dayjs, { Dayjs } from 'dayjs';

export class DateHelper {
    static getDifferenceString(now: Dayjs, date: Dayjs) {
        const diffMilliseconds = now.diff(date);
        const timeStamp = dayjs.duration(diffMilliseconds);

        const diffString = timeStamp.humanize();
        return diffString;
    }
}
