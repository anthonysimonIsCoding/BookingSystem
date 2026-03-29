// src/utils/axiosInstance.ts
import axios from 'axios';

const api = axios.create({
    baseURL: 'http://localhost:5263/api',
});

api.interceptors.request.use((config) => {
    const vendorToken = localStorage.getItem("vendorToken");
    const customerToken = localStorage.getItem("token");

    const isVendorPage = window.location.pathname.startsWith('/vendor');

    const token = isVendorPage ? vendorToken : customerToken;

    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }

    return config;
});

api.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response?.status === 401) {
            console.warn("Token hết hạn hoặc không hợp lệ → logout");

            const isVendorPage = window.location.pathname.startsWith('/vendor');

            // Xóa token tương ứng
            if (isVendorPage) {
                localStorage.removeItem("vendorToken");
                window.location.href = "/vendor/login";
            } else {
                localStorage.removeItem("token");
                window.location.href = "/login";
            }
        }

        return Promise.reject(error);
    }
);

export default api;