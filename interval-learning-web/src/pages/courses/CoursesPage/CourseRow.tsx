import { IconButton, Link, Stack } from "@mui/material";
import { FC } from "react";
import { TableCell, TableRow } from "../../../controls/Table/Table";
import { Course } from "../../../types/Course";
import { Delete, Edit } from "@mui/icons-material";
import { withMutationResolver, WithMutationResolverProps } from "../../../hoc/withQueryResolver";
import { useDeleteCourseMutation } from "../../../redux/courseApi";
import { useRouter } from "next/router";
import { useNavigate } from "react-router-dom";

export interface CourseRowProps extends WithMutationResolverProps<typeof useDeleteCourseMutation> {
    course: Course;
}

export const CourseRowContent: FC<CourseRowProps> = ({ course, mutationProps: { mutate, showRetryModal } }) => {
    const router = useRouter();
    const navigate = useNavigate();
    const onDelete = async (id: string) => {
        try {
            await mutate({ courseId: id })
        } catch (e) {
        }
    }

    return (
        <TableRow
            hover
            onClick={event => {
                navigate(`${course.id}`);
            }}
            key={`course-row-${course.id}`}>
            <TableCell>{course.name}</TableCell>
            <TableCell>
                <Link>{course.link}</Link>
            </TableCell>
            <TableCell width={150}>
                <IconButton onClick={(e) => {
                    navigate(`${course.id}/edit`);
                    e.stopPropagation();
                }}>
                    <Edit style={{ position: 'relative' }} fontSize={"small"}/>
                </IconButton>
                <IconButton onClick={async (e) => await onDelete(course.id)}>
                    <Delete color={"error"} fontSize={"small"}/>
                </IconButton>
            </TableCell>
        </TableRow>);
}

export const CourseRow = withMutationResolver(useDeleteCourseMutation,
    'Не удалось удалить курс')(CourseRowContent);