import React, { FC, useState } from "react";
import { PageContainer } from "../../../controls/PageContainer/PageContainer";
import { PageHeader } from "../../../controls/PageHeader/PageHeader";
import { useParams } from "react-router-dom";
import { coursesApi, useGetCourseQuery, useGetTopicsQuery } from "../../../redux/courseApi";
import { PageContent } from "../../../controls/PageContent/PageContent";
import { Stack, ToggleButton, ToggleButtonGroup } from "@mui/material";
import { Table, TableBody, TableCell, TableHead, TableHeaderCell, TableRow } from "../../../controls/Table/Table";

export enum Mode {
    Topics,
    Groups,
    Statistic
}

export const AdminCourseNavigationPage: FC = () => {
    const { courseId } = useParams();
    if (!courseId)
        throw new Error();
    const [mode, setMode] = useState<Mode>();
    const { data: course, isFetching: isCourseFetching } = useGetCourseQuery({ courseId: courseId })
    const { data: topics } = useGetTopicsQuery({ courseId: courseId, page: 1, count: 50 })

    const generateContent = () => {
        switch (mode) {
            case Mode.Topics:
                return (
                    <Table>
                        <TableHead>
                            <TableHeaderCell>Название</TableHeaderCell>
                        </TableHead>
                        <TableBody>
                            {topics?.foundItems?.map((x, i) => (<TableRow key={`topics-${x.id}`}>
                                <TableCell>{i}. {x.name}</TableCell>
                            </TableRow>))}
                        </TableBody>
                    </Table>)
            case Mode.Groups:
                return (
                    <Table>
                        <TableHead>
                            <TableHeaderCell>Название</TableHeaderCell>
                            <TableHeaderCell>Ссылка на вступление</TableHeaderCell>
                            <TableHeaderCell>Кол-во человек</TableHeaderCell>
                        </TableHead>
                        <TableBody>
                            {}

                        </TableBody>
                    </Table>
                )
        }
    }
    return (
        <PageContainer>
            <PageHeader title={course?.name ?? ""}/>
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
                        <ToggleButton value={Mode.Topics}>Темы</ToggleButton>
                        <ToggleButton value={Mode.Groups}>Группы</ToggleButton>
                        <ToggleButton value={Mode.Statistic}>Стастисика</ToggleButton>
                    </ToggleButtonGroup>
                    {generateContent()}
                </Stack>
            </PageContent>
        </PageContainer>
    )
}