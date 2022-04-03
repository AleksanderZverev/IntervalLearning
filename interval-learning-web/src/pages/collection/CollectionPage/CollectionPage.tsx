/* eslint-disable react-hooks/rules-of-hooks */
import { Add } from '@mui/icons-material';
import { Button, CircularProgress, Pagination } from '@mui/material';
import { FC, useState } from 'react';
import { useParams } from 'react-router-dom';
import { CreateCardModal } from '../../../controls/Modals/CreateCardModal';
import { PageContainer } from '../../../controls/PageContainer/PageContainer';
import { PageHeader } from '../../../controls/PageHeader/PageHeader';
import { Table, TableBody, TableHead, TableHeaderCell } from '../../../controls/Table/Table';
import useTypedSelector from '../../../hooks/useTypedSelector';
import { useGetCardsQuery } from '../../../redux/cardsApi';
import { useGetCollectionQuery } from '../../../redux/collectionApi';
import { selectCards } from '../../../redux/slices/cardsSlice';
import { selectCollectionById } from '../../../redux/slices/collectionsSlice';
import { getScheduleId, selectScheduleById } from '../../../redux/slices/scheduleSlice';
import { CardRow } from './CardRow';

const cardsCountPerPage = 50;

export const CollectionPage: FC = () => {
    const { userId, collectionId } = useParams();

    if (!collectionId || !userId) {
        throw new Error();
    }

    const [page, setPage] = useState(1);

    const { isFetching: isCollectionFetching, isError: isCollectionError } = useGetCollectionQuery(collectionId);
    const { isFetching: isCardsFetching, isError: isCardsError } = useGetCardsQuery({
        userId,
        collectionId,
        request: { page, count: 100 },
    });

    const isFetching = isCollectionFetching || isCardsFetching;
    const isError = isCollectionError || isCardsError;

    const collection = useTypedSelector((state) => selectCollectionById(state, userId, collectionId));
    const cards = useTypedSelector((state) => selectCards(state, collection?.userId, collection?.id));

    const [showCreateCardModal, setShowCreateCardModal] = useState(false);
    const defaultSchedule = useTypedSelector(
        (state) =>
            collection &&
            selectScheduleById(state, getScheduleId(collection?.defaultScheduleUserId, collection?.defaultScheduleId))
    );

    if (isFetching || isError) {
        return (
            <PageContainer>
                {isFetching && <CircularProgress />}
                {isError && <div>Load error</div>}
            </PageContainer>
        );
    }

    if (!collection) {
        return (
            <PageContainer>
                <PageHeader title="Коллекция не найдена" />
                <div></div>
            </PageContainer>
        );
    }

    return (
        <PageContainer>
            <PageHeader
                title={collection.title}
                subMenu={
                    <Button variant="contained" endIcon={<Add />} onClick={() => setShowCreateCardModal(true)}>
                        Слово
                    </Button>
                }
            />
            {showCreateCardModal && (
                <CreateCardModal
                    collectionId={collection.id}
                    collectionUserId={collection.userId}
                    open={showCreateCardModal}
                    onClose={() => setShowCreateCardModal(false)}
                    defaultSchedule={defaultSchedule}
                />
            )}
            <div style={{ padding: '20px 50px 0' }}>
                <Table>
                    <TableHead>
                        <TableHeaderCell>Запомнить</TableHeaderCell>
                        <TableHeaderCell>Значение</TableHeaderCell>
                        <TableHeaderCell>Описание</TableHeaderCell>
                        <TableHeaderCell></TableHeaderCell>
                    </TableHead>
                    <TableBody>
                        {cards.map((c) => (
                            <CardRow key={c.id} card={c} />
                        ))}
                    </TableBody>
                </Table>
            </div>
            {collection.cardsCount > cardsCountPerPage && (
                <Pagination
                    count={collection.cardsCount / cardsCountPerPage}
                    onChange={(event, page) => setPage(page)}
                />
            )}
        </PageContainer>
    );
};
