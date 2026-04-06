// src/pages/admin/MasterDataPage.tsx
import { useState } from "react";
import AdminSidebar from "../../components/admin/AdminSidebar";

import StoreCategoryTab from "../../components/admin/StoreCategoryTab";
import SpeciesBreedTab from "../../components/admin/SpeciesBreedTab";

export default function MasterDataPage() {
    const [activeTab, setActiveTab] = useState<"category" | "species">("category");

    return (
        <div style={{ display: "flex", minHeight: "100vh" }}>
            <AdminSidebar />

            <div style={{ marginLeft: "260px", flex: 1, padding: "30px 40px", background: "#f8f5f0" }}>
                <h1>Quản lý Master Data</h1>

                <div style={{ display: "flex", gap: "8px", background: "white", padding: "12px", borderRadius: "12px", width: "fit-content", marginBottom: "30px" }}>
                    <TabButton active={activeTab === "category"} label="Danh mục cửa hàng" onClick={() => setActiveTab("category")} />
                    <TabButton active={activeTab === "species"} label="Loài & Giống thú cưng" onClick={() => setActiveTab("species")} />
                </div>

                {activeTab === "category" && <StoreCategoryTab />}
                {activeTab === "species" && <SpeciesBreedTab />}
            </div>
        </div>
    );
}

function TabButton({ active, label, onClick }: any) {
    return (
        <div onClick={onClick} style={{
            padding: "12px 32px",
            borderRadius: "10px",
            cursor: "pointer",
            background: active ? "#86542B" : "transparent",
            color: active ? "white" : "#555",
            fontWeight: active ? "700" : "600"
        }}>
            {label}
        </div>
    );
}