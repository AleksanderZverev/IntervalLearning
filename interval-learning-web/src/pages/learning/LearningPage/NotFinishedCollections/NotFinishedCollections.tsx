import { CircularProgress } from '@mui/material';
import { FC, useState } from 'react';
import { Table, TableBody, TableCell, TableHead, TableHeaderCell, TableRow } from '../../../../controls/Table/Table';
import { withQueryResolver, WithQueryResolverData } from '../../../../hoc/withQueryResolver';
import { GetNotFinishedResponse, useGetNotFinishedQuery } from '../../../../redux/collectionApi';
import { CollectionRow } from './CollectionRow/CollectionRow';

interface NotFinishedCollectionsContentProps extends WithQueryResolverData<GetNotFinishedResponse> {}

const NotFinishedCollectionsContent: FC<NotFinishedCollectionsContentProps> = ({
    resolverData: { startedCollections, notStartedCollections },
}) => {
    const collections = [...startedCollections, ...notStartedCollections];

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
                    collections.map((c) => <CollectionRow key={c.id} collection={c} />)
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

const ConnectedNotFinishedCollectionsContent = withQueryResolver(useGetNotFinishedQuery)(NotFinishedCollectionsContent);

export const NotFinishedCollections: FC = () => {
    const [page, setPage] = useState(1);
    const [collectionsCount, setCollectionsCount] = useState(30);

    return <ConnectedNotFinishedCollectionsContent queryArg={{ page, count: collectionsCount }} />;
};
