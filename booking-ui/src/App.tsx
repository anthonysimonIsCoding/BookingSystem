import { BrowserRouter, Routes, Route } from "react-router-dom";
import StoreListPage from "./pages/StoreListPage";
import StoreDetailPage from "./pages/StoreDetailPage";
import LoginPage from "./pages/LoginPage";
function App() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/" element={<StoreListPage />} />
                <Route path="/store/:id" element={<StoreDetailPage />} />
                <Route path="/login" element={<LoginPage />} />
            </Routes>
        </BrowserRouter>
    );
}

export default App;