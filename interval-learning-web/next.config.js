/** @type {import('next').NextConfig} */
const nextConfig = {
    reactStrictMode: true,
    i18n: {
        locales: ['en', 'ru'],
        defaultLocale: 'en',
    },
    experimental: {
        outputStandalone: true,
    },
    typescript: {
        ignoreBuildErrors: true,
    },
    // async rewrites() {
    //     return [
    //         {
    //             source: '/api/backend/:path*',
    //             destination: 'http://localhost:5249/api/:path*',
    //         },
    //     ];
    // },
};

module.exports = nextConfig;
