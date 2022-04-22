import { CircularProgress } from '@mui/material';
import { FC, useState } from 'react';
import { Table, TableBody, TableCell, TableHead, TableHeaderCell, TableRow } from '../../../../controls/Table/Table';
import { useGetNotFinishedQuery } from '../../../../redux/collectionApi';
import { Collection } from '../../../../types/Collection';
import { CollectionRow } from './CollectionRow/CollectionRow';

export const NotFinishedCollections: FC = () => {
    const [page, setPage] = useState(1);
    const [collectionsCount, setCollectionsCount] = useState(30);
    const { data, isFetching, isError, isSuccess } = useGetNotFinishedQuery({ page: page, count: collectionsCount });

    if (isFetching) {
        return <CircularProgress />;
    }

    if (isError || !isSuccess) {
        return <div>ERROR</div>;
    }

    const { startedCollections, notStartedCollections } = data;
    const collections = [...startedCollections, ...notStartedCollections];

    return (
        <Table>
            <TableHead borderless>
                <TableHeaderCell>Название</TableHeaderCell>
                <TableHeaderCell>Изучено</TableHeaderCell>
                <TableHeaderCell>Слов в этапе</TableHeaderCell>
                <TableHeaderCell>Тип</TableHeaderCell>
            </TableHead>
            <TableBody>
                {collections.map((c) => (
                    <CollectionRow key={c.id} collection={c} />
                ))}
            </TableBody>
        </Table>
    );
};
