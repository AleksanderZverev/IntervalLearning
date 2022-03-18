export interface User {
    id: string;
    firstName: string;
    lastName: string | null;
    email: string;
    jwtToken: string;
}
