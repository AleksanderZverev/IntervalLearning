import axios, { AxiosInstance } from 'axios';

const axiosInstance: AxiosInstance = axios.create({
    baseURL: '/api/backend',
});

axiosInstance.interceptors.response.use(
    (response) => {
        if (response.status === 204) {
            return response;
        } else {
            if (response.data) {
                return response;
            }

            return;
        }
    },
    (error) => {
        if (error?.response && error.response.status >= 400 && error.response.status < 600) {
            console.error(`Code: ${error.response.status}`);
        }

        return Promise.reject(error);
    }
);

export const setAuthToken = (token: string) => {
    axiosInstance.defaults.headers.common['Authorization'] = `Bearer ${token}`;
};

export const removeAuthToken = () => {
    delete axiosInstance.defaults.headers.common['Authorization'];
};

export default axiosInstance;
