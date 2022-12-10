import React, { FC, useEffect, useRef, useState } from "react";
import { PageContainer } from "../../../controls/PageContainer/PageContainer";
import { PageHeader } from "../../../controls/PageHeader/PageHeader";
import { useParams } from "react-router-dom";
import { Edit } from "@mui/icons-material";
import { PageContent } from "../../../controls/PageContent/PageContent";
import { Box, Button, Stack, ToggleButton, ToggleButtonGroup } from "@mui/material";
import { TextAreaFormField } from "../../../controls/Form/Form";
import {
    useGetCourseQuery,
    useGetTopicQuery,
    usePatchTopicMutation
} from "../../../redux/courseApi";
import { Table, TableBody, TableCell, TableHead, TableHeaderCell, TableRow } from "../../../controls/Table/Table";
import { withMutationResolver, WithMutationResolverProps } from "../../../hoc/withQueryResolver";
import Markdown from "markdown-to-jsx";

enum Mode {
    Theory = "Теория",
    Collections = "Коллекции",
    Test = "Тест"
}

export interface EditTopicPageProps extends WithMutationResolverProps<typeof usePatchTopicMutation> {

}

export const EditTopicPageContent: FC<EditTopicPageProps> =
    ({
         mutationProps:
             {
                 mutate,
                 showRetryModal
             }
     }) => {
        const { courseId, topicId } = useParams();
        if (!Boolean(courseId) || !Boolean(topicId)) {
            throw new Error();
        }
        const timer = useRef<{ id: number | null }>({ id: null });
        const [mode, setMode] = useState<Mode>(Mode.Theory);
        const [page, setPage] = useState(1);

        const [showMarkdownTheory, setShowMarkdownTheory] = useState(false);
        const { data: course, isFetching: isCoursesFetching } = useGetCourseQuery({ courseId: courseId });

        const { data: topic, isFetching: isTopicsFetching } = useGetTopicQuery({ courseId, topicId });

        const [theory, setTheory] = useState("");
        useEffect(() => {
            if (topic)
                setTheory(topic.theory)
        }, [topic]);

        if (isCoursesFetching || isTopicsFetching) {
            return (
                <PageContainer>
                    <PageHeader title={"Loading"}/>
                </PageContainer>);
        }
        if (course == undefined || topic == undefined) {
            throw new Error();
        }

        const editTopicName = (e: any) => {
        };

        const generateContent = (): React.ReactNode => {
            switch (mode) {
                case Mode.Theory:
                    return (
                        <Stack direction={"column"} gap={5}>
                            {showMarkdownTheory ? (
                                    <Markdown>{theory}</Markdown>
                                ) :

                                <TextAreaFormField value={theory} onChange={(e) => {
                                    const newValue = e.target.value;
                                    setTheory(newValue)
                                    if (timer.current.id != null) {
                                        window.clearTimeout(timer.current.id);
                                    }

                                    timer.current.id = window.setTimeout(
                                        async () => await mutate({
                                            courseId: course.id,
                                            id: topic.id,
                                            theory: newValue,
                                            name: topic.name
                                        }), 500);
                                }} sx={{ minHeight: "600px" }}/>}
                            <ToggleButtonGroup
                                color={"primary"}
                                value={showMarkdownTheory}
                                onChange={(e, newMode: boolean) => {
                                    setShowMarkdownTheory(newMode);
                                }}
                                exclusive
                            >
                                <ToggleButton value={false}>Текст</ToggleButton>
                                <ToggleButton value={true}>Предпросмотр</ToggleButton>
                            </ToggleButtonGroup>
                        </Stack>
                    )
                case Mode.Collections:
                    return (
                        <Box>
                            <Table>
                                <TableHead>
                                    <TableHeaderCell>Название</TableHeaderCell>
                                    <TableHeaderCell>Слов</TableHeaderCell>
                                    <TableHeaderCell>Добавило</TableHeaderCell>
                                    <TableHeaderCell/>
                                </TableHead>
                                <TableBody>
                                    {topic.collections.map(x => (
                                        <TableRow key={`topic-${topic.id}-collections-${x.id}`}>
                                            <TableCell>{x.title}</TableCell>
                                            <TableCell>{x.cardsCount}</TableCell>
                                            <TableCell>100</TableCell>
                                            <TableCell><Edit/></TableCell>
                                        </TableRow>))}
                                </TableBody>
                            </Table>
                            <Button variant={"outlined"}>Добавить коллекцию</Button>
                        </Box>
                    )
            }
        }

        return (
            <PageContainer>
                <PageHeader title={topic.name ?? ""} subMenu={<Edit onClick={editTopicName}/>}/>
                <PageContent>
                    <Stack direction={"column"} gap={5}>
                        <ToggleButtonGroup
                            color="primary"
                            value={mode}
                            onChange={(e, newMode: Mode) => {
                                setMode(newMode);
                            }}
                            exclusive
                        >
                            <ToggleButton value={Mode.Theory}>Теория</ToggleButton>
                            <ToggleButton value={Mode.Collections}>Коллекции</ToggleButton>
                            <ToggleButton value={Mode.Test}>Тест</ToggleButton>
                        </ToggleButtonGroup>
                        {generateContent()}
                    </Stack>
                </PageContent>
            </PageContainer>
        );
    }

export const EditTopicPage = withMutationResolver(usePatchTopicMutation, 'Не удалось сохранить данные')
(EditTopicPageContent)