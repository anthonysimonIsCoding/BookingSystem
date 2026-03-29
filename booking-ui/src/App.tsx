import { BrowserRouter, Routes, Route } from "react-router-dom";
import StoreListPage from "./pages/StoreListPage";
import StoreDetailPage from "./pages/StoreDetailPage";
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";
import ProfilePage from "./pages/ProfilePage";
import HomePage from "./pages/HomePage";
import NearMe from "./pages/NearMe";
import LoginVendorPage from "./pages/vendor/LoginVendorPage";
import OrdersPage from "./pages/vendor/OrdersPage";

import { AuthProvider } from './contexts/AuthContext';
import DashboardPage from "./pages/vendor/DashboardPage";
import StoreManagementPage from "./pages/vendor/StoreManagementPage";

function App() {
    return (
        <AuthProvider>
        <BrowserRouter>
            <Routes>
                    {/* ==================== CUSTOMER ROUTES ==================== */}
                    <Route path="/" element={<HomePage />} />
                    <Route path="/store/:id" element={<StoreDetailPage />} />
                    <Route path="/login" element={<LoginPage />} />
                    <Route path="/register" element={<RegisterPage />} />
                    <Route path="/profile" element={<ProfilePage />} />
                    <Route path="/list" element={<StoreListPage />} />
                    <Route path="/nearme" element={<NearMe />} />

                    {/* ==================== VENDOR ROUTES ==================== */}
                    <Route path="/vendor" element={<DashboardPage />} />
                    <Route path="/vendor/login" element={<LoginVendorPage />} />
                    <Route path="/vendor/orders" element={<OrdersPage />} />
                    <Route path="/vendor/store" element={<StoreManagementPage />} />
                    
            </Routes>
            </BrowserRouter>
        </AuthProvider>
    );
}

export default App;