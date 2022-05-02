import { FC, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { FormFiledLabel } from '../../../../controls/Form/Form';
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
}

const CanStartCollectionsContent: FC<CanStartCollectionsContentProps> = ({
    scheduleUserId,
    scheduleId,
    queryData: { canStartCollections },
}) => {
    const collections = [...canStartCollections];

    const navigate = useNavigate();

    const onClick = (collection: Collection) => {
        navigate(
            `/learning/learn/${collection.userId}-${collection.id}?scheduleUserId=${scheduleUserId}&scheduleId=${scheduleId}`
        );
    };

    return (
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
    );
};

const ConnectedCanStartCollectionsContent = withQueryResolver(useGetNotFinishedQuery)(CanStartCollectionsContent);

interface CanStartCollectionsProps {}

export const CanStartCollections: FC<CanStartCollectionsProps> = ({}) => {
    const [page, setPage] = useState(1);
    const [collectionsCount, setCollectionsCount] = useState(30);
    const [schedule, setSchedule] = useState<Schedule>();

    return (
        <>
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
                        count: collectionsCount,
                        scheduleUserId: schedule.userId,
                        scheduleId: schedule.id,
                    }}
                />
            )}
        </>
    );
};
