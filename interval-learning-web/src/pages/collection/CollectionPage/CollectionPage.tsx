/* eslint-disable react-hooks/rules-of-hooks */
import { Add, Casino, Public } from '@mui/icons-material';
import { Button, CircularProgress, Pagination, Stack, TableCell } from '@mui/material';
import { FC, useMemo, useState } from 'react';
import { Navigate, useNavigate, useParams } from 'react-router-dom';
import { CreateCardModal } from '../../../controls/Modals/CreateCardModal';
import { PageContainer } from '../../../controls/PageContainer/PageContainer';
import { PageHeader } from '../../../controls/PageHeader/PageHeader';
import { Table, TableBody, TableHead, TableHeaderCell, TableRow } from '../../../controls/Table/Table';
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

    const navigate = useNavigate();
    const [page, setPage] = useState(1);

    const { isFetching: isCollectionFetching, isError: isCollectionError } = useGetCollectionQuery({ collectionId });
    const { isFetching: isCardsFetching, isError: isCardsError } = useGetCardsQuery({
        userId,
        collectionId,
        request: { page, count: cardsCountPerPage },
    });

    const isFetching = isCollectionFetching || isCardsFetching;
    const isError = isCollectionError || isCardsError;

    const collection = useTypedSelector((state) => selectCollectionById(state, userId, collectionId));
    const storageCards = useTypedSelector((state) => selectCards(state, collection?.userId, collection?.id));

    const sortedCards = useMemo(
        () => [...storageCards].sort((f, s) => f.frontSideText.localeCompare(s.frontSideText)),
        [storageCards]
    );

    const cards = useMemo(() => {
        const skip = (page - 1) * cardsCountPerPage;

        const workingCards = [...sortedCards];
        workingCards.splice(0, skip);
        workingCards.splice(cardsCountPerPage);

        return workingCards;
    }, [sortedCards, page]);

    const [showCreateCardModal, setShowCreateCardModal] = useState(false);
    // const defaultSchedule = useTypedSelector(
    //     (state) =>
    //         collection &&
    //         selectScheduleById(state, getScheduleId(collection?.defaultScheduleUserId, collection?.defaultScheduleId))
    // );

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
                titleIcon={collection.isPublic && <Public color="primary" />}
                subMenu={
                    <Stack direction={'row'} gap="10px">
                        <Button variant="contained" endIcon={<Casino />} onClick={() => navigate('words/random')}>
                            Случайные
                        </Button>
                        <Button variant="contained" endIcon={<Add />} onClick={() => setShowCreateCardModal(true)}>
                            Слово
                        </Button>
                    </Stack>
                }
            />
            {showCreateCardModal && (
                <CreateCardModal
                    collectionId={collection.id}
                    collectionUserId={collection.userId}
                    open={showCreateCardModal}
                    onClose={() => setShowCreateCardModal(false)}
                    // defaultSchedule={defaultSchedule}
                />
            )}
            <div>
                <Table>
                    <TableHead>
                        <TableHeaderCell>Запомнить</TableHeaderCell>
                        <TableHeaderCell>Подсказка (чтение)</TableHeaderCell>
                        <TableHeaderCell>Значение</TableHeaderCell>
                        <TableHeaderCell>Описание</TableHeaderCell>
                    </TableHead>
                    <TableBody>
                        {cards && cards.length > 0 ? (
                            cards.map((c) => <CardRow key={c.id} card={c} />)
                        ) : (
                            <TableRow borderless>
                                <TableCell colSpan={99} align="center">
                                    Коллекция пуста
                                </TableCell>
                            </TableRow>
                        )}
                    </TableBody>
                </Table>
                {collection.cardsCount > cardsCountPerPage && (
                    <Pagination
                        count={Math.ceil(collection.cardsCount / cardsCountPerPage)}
                        onChange={(event, page) => setPage(page)}
                    />
                )}
            </div>
        </PageContainer>
    );
};
