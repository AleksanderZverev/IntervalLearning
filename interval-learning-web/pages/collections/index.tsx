import { Folder } from '@mui/icons-material';
import { Button } from '@mui/material';
import { FC, useState } from 'react';
import { CreateCollectionModal } from '../../src/controls/Modals/CreateCollectionModal';
import { PageContainer } from '../../src/controls/PageContainer/PageContainer';
import { PageHeader } from '../../src/controls/PageHeader/PageHeader';
import { Table, TableBody, TableCell, TableHead, TableHeaderCell, TableRow } from '../../src/controls/Table/Table';
import { withQueryResolver } from '../../src/hoc/withQueryResolver';
import useTypedSelector from '../../src/hooks/useTypedSelector';
import { useGetCollectionsQuery } from '../../src/redux/collectionApi';
import { selectCollections } from '../../src/redux/slices/collectionsSlice';
import styles from './collections.module.css';

const CollectionsPageContent: FC = () => {
    const [showCreateCollectionModal, setShowCreateCollectionModal] = useState(false);
    const collections = useTypedSelector(selectCollections);

    return (
        <PageContainer>
            <PageHeader
                title="Мои коллекции"
                subMenu={
                    <Button
                        onClick={() => setShowCreateCollectionModal(true)}
                        variant={'contained'}
                        endIcon={<Folder />}
                    >
                        Создать
                    </Button>
                }
            />
            <div style={{ padding: '20px 50px 0' }}>
                {showCreateCollectionModal && (
                    <CreateCollectionModal
                        open={showCreateCollectionModal}
                        onClose={() => setShowCreateCollectionModal(false)}
                    />
                )}
                <Table>
                    <TableHead>
                        <TableHeaderCell>Название</TableHeaderCell>
                        <TableHeaderCell align="center">След. повторение</TableHeaderCell>
                        <TableHeaderCell align="center">Слов</TableHeaderCell>
                        <TableHeaderCell align="center">Создана</TableHeaderCell>
                    </TableHead>
                    <TableBody>
                        {collections.map((c) => {
                            const date = new Date(c.createdAt);
                            return (
                                <TableRow key={c.id}>
                                    {/* <div key={c.id}>{c.title}</div> */}
                                    <TableCell>{c.title}</TableCell>
                                    <TableCell align="center">-</TableCell>
                                    <TableCell align="center">{c.cards.length}</TableCell>
                                    <TableCell align="center">{date.toLocaleDateString()}</TableCell>
                                </TableRow>
                            );
                        })}
                    </TableBody>
                </Table>
            </div>
        </PageContainer>
    );
};

const CollectionsPage = withQueryResolver(useGetCollectionsQuery)(CollectionsPageContent);
//const AuthorizationPage = withAuthorization(CollectionsPage);

(CollectionsPage as any).auth = true;

export default CollectionsPage;
