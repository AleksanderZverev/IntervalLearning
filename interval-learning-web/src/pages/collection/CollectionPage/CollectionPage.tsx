/* eslint-disable react-hooks/rules-of-hooks */
import { Add, Casino, Public } from '@mui/icons-material';
import { Autocomplete, Button, CircularProgress, Pagination, Stack, TableCell, TextField } from '@mui/material';
import dayjs from 'dayjs';
import { FC, useEffect, useMemo, useRef, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { CreateCardModal } from '../../../controls/Modals/CreateCardModal';
import { PageContainer } from '../../../controls/PageContainer/PageContainer';
import { PageHeader } from '../../../controls/PageHeader/PageHeader';
import { Table, TableBody, TableHead, TableHeaderCell, TableRow } from '../../../controls/Table/Table';
import useTypedSelector, { useRequiredTypedSelector } from '../../../hooks/useTypedSelector';
import { SearchFieldType, useGetCardsQuery, useSearchCardsQuery } from '../../../redux/cardsApi';
import { useGetCollectionQuery } from '../../../redux/collectionApi';
import { selectCards } from '../../../redux/slices/cardsSlice';
import { selectCollectionById } from '../../../redux/slices/collectionsSlice';
import { selectTheme } from '../../../redux/slices/themeSlice';
import { CardRow } from './CardRow';
import { FormField } from "../../../controls/Form/Form";
import { useDocumentTitle } from "../../../hooks/useCollectionTitle";

const cardsCountPerPage = 1;
const defaultSearchFieldType = "Слово";
const mapTextToFieldType: { [key: string]: SearchFieldType; } = {
    "Перевод": SearchFieldType.MeaningText,
    "Подсказка": SearchFieldType.PromptText,
    "Слово": SearchFieldType.RememberingText
}

export const CollectionPage: FC = () => {
    const { userId, collectionId } = useParams();

    if (!collectionId || !userId) {
        throw new Error();
    }

    const navigate = useNavigate();
    const [page, setPage] = useState(1);
    const [searchPage, setSearchPage] = useState(1);
    const [searchValue, setSearchValue] = useState('');
    const [searchFieldType, setSearchFieldType] = useState<string>(defaultSearchFieldType);
    const useSearch = useMemo(() => Boolean(searchValue), [searchValue]);

    const onSearchValueChange = (newValue: string) => {
        if (!Boolean(newValue)) {
            setSearchPage(1);
        }
        setSearchValue(newValue);
    }

    const { isFetching: isCollectionFetching, isError: isCollectionError } = useGetCollectionQuery({ collectionId });
    const { isFetching: isCardsFetching, isError: isCardsError } = useGetCardsQuery({
        userId,
        collectionId,
        request: { page, count: cardsCountPerPage },
    });

    const { isError: isSearchError, data: searchedCards } = useSearchCardsQuery({
        collectionId,
        userId,
        request: {
            searchValue,
            page: 1,
            count: cardsCountPerPage * searchPage,//todo придумать как хранить в состоянии
            fieldType: mapTextToFieldType[searchFieldType]
        }
    }, { skip: !useSearch });
    const isFetching = isCollectionFetching || isCardsFetching;
    const isError = isCollectionError || isCardsError || isSearchError;

    const collection = useRequiredTypedSelector((state) => selectCollectionById(state, userId, collectionId));
    const theme = useRequiredTypedSelector((state) => selectTheme(state, collection.themeId));
    const storageCards = useTypedSelector((state) => selectCards(state, collection?.userId, collection?.id));

    useDocumentTitle(collection?.title, '📘');

    const sortedCards = useMemo(
        () => [...(useSearch ? (searchedCards ?? []) : storageCards)].sort((f, s) => dayjs(s.createdDate).diff(dayjs(f.createdDate))),
        [useSearch, searchedCards, storageCards]
    );

    const cards = useMemo(() => {
        const skip = ((useSearch ? searchPage : page) - 1) * cardsCountPerPage;

        const workingCards = [...sortedCards];
        workingCards.splice(0, skip);
        workingCards.splice(cardsCountPerPage);

        return workingCards;
    }, [page, sortedCards]);

    const [showCreateCardModal, setShowCreateCardModal] = useState(false);
    // const defaultSchedule = useTypedSelector(
    //     (state) =>
    //         collection &&
    //         selectScheduleById(state, getScheduleId(collection?.defaultScheduleUserId, collection?.defaultScheduleId))
    // );

    if (isFetching || isError) {
        return (
            <PageContainer>
                {isFetching && <CircularProgress/>}
                {isError && <div>Load error</div>}
            </PageContainer>
        );
    }

    if (!collection) {
        return (
            <PageContainer>
                <PageHeader title="Коллекция не найдена"/>
                <div></div>
            </PageContainer>
        );
    }

    return (
        <PageContainer>
            <PageHeader
                title={collection.title}
                titleIcon={collection.isPublic && <Public color="primary"/>}
                subTitle={theme.name + ', ' + collection.cardsCount + ' карточек'}
                subMenu={
                    <Stack direction={'row'} gap="10px">
                        <Button variant="contained" endIcon={<Casino/>} onClick={() => navigate('words/random')}>
                            Случайные
                        </Button>
                        <Button variant="contained" endIcon={<Add/>} onClick={() => setShowCreateCardModal(true)}>
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
                <Stack direction={"row"} gap={4} marginY={2}>
                    <TextField
                        value={searchValue}
                        margin={"none"}
                        placeholder="Поиск карточки"
                        variant="standard"
                        sx={{ fontSize: "50px" }}
                        onChange={(e) => onSearchValueChange(e.target.value)}
                    />
                    <Autocomplete
                        sx={{ minWidth: '150px', width: "150px" }}
                        value={searchFieldType}
                        options={Object.keys(mapTextToFieldType)}
                        renderInput={params => <FormField sx={{ height: "20px" }} {...params}/>}
                        onChange={(event, newValue) => setSearchFieldType(newValue ?? defaultSearchFieldType)}
                    />
                </Stack>
                <Table>
                    <TableHead>
                        <TableHeaderCell>Запомнить</TableHeaderCell>
                        <TableHeaderCell>Подсказка (чтение)</TableHeaderCell>
                        <TableHeaderCell>Значение</TableHeaderCell>
                        <TableHeaderCell>Описание</TableHeaderCell>
                        <TableHeaderCell sx={{ minWidth: "100px" }}/>
                    </TableHead>
                    <TableBody>
                        {cards && cards.length > 0 ? (
                            cards.map((c) => <CardRow key={c.id} card={c}/>)
                        ) : (
                            <TableRow borderless>
                                <TableCell colSpan={99} align="center">
                                    {searchValue ? "Ничего не найдено" : "Коллекция пуста"}
                                </TableCell>
                            </TableRow>
                        )}
                    </TableBody>
                </Table>
                {collection.cardsCount > cardsCountPerPage && (
                    <Pagination
                        page={useSearch ? searchPage : page}
                        count={Math.ceil(collection.cardsCount / cardsCountPerPage)}
                        onChange={(event, page) => useSearch ? setSearchPage(page) : setPage(page)}
                    />
                )}
            </div>
        </PageContainer>
    );
};
