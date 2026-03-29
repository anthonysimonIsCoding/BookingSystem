import { createContext, useState, useCallback, useEffect, type ReactNode } from 'react';

type AuthContextType = {
    isCustomerLoggedIn: boolean;
    isVendorLoggedIn: boolean;
    logoutCustomer: () => void;
    logoutVendor: () => void;
};

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
    const [customerToken, setCustomerToken] = useState<string | null>(
        () => localStorage.getItem("token")          // giữ nguyên "token" cho customer
    );
    const [vendorToken, setVendorToken] = useState<string | null>(
        () => localStorage.getItem("vendorToken")
    );

    const isCustomerLoggedIn = !!customerToken;
    const isVendorLoggedIn = !!vendorToken;

    const logoutCustomer = useCallback(() => {
        localStorage.removeItem("token");
        setCustomerToken(null);
    }, []);

    const logoutVendor = useCallback(() => {
        localStorage.removeItem("vendorToken");
        setVendorToken(null);
    }, []);

    // Kiểm tra token khi mở app lại (phiên bản nhẹ để tránh warning)
    useEffect(() => {
        // Có thể mở rộng sau nếu cần gọi API check thật
    }, []);

    return (
        <AuthContext.Provider value={{
            isCustomerLoggedIn,
            isVendorLoggedIn,
            logoutCustomer,
            logoutVendor,
        }}>
            {children}
        </AuthContext.Provider>
    );
}

export { AuthContext };