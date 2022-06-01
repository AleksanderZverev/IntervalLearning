import dayjs from 'dayjs';
import { FC } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { ShowScheduleModal } from '../../../controls/Modals/ShowScheduleModal';
import { PageContainer } from '../../../controls/PageContainer/PageContainer';
import { PageContent } from '../../../controls/PageContent/PageContent';
import { PageHeader } from '../../../controls/PageHeader/PageHeader';
import { Table, TableBody, TableCell, TableHead, TableHeaderCell, TableRow } from '../../../controls/Table/Table';
import { withQueryResolver, WithQueryResolverData } from '../../../hoc/withQueryResolver';
import { useGetSchedulesQuery } from '../../../redux/schedulesSlice';
import { Schedule } from '../../../types/schedule';

export function getIntervals(schedule: Schedule): string {
    return [...schedule.phases]
        .sort((f, s) => parseInt(f.id) - parseInt(s.id))
        .map((p) => {
            if (p.secondsFromLastPhase <= 10) {
                return '';
            }

            const duration = dayjs.duration(p.secondsFromLastPhase, 's');
            return duration.humanize();
        })
        .filter(Boolean)
        .join(' - ');
}

interface ScheduleListPageContentProps extends WithQueryResolverData<typeof useGetSchedulesQuery> {}

const ScheduleListPageContent: FC<ScheduleListPageContentProps> = ({ queryData: schedules }) => {
    const navigate = useNavigate();
    const params = new URLSearchParams(location.search);
    const scheduleId = params.get('scheduleId');
    const schedule = scheduleId ? schedules.find((s) => s.id === scheduleId) : undefined;

    return (
        <PageContainer>
            <PageHeader title="Мои учебные планы" />
            <PageContent>
                {schedule && <ShowScheduleModal open onClose={() => navigate('')} schedule={schedule} />}
                <Table>
                    <TableHead>
                        <TableHeaderCell>Название</TableHeaderCell>
                        <TableHeaderCell>Интервалы</TableHeaderCell>
                    </TableHead>
                    <TableBody>
                        {schedules.map((s) => (
                            <TableRow key={s.id} hover onClick={() => navigate(`?scheduleId=${s.id}`)}>
                                <TableCell>{s.title}</TableCell>
                                <TableCell>{getIntervals(s)}</TableCell>
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </PageContent>
        </PageContainer>
    );
};

const WithGetSchedulesQuery = withQueryResolver(useGetSchedulesQuery)(ScheduleListPageContent);

export const ScheduleListPage: FC = () => {
    return <WithGetSchedulesQuery queryArg={undefined} />;
};
