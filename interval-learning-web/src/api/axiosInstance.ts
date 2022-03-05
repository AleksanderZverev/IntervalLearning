import axios, { AxiosError, AxiosInstance } from 'axios';

const axiosInstance: AxiosInstance = axios.create({
    baseURL: 'http://localhost:5249/api', //'/api/backend',
});

axiosInstance.interceptors.response.use(
    (response) => response,
    (error: AxiosError) => {
        if (!error.response) {
            return Promise.reject(error);
        }

        if (error.response.status === 401) {
            console.error('Code 401 (Unauthorized)');
            error.response.data = 'Вы не авторизованы';
        }

        if (error.response.status >= 500 && error.response.status < 600) {
            error.response.data = 'Сервер не отвечает';
        }

        return Promise.reject(pickDataOrTitle(error));
    }
);

export const setAuthToken = (token: string) => {
    axiosInstance.defaults.headers.common['Authorization'] = `Bearer ${token}`;
};

export const removeAuthToken = () => {
    delete axiosInstance.defaults.headers.common['Authorization'];
};

export default axiosInstance;

function getErrorString(e: any): string | null {
    if (typeof e === 'string') {
        return e;
    }

    if (typeof e !== 'object') {
        return null;
    }

    const propertyNameToDescriptor = Object.getOwnPropertyDescriptors(e);
    const descriptorKeys = Object.keys(propertyNameToDescriptor);

    const propertyNameWithErrors = [];

    for (const descriptorKey of descriptorKeys) {
        const descriptor = propertyNameToDescriptor[descriptorKey];
        const value = descriptor.value;

        if (value && Array.isArray(value)) {
            const errorsComment = value
                .filter((e) => typeof e === 'string' && Boolean(e))
                .join(', ')
                .trim();

            if (errorsComment) {
                propertyNameWithErrors.push(errorsComment);
            }
        }
    }

    return propertyNameWithErrors.length > 0 ? propertyNameWithErrors.join('. ') : null;
}

function pickDataOrTitle(r: AxiosError): string {
    const unknownError = 'Неизвестная ошибка';
    if (!r.response?.data) {
        return unknownError;
    }

    const data = r.response.data;

    if (typeof data === 'string') {
        return data;
    }

    if ('errors' in data) {
        const errorsString = getErrorString(data.errors);

        if (errorsString) {
            return errorsString;
        }
    }

    if ('title' in data) {
        return data.title;
    }

    return unknownError;
}
