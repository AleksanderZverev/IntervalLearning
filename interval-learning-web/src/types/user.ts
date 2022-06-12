export interface User {
    id: string;
    firstName: string;
    lastName: string | null;
    email: string;
    jwtToken: string;
}

export interface UserInfo {
    id: string;
    firstName: string;
    lastName: string;
    email: string;
}
