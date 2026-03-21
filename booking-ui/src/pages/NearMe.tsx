import { useEffect, useState } from "react";
import axios from "axios";
import Navbar from "../components/Navbar";
import StoreMap from "../components/StoreMap";

interface Store {
    id: string;
    name: string;
    address: string;
}

export default function NearMe() {
    const [stores, setStores] = useState<Store[]>([]);

    useEffect(() => {
        const loadStores = async () => {
            const token = localStorage.getItem("token");

            const res = await axios.get(
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

            <div style={{ maxWidth: 1400, margin: "0 auto", padding: "30px 20px" }}>

                <h2 style={{ fontSize: 26, fontWeight: 700, marginBottom: 20 }}>
                    📍 Cửa hàng gần bạn
                </h2>

                <StoreMap />

            </div>
        </div>
    );
}