import { api, tagTypes } from "./apiSlice";
import { addTopicToCourse, deleteCourse, setCourse, setCourses } from "./slices/coursesSlice";
import { Course, Topic, TopicCollection } from "../types/Course";
import Courses from "../../pages/courses/[[...courses]]";

const baseUrl = "/courses"

export interface GetCourses {
    page: number;
    count: number;
}


export interface CreateCourseItem {
    name: string;
    description: string;
    isPrivate: boolean;
}

export interface PatchTopicItem {
    id: string;
    name?: string;
    courseId: string;
    theory?: string;
}

export interface GetTopicsItem {
    courseId: string;
    page: number;
    count: number;
    name?: string;
}

export interface SearchResponse<T> {
    foundItems: T[];
    totalCount: number;
}

export interface SearchTopicCollectionsRequest {
    courseId: string;
    topicId: string;
    name?: string;
    page: number;
    count: number;
}

export const coursesApi = api.injectEndpoints({
    endpoints: (build) => ({
        getCourses: build.query<SearchResponse<Course>, GetCourses>({
            query: (arg) => ({
                method: 'GET',
                url: baseUrl,
                params: arg,
                onSuccess: async (dispatch, data) => {
                    const response = data as SearchResponse<Course>;
                    dispatch(setCourses(response?.foundItems));
                },
            })
        }),
        getCourse: build.query<Course, { courseId: string; }>({
            query: (arg) => ({
                method: 'GET',
                url: `${baseUrl}/${arg.courseId}`,
                params: arg,
                onSuccess: async (dispatch, data) => {
                    dispatch(setCourse(data as Course));
                }
            })
        }),
        createCourse: build.mutation<Course, CreateCourseItem>({
            query: (item) => ({
                method: 'POST', url: baseUrl,
                data: item,
                onSuccess: async (dispatch, data) => {
                    dispatch(setCourse(data as Course))
                }
            }),
        }),
        deleteCourse: build.mutation<Course, { courseId: string }>({
            query: (item) => ({
                method: 'DELETE',
                url: `${baseUrl}/${item.courseId}`,
                onSuccess: async (dispatch, data) => {
                    dispatch(deleteCourse(data as Course))
                }
            }),
        }),
        createTopic: build.mutation<Topic, { name: string, courseId: string; theory: string; }>({
            query: (item) => ({
                method: 'POST',
                data: item,
                url: `${baseUrl}/${item.courseId}/topics`,
                onSuccess: async (dispatch, data) => {
                    dispatch(addTopicToCourse(data as Topic))
                }
            }),
        }),
        patchTopic: build.mutation<Topic, PatchTopicItem>({
            query: (item) => ({
                method: 'POST',
                url: `${baseUrl}/${item.courseId}/topics/${item.id}`,
                data: item,
                onSuccess: async (dispatch, data) => {
                    dispatch(addTopicToCourse(data as Topic))
                }
            }),
        }),
        getTopics: build.query<SearchResponse<Topic>, GetTopicsItem>({
            query: (item) => ({
                method: 'GET',
                url: `${baseUrl}/${item.courseId}/topics`,
                params: item,
                onSuccess: async (dispatch, data) => {
                    console.log(data)
                }
            })
        }),
        getTopic: build.query<Topic, { topicId: string; courseId: string; }>({
            query: (item) => ({
                method: 'GET',
                url: `${baseUrl}/${item.courseId}/topics/${item.topicId}`,
                onSuccess: async (dispatch, data) => {
                    console.log(data)
                }
            })
        }),
        deleteTopic: build.mutation<Topic, { id: string; courseId: string; }>({
            query: (item) => ({
                method: 'DELETE',
                url: `${baseUrl}/${item.courseId}/topics/${item.id}`,
                onSuccess: async (dispatch, data) => {
                    //todo
                }
            }),
        }),
        searchTopicCollections: build.query<TopicCollection[], SearchTopicCollectionsRequest>({
            query: (item) => ({
                method: 'GET',
                url: `${baseUrl}/${item.courseId}/topics/${item.topicId}/topic-collections`,
                params: item,
                onSuccess: async (dispatch, data) => {
                    //todo
                }
            }),
        }),
        createTopicCollection: build.mutation<TopicCollection, {courseId: string, topicId: string, name: string}>({
            query: (item) => ({
                method: 'POST',
                url: `${baseUrl}/${item.courseId}/topics/${item.topicId}/topic-collections`,
                data: item,
                onSuccess: async (dispatch, data) => {
                    //todo
                }
            }),
        }),
    })
});

export const {
    useGetCoursesQuery,
    useCreateCourseMutation,
    useDeleteCourseMutation,
    useCreateTopicMutation,
    useGetTopicsQuery,
    useDeleteTopicMutation,
    usePatchTopicMutation,
    useGetCourseQuery,
    useGetTopicQuery,
    useSearchTopicCollectionsQuery,
    useCreateTopicCollectionMutation,
} = coursesApi;