import { useEffect, useState } from "react";
import api from '../utils/axiosInstance';
import StoreCard from "../components/StoreCard";
import Navbar from '../components/Navbar';
import HeroBanner from '../components/HeroBanner';
import StoreMap from "../components/StoreMap"
interface Store {
    id: string;
    name: string;
    address: string;
}

export default function HomePage() {
    const [stores, setStores] = useState<Store[]>([]);

    useEffect(() => {
        const loadStores = async () => {
            const token = localStorage.getItem("token");

            const res = await api.get(
                "http://localhost:5263/api/stores",
                {
                    headers: { Authorization: `Bearer ${token}` }
                }
            );

            setStores(res.data);
        };

        loadStores();
    }, []);

    return (

        <div className="min-h-screen bg-gray-100">
            <Navbar />
            <div style={{ maxWidth: 1400, margin: "0 auto" }}>
            <HeroBanner />
            </div>

            
        </div>
    );
}