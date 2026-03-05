import { useEffect, useState } from "react";
import axios from "axios";
import StoreCard from "../components/StoreCard";

interface Store {
    id: string;
    name: string;
    address: string;
}

export default function StoreListPage() {
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
        <div
            style={{
                background: "#f3f4f6",
                minHeight: "100vh",
                padding: "40px 32px"
            }}
        >
            <div style={{ maxWidth: 1400, margin: "0 auto" }}>
                {/* Header giống Flash Sale */}
                <div
                    style={{
                        display: "flex",
                        justifyContent: "space-between",
                        alignItems: "center",
                        marginBottom: 28
                    }}
                >
                    <h2 style={{ fontSize: 26, fontWeight: 700 }}>
                        ⚡ Cửa hàng gần bạn
                    </h2>

                    <span
                        style={{
                            color: "#ff6b00",
                            fontWeight: 600,
                            cursor: "pointer"
                        }}
                    >
                        Xem tất cả →
                    </span>
                </div>

                {/* Grid card */}
                <div
                    style={{
                        display: "grid",
                        gridTemplateColumns:
                            "repeat(auto-fill, minmax(320px, 1fr))",
                        gap: 24
                    }}
                >
                    {stores.map(store => (
                        <StoreCard key={store.id} store={store} />
                    ))}
                </div>
            </div>
        </div>
    );
}