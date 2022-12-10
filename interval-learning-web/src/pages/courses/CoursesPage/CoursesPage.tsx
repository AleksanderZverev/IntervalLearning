import { FC, useState } from "react";
import { PageContainer } from "../../../controls/PageContainer/PageContainer";
import { PageHeader } from "../../../controls/PageHeader/PageHeader";
import { Button, Pagination, Portal } from "@mui/material";
import { Folder } from "@mui/icons-material";
import { Table, TableBody, TableHead, TableHeaderCell } from "../../../controls/Table/Table";
import { CourseRow } from "./CourseRow";
import { CreateCourseModal } from "../CoursePage/CreateCourseModal";
import { useGetCoursesQuery } from "../../../redux/courseApi";

const countPerPage = 50;

export const CoursesPage: FC = () => {
    const [showCreateCourseModal, setShowCreateCourseModal] = useState(false);
    const [page, setPage] = useState(1);
    const { data } = useGetCoursesQuery({ page, count: countPerPage });

    return (
        <PageContainer>
            <PageHeader title={"Мои курсы"}
                        subMenu={
                            <Button
                                onClick={() => setShowCreateCourseModal(true)}
                                variant={'contained'}
                                endIcon={<Folder/>}
                            >
                                Создать
                            </Button>
                        }/>
            <Portal>
                <CreateCourseModal isOpen={showCreateCourseModal}
                                   onClose={() => setShowCreateCourseModal(false)}/>
            </Portal>
            <div>
                <Table>
                    <TableHead>
                        <TableHeaderCell>Название</TableHeaderCell>
                        <TableHeaderCell>Ссылка</TableHeaderCell>
                    </TableHead>
                    <TableBody>
                        {data?.foundItems?.slice((page - 1) * countPerPage, page * countPerPage).map(x => <CourseRow
                            course={x} key={x.id}/>)}
                    </TableBody>
                </Table>
                <Pagination
                    page={page}
                    count={Math.ceil((data?.totalCount ?? 0) / countPerPage)}
                    onChange={(event, page) => setPage(page)}
                />
            </div>
        </PageContainer>);
}

