import { Folder } from '@mui/icons-material';
import { Button } from '@mui/material';
import Head from 'next/head';
import { FC, useState } from 'react';
import { CreateCollectionModal } from '../../../controls/Modals/CreateCollectionModal';
import { PageContainer } from '../../../controls/PageContainer/PageContainer';
import { PageHeader } from '../../../controls/PageHeader/PageHeader';
import { Table, TableHead, TableHeaderCell, TableBody } from '../../../controls/Table/Table';
import { withQueryResolver } from '../../../hoc/withQueryResolver';
import useTypedSelector from '../../../hooks/useTypedSelector';
import { useGetCollectionsQuery } from '../../../redux/collectionApi';
import { selectCollections } from '../../../redux/slices/collectionsSlice';
import { CollectionRow } from './CollectionRow';

const pageTitle = 'Мои коллекции';

const CollectionsPageContent: FC = () => {
    const [showCreateCollectionModal, setShowCreateCollectionModal] = useState(false);
    const collections = useTypedSelector(selectCollections);

    return (
        <PageContainer>
            <PageHeader
                title={pageTitle}
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
            <div>
                {showCreateCollectionModal && (
                    <CreateCollectionModal
                        open={showCreateCollectionModal}
                        onClose={() => setShowCreateCollectionModal(false)}
                    />
                )}
                <Table>
                    <TableHead>
                        <TableHeaderCell>Название</TableHeaderCell>
                        <TableHeaderCell></TableHeaderCell>
                        <TableHeaderCell align="center">Слов</TableHeaderCell>
                        <TableHeaderCell align="center">Создана</TableHeaderCell>
                    </TableHead>
                    <TableBody>
                        {collections.map((c) => (
                            <CollectionRow key={c.id} collection={c} />
                        ))}
                    </TableBody>
                </Table>
            </div>
        </PageContainer>
    );
};

const ConnectedCollection = withQueryResolver(useGetCollectionsQuery)(CollectionsPageContent);

export const CollectionsPage: FC = () => {
    return (
        <>
            <Head>
                <title>{pageTitle}</title>
            </Head>
            <ConnectedCollection queryArg={undefined} />
        </>
    );
};
