import { FC, useState } from "react";
import { PageContainer } from "../../../controls/PageContainer/PageContainer";
import { PageHeader } from "../../../controls/PageHeader/PageHeader";
import { Button, Icon, IconButton, Stack, Typography } from "@mui/material";
import { Delete, DragIndicator, Edit } from "@mui/icons-material";
import { PageContent } from "../../../controls/PageContent/PageContent";
import { Table, TableBody, TableCell, TableHead, TableHeaderCell, TableRow } from "../../../controls/Table/Table";
import useTypedSelector from "../../../hooks/useTypedSelector";
import { useNavigate, useParams } from "react-router-dom";
import { AddTopicModal } from "./AddTopicModal";
import { selectCourse } from "../../../redux/slices/coursesSlice";
import { useDeleteTopicMutation, useGetTopicsQuery } from "../../../redux/courseApi";
import { withMutationResolver, WithMutationResolverProps } from "../../../hoc/withQueryResolver";
import { useRouter } from "next/router";

export interface EditCoursePageProps extends WithMutationResolverProps<typeof useDeleteTopicMutation> {

}

export const EditCoursePageContent: FC<EditCoursePageProps> = ({ mutationProps: { mutate, showRetryModal } }) => {
    const router = useRouter();
    const navigate = useNavigate();
    const { courseId } = useParams();
    const [showCreateTopicModal, setShowCreateTopicModal] = useState(false);
    if (courseId == null)
        throw new Error();

    const editCourseName = (e: any) => {
    };

    const onDelete = async (topicId: string) => {
        try {
            await mutate({ courseId: courseId, id: topicId })
        } catch {
            showRetryModal(async () => await onDelete(topicId))
        }
    }

    const course = useTypedSelector((state) => selectCourse(state, courseId));
    if (!course) {
        throw new Error("Course is undefined")
    }
    const { data } = useGetTopicsQuery({ page: 1, count: 50, courseId: course.id });
    const generateTopics = () => {
        if (!Boolean(data?.foundItems?.length))
            return (
                <>
                    <Typography textAlign={"center"}>Здесь пока нет тем:(</Typography>
                </>
            )
        return (
            <Table>
                <TableHead>
                    <TableHeaderCell sx={{ margin: '0px' }} width={5}/>
                    <TableHeaderCell>Название</TableHeaderCell>
                    <TableHeaderCell/>
                    <TableHeaderCell/>
                </TableHead>
                <TableBody>{data?.foundItems?.map((x, i) => (
                    <TableRow key={`${course.id}-topics-table`} >
                        <TableCell sx={{ padding: '0' }} align={"center"}><DragIndicator width={10}/></TableCell>
                        <TableCell>{i + 1}. {x.name}</TableCell>
                        <TableCell>
                            <IconButton onClick={(e) => navigate(`/courses/${course.id}/topics/${x.id}/edit`)}>
                                <Edit/>
                            </IconButton>
                        </TableCell>
                        <TableCell><Delete color={"error"} onClick={() => onDelete(x.id)}/></TableCell>
                    </TableRow>
                ))}
                </TableBody>
            </Table>
        )
    }
    return (
        <PageContainer>
            <PageHeader title={course.name} subMenu={(
                <Stack direction={"row"} gap={2}>
                    <Edit onClick={editCourseName}/>
                    <Button variant={"outlined"}>Сохранить</Button>
                </Stack>
            )}/>
            <PageContent>
                {generateTopics()}
                <Button style={{ marginTop: "10px" }} variant={"outlined"}
                        onClick={() => setShowCreateTopicModal(true)}>Добавить тему</Button>
                <AddTopicModal isOpen={showCreateTopicModal}
                               onClose={() => setShowCreateTopicModal(false)}
                               courseId={course.id}/>
            </PageContent>
        </PageContainer>
    );
}
export const EditCoursePage = withMutationResolver(useDeleteTopicMutation, 'Не удалось удалить тему')
(EditCoursePageContent);