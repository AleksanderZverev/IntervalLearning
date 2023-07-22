import { Folder } from '@mui/icons-material';
import { Button } from '@mui/material';
import Head from 'next/head';
import { FC, Fragment, useState } from 'react';
import { CreateCollectionModal } from '../../../controls/Modals/CreateCollectionModal';
import { PageContainer } from '../../../controls/PageContainer/PageContainer';
import { PageHeader } from '../../../controls/PageHeader/PageHeader';
import { Table, TableHead, TableHeaderCell, TableBody, TableRow, TableCell } from '../../../controls/Table/Table';
import { withQueryResolver } from '../../../hoc/withQueryResolver';
import useTypedSelector from '../../../hooks/useTypedSelector';
import { useGetCollectionsQuery } from '../../../redux/collectionApi';
import { selectCollections } from '../../../redux/slices/collectionsSlice';
import { CollectionRow } from './CollectionRow';
import { selectThemes } from '../../../redux/slices/themeSlice';
import { Collection } from '../../../types/Collection';
import styles from './styles.module.css';
import _ from 'lodash';

const pageTitle = 'Мои коллекции';

const CollectionsPageContent: FC = () => {
    const [showCreateCollectionModal, setShowCreateCollectionModal] = useState(false);
    const collections = useTypedSelector(selectCollections);
    const themes = useTypedSelector(selectThemes);

    const themeToCollections = _.chain(collections)
        .groupBy((c) => c.themeId)
        .value();
    const themeIds = _.keys(themeToCollections);

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
                        {themeIds.map((themeId) => {
                            const theme = themes.find((t) => t.id.toString() === themeId);
                            const collections = themeToCollections[themeId];
                            return (
                                <Fragment key={themeId}>
                                    <TableRow borderless>
                                        <TableCell className={styles.subLabel}>{theme?.name}</TableCell>
                                    </TableRow>
                                    {collections.map((c) => {
                                        return <CollectionRow key={c.id} collection={c} />;
                                    })}
                                </Fragment>
                            );
                        })}
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
