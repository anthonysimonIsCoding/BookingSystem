// src/pages/vendor/StoreManagementPage.tsx
import { useState } from "react";
import VendorSidebar from '../../components/vendor/VendorSidebar';
import StoreInfoTab from '../../components/vendor/StoreInfoTab';

export default function StoreManagementPage() {
    const [activeTab, setActiveTab] = useState<"info" | "timeslot" | "service">("info");

    return (
        <div style={{ display: "flex", minHeight: "100vh" }}>
            <VendorSidebar />

            <div style={{ marginLeft: "260px", flex: 1, padding: "30px 40px", background: "#f8f5f0" }}>
                <div style={{ maxWidth: "1400px", margin: "0 auto" }}>
                    <h1 style={{ fontSize: "36px", fontWeight: "700", color: "#333", marginBottom: "10px" }}>
                        Quản Lý Cửa Hàng
                    </h1>

                    {/* Tab Navigation */}
                    <div style={{ display: "flex", gap: "8px", background: "white", padding: "10px", borderRadius: "12px", width: "fit-content", marginBottom: "30px" }}>
                        <TabButton active={activeTab === "info"} onClick={() => setActiveTab("info")} label="Thông tin cửa hàng" />
                        <TabButton active={activeTab === "timeslot"} onClick={() => setActiveTab("timeslot")} label="Quản lý Timeslot" />
                        <TabButton active={activeTab === "service"} onClick={() => setActiveTab("service")} label="Quản lý Dịch vụ" />
                    </div>

                    {/* Nội dung theo tab */}
                    {activeTab === "info" && <StoreInfoTab />}
                    {activeTab === "timeslot" && (
                        <div style={{ background: "white", padding: "40px", borderRadius: "20px", textAlign: "center" }}>
                            <h2>Quản lý Timeslot</h2>
                            <p style={{ color: "#666" }}>Đang phát triển...</p>
                        </div>
                    )}
                    {activeTab === "service" && (
                        <div style={{ background: "white", padding: "40px", borderRadius: "20px", textAlign: "center" }}>
                            <h2>Quản lý Dịch vụ</h2>
                            <p style={{ color: "#666" }}>Đang phát triển...</p>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}

function TabButton({ active, onClick, label }: { active: boolean; onClick: () => void; label: string }) {
    return (
        <div onClick={onClick} style={{
            padding: "12px 28px", borderRadius: "10px", cursor: "pointer", fontWeight: "600",
            background: active ? "#86542B" : "transparent", color: active ? "white" : "#555"
        }}>
            {label}
        </div>
    );
}