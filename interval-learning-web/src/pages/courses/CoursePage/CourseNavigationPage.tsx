import { FC, useState } from "react";
import { PageHeader } from "../../../controls/PageHeader/PageHeader";
import { PageContainer } from "../../../controls/PageContainer/PageContainer";
import { useNavigate, useParams } from "react-router-dom";
import { Box, Button, Container, IconButton, Stack } from "@mui/material";
import { PageContent } from "../../../controls/PageContent/PageContent";
import Markdown from "markdown-to-jsx";
import { Slider } from "../../../controls/Slider/Slider";
import useTypedSelector from "../../../hooks/useTypedSelector";
import { selectCourse } from "../../../redux/slices/coursesSlice";
import { useGetTopicsQuery } from "../../../redux/courseApi";
import { Edit } from "@mui/icons-material";
import { useRouter } from "next/router";
import HourglassDisabledIcon from '@mui/icons-material/HourglassDisabled';

export const CourseNavigationPage: FC = () => {
    const { courseId } = useParams();
    const router = useRouter();
    const navigate = useNavigate();
    const [index, setIndex] = useState<number>(0);
    if (courseId == null)
        throw new Error();

    const course = useTypedSelector((state) => selectCourse(state, courseId));
    if (course == null)
        throw new Error("Course is null");
    const { data: topicsResponse } = useGetTopicsQuery({ courseId: courseId, page: 1, count: 50 })
    if (!topicsResponse || !topicsResponse.foundItems || !topicsResponse.foundItems.length)
        return (
            <PageContainer>
                <PageHeader title={course.name} subTitle={course.description}/>
                <PageContent>
                    Здесь пока нет тем :(
                </PageContent>
            </PageContainer>
        )

    const topics = topicsResponse.foundItems;
    const topic = topics[index];

    return (
        <Box display={"flex"} justifyContent={"start"} alignItems={"center"} alignContent={"start"}>
            {Boolean(topics) &&
                (
                    <Box height={300} marginLeft={5} style={{ position: "relative" }}>
                        <Slider
                            min={0}
                            max={topics?.length - 1 ?? 0}
                            value={index + 1}
                            activeValue={index + 1}
                            onValueChange={newValue => setIndex(newValue)}
                            getHoverTitle={currentIndex => topics[currentIndex]?.name ?? ''} vertical/>
                    </Box>)
            }
            <PageContainer>
                <PageHeader title={course.name} subTitle={course.description}/>
                <PageContent>
                    <Stack gap={5} direction={"column"}>
                        <Stack gap={2} direction={"row"} alignItems={"center"}>
                            <span>{topic?.name}</span>
                            <IconButton onClick={() => navigate(`topics/${topic.id}/edit`)}>
                                <Edit/>
                            </IconButton>
                        </Stack>
                        <Container maxWidth={"lg"} sx={{ minHeight: "500px" }}>
                            {topic.theory
                                ? (<Markdown options={{ wrapper: 'aside' }}>{topic?.theory ?? ''}</Markdown>)
                                : (
                                    <Stack direction={"column"} gap={1}>
                                        <span>Здесь пока нет контента</span>
                                        <HourglassDisabledIcon width={200}/>
                                    </Stack>)}
                            <Box display={"flex"} justifyContent={"space-between"} marginTop={5}>
                                <Button variant={"outlined"} onClick={(e) => {
                                    if (index >= 1)
                                        setIndex(index - 1)
                                }}>К предыдущему</Button>
                                <Button variant={"outlined"} onClick={(e) => {
                                    if (index < topics.length - 1)
                                        setIndex(index + 1)
                                }}>К следующему</Button>
                            </Box>
                        </Container>
                    </Stack>
                </PageContent>
            </PageContainer>
        </Box>
    )
}