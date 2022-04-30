import { FC, useState } from 'react';
import { Table, TableBody, TableCell, TableHead, TableHeaderCell, TableRow } from '../../../../controls/Table/Table';
import { withQueryResolver, WithQueryResolverData } from '../../../../hoc/withQueryResolver';
import { useGetNotFinishedQuery } from '../../../../redux/collectionApi';
import { CollectionRow } from './CollectionRow/CollectionRow';

interface CanStartCollectionsContentProps extends WithQueryResolverData<typeof useGetNotFinishedQuery> {}

const CanStartCollectionsContent: FC<CanStartCollectionsContentProps> = ({
    queryData: { startedCollections, notStartedCollections },
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

const ConnectedCanStartCollectionsContent = withQueryResolver(useGetNotFinishedQuery)(CanStartCollectionsContent);

export const CanStartCollections: FC = () => {
    const [page, setPage] = useState(1);
    const [collectionsCount, setCollectionsCount] = useState(30);

    return <ConnectedCanStartCollectionsContent queryArg={{ page, count: collectionsCount }} />;
};
