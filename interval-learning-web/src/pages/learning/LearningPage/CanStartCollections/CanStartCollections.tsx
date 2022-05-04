import { Pagination } from '@mui/material';
import { FC, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { SelectSchedule } from '../../../../controls/SelectSchedule/SelectSchedule';
import { Table, TableBody, TableCell, TableHead, TableHeaderCell, TableRow } from '../../../../controls/Table/Table';
import { withQueryResolver, WithQueryResolverData } from '../../../../hoc/withQueryResolver';
import { useGetNotFinishedQuery } from '../../../../redux/collectionApi';
import { Collection } from '../../../../types/Collection';
import { Schedule } from '../../../../types/schedule';
import { CollectionRow } from './CollectionRow/CollectionRow';

interface CanStartCollectionsContentProps extends WithQueryResolverData<typeof useGetNotFinishedQuery> {
    scheduleUserId: string;
    scheduleId: string;
    page: number;
    count: number;
    setPage: (page: number) => void;
}

const CanStartCollectionsContent: FC<CanStartCollectionsContentProps> = ({
    scheduleUserId,
    scheduleId,
    page,
    count: collectionsCount,
    setPage,
    queryData: { totalCollections, canStartCollections },
}) => {
    const collections = [...canStartCollections];

    const navigate = useNavigate();

    const onClick = (collection: Collection) => {
        navigate(
            `/learning/learn/${collection.userId}-${collection.id}?scheduleUserId=${scheduleUserId}&scheduleId=${scheduleId}`
        );
    };

    const pagesCount = Math.ceil(totalCollections / collectionsCount);
    return (
        <div
            style={{
                display: 'grid',
                gridTemplateRows: 'auto auto',
                alignContent: 'space-between',
                // minHeight: '100%',
                // height: 'auto',
                rowGap: 10,
            }}
        >
            <Table>
                <TableHead>
                    <TableHeaderCell>Название</TableHeaderCell>
                    <TableHeaderCell>Изучено</TableHeaderCell>
                    <TableHeaderCell>Слов в этапе</TableHeaderCell>
                    <TableHeaderCell>Тип</TableHeaderCell>
                </TableHead>
                <TableBody>
                    {collections.length > 0 ? (
                        collections.map((c) => <CollectionRow key={c.id} collection={c} onClick={onClick} />)
                    ) : (
                        <TableRow borderless>
                            <TableCell colSpan={4} align={'center'}>
                                Все слова изучены...
                            </TableCell>
                        </TableRow>
                    )}
                </TableBody>
            </Table>
            {pagesCount > 1 && <Pagination count={pagesCount} page={page} onChange={(e, page) => setPage(page)} />}
        </div>
    );
};

const ConnectedCanStartCollectionsContent = withQueryResolver(useGetNotFinishedQuery)(CanStartCollectionsContent);

interface CanStartCollectionsProps {}

export const CanStartCollections: FC<CanStartCollectionsProps> = ({}) => {
    const [page, setPage] = useState(1);

    const count = window ? Math.ceil((window.innerHeight - 50) / 80) : 10;
    const [schedule, setSchedule] = useState<Schedule>();

    return (
        <div style={{ display: 'grid', gridTemplateRows: 'auto 1fr' }}>
            <div style={{ display: 'flex', columnGap: 20, marginTop: 10, fontSize: '20px' }}>
                <label style={{ marginTop: 2 }}>Выберите учебный план:</label>
                <SelectSchedule
                    width="250px"
                    scheduleUserId={schedule?.userId}
                    scheduleId={schedule?.id}
                    onChange={(newSchedule) => setSchedule(newSchedule)}
                />
                {schedule && <div style={{ marginTop: 2 }}>Слов в этапе: {schedule.cardsCountPerPhase}</div>}
            </div>
            {schedule && (
                <ConnectedCanStartCollectionsContent
                    queryArg={{
                        page,
                        count,
                        scheduleUserId: schedule.userId,
                        scheduleId: schedule.id,
                    }}
                    setPage={setPage}
                />
            )}
        </div>
    );
};
