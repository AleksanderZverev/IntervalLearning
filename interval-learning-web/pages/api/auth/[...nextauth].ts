import NextAuth from 'next-auth';
import GoogleProvider from 'next-auth/providers/google';
import CredentialsProvider from 'next-auth/providers/credentials';
import axiosInstance from '../../../src/api/axiosInstance';
import { AuthenticateRequest, AuthenticateResponse } from '../../../src/types/Authentication';

export default NextAuth({
    providers: [
        GoogleProvider({
            clientId: process.env.GOOGLE_CLIENT_ID || '',
            clientSecret: process.env.GOOGLE_CLIENT_SECRET || '',
            authorization: {
                params: {
                    scope: 'https://www.googleapis.com/auth/userinfo.profile https://www.googleapis.com/auth/userinfo.email',
                    prompt: 'consent',
                    access_type: 'offline',
                    response_type: 'code',
                },
            },
        }),
        CredentialsProvider({
            name: 'Email',
            credentials: {
                email: { label: 'Email', type: 'text', placeholder: 'daveglow@foomail.com' },
                password: { label: 'Password', type: 'password' },
            },
            async authorize(credentials, req) {
                if (!credentials?.email || !credentials?.password) {
                    return null;
                }

                const requestItem: AuthenticateRequest = {
                    email: credentials.email,
                    password: credentials.password,
                };

                try {
                    const response = await axiosInstance.post<AuthenticateResponse>(
                        '/authentication/authenticate',
                        requestItem
                    );
                    if (!response.data) return null;

                    const user = response.data;

                    return { ...user };
                } catch {
                    return null;
                }
            },
        }),
    ],
    jwt: {
        encryption: true,
    } as any,
    secret: process.env.SECRET,
    callbacks: {
        async jwt({ token, account }) {
            console.debug('jwt api method', 'token', token, 'account', account);
            if (account?.id_token) {
                token.accessToken = account.id_token;
            }
            return token;
        },
        async session({ session, token, user }) {
            console.debug('session api method', 'session', session, 'token', token, 'user', user);
            session.accessToken = token.accessToken;
            return session;
        },
    },
});
