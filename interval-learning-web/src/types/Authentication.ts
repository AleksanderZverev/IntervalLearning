export interface AuthenticateRequest {
    email: string;
    password: string;
}

export interface AuthenticateResponse {
    id: number;
    firstName: string;
    lastName: string;
    email: string;
    jwtToken: string;
}
