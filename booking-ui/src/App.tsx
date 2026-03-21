import { BrowserRouter, Routes, Route } from "react-router-dom";
import StoreListPage from "./pages/StoreListPage";
import StoreDetailPage from "./pages/StoreDetailPage";
import LoginPage from "./pages/LoginPage";
import ProfilePage from "./pages/ProfilePage";
import HomePage from "./pages/HomePage";
import NearMe from "./pages/NearMe";
function App() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/" element={<HomePage />} />
                <Route path="/store/:id" element={<StoreDetailPage />} />
                <Route path="/login" element={<LoginPage />} />
                <Route path="/profile" element={<ProfilePage />} />
                <Route path="/list" element={<StoreListPage />} />
                <Route path="/nearme" element={<NearMe />} />
            </Routes>
        </BrowserRouter>
    );
}

export default App;