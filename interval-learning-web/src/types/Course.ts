import { Collection } from "./Collection";
import { User } from "./user";

export interface Course {
    id: string;
//    userId: string;
    name: string;
    link: string;
    description: string;
    topics: Topic[];
    usersGroups: UserGroup[];
}

export interface Topic {
    id: string;
    parentCourseId: string;
    name: string;
    theory: string;
    collections: Collection[];
}

export interface UserGroup {
    id: string;
    name: string;
    users: User[]
}