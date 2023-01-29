import React, { FC, useState } from "react";
import { Box, Button, IconButton, Stack } from "@mui/material";
import { Table, TableBody, TableCell, TableHead, TableHeaderCell, TableRow } from "../../../controls/Table/Table";
import { Edit } from "@mui/icons-material";
import { useSearchTopicCollectionsQuery } from "../../../redux/courseApi";
import { AddTopicCollectionModal } from "./AddTopicCollectionModal";

export interface AddTopicCollectionsPageProps {
    courseId: string;
    topicId: string;
}


export const AddTopicCollectionsPage: FC<AddTopicCollectionsPageProps> = ({ topicId, courseId }) => {
    const [showModal, setShowModal] = useState(false);

    const { data: topicCollections, isFetching: isTopicCollectionsFetching } = useSearchTopicCollectionsQuery({
        courseId: courseId,
        topicId: topicId,
        page: 1,
        count: 50
    });

    if (isTopicCollectionsFetching)
        return (<Box>
            Loading
        </Box>)

    return (
        <Stack gap={5}>
            {!topicCollections?.length
                ? (<div>Здесь пока ничего нет :(</div>)
                : (<Table>
                    <TableHead>
                        <TableHeaderCell>Название</TableHeaderCell>
                    </TableHead>
                    <TableBody>
                        {topicCollections?.map(x => (
                            <TableRow key={`topic-collections-${x.id}`}>
                                <TableCell>{x.name}</TableCell>
                                <TableCell width={50}><IconButton><Edit/></IconButton></TableCell>
                            </TableRow>))}
                    </TableBody>
                </Table>)}
            <Button variant={"outlined"} onClick={() => setShowModal(true)}>Добавить коллекцию</Button>
            <AddTopicCollectionModal topicId={topicId}
                                     courseId={courseId}
                                     isOpen={showModal}
                                     onClose={() => setShowModal(false)}/>
        </Stack>
    )
}