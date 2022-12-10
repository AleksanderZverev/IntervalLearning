import React, { FC } from 'react';
import { Route, Routes } from 'react-router-dom';
import { CoursesPage } from "../../src/pages/courses/CoursesPage/CoursesPage";
import { CourseNavigationPage } from "../../src/pages/courses/CoursePage/CourseNavigationPage";
import { EditCoursePage } from "../../src/pages/courses/CoursePage/EditCoursePage";
import { EditTopicPage } from "../../src/pages/courses/CoursePage/EditTopicPage";

const CoursesPageRouter: FC = () => {
    return (
        <Routes>
            <Route path="/courses" element={<CoursesPage />} />
            <Route path="/courses/:courseId" element={<CourseNavigationPage />} />
            <Route path="/courses/:courseId/edit" element={<EditCoursePage />} />
            <Route path={"/courses/:courseId/topics/:topicId/edit"} element={<EditTopicPage />}/>
            <Route path="/courses/create" />
        </Routes>
    );
};

export default CoursesPageRouter;