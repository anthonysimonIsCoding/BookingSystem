import { useState, useEffect } from "react";
import { useSearchParams } from "react-router-dom";   // ← Thêm dòng này
import VendorSidebar from '../../components/vendor/VendorSidebar';
import StoreInfoTab from '../../components/vendor/StoreInfoTab';
import TimeslotTab from '../../components/vendor/TimeslotTab';
import ServiceTab from '../../components/vendor/ServiceTab';
import VoucherTab from '../../components/vendor/VoucherTab';

export default function StoreManagementPage() {
    const [searchParams, setSearchParams] = useSearchParams();

    // Lấy tab từ URL, mặc định là "info"
    const currentTab = (searchParams.get("tab") as "info" | "timeslot" | "service" | "voucher") || "info";

    const setActiveTab = (tab: "info" | "timeslot" | "service" | "voucher") => {
        setSearchParams({ tab });   // Cập nhật URL
    };

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
                        <TabButton active={currentTab === "info"} onClick={() => setActiveTab("info")} label="Thông tin cửa hàng" />
                        <TabButton active={currentTab === "timeslot"} onClick={() => setActiveTab("timeslot")} label="Quản lý Timeslot" />
                        <TabButton active={currentTab === "service"} onClick={() => setActiveTab("service")} label="Quản lý Dịch vụ" />
                        <TabButton active={currentTab === "voucher"} onClick={() => setActiveTab("voucher")} label="Quản lý Voucher" />
                    </div>

                    {/* Nội dung theo tab */}
                    {currentTab === "info" && <StoreInfoTab />}
                    {currentTab === "timeslot" && <TimeslotTab />}
                    {currentTab === "service" && <ServiceTab />}
                    {currentTab === "voucher" && <VoucherTab />}
                </div>
            </div>
        </div>
    );
}

function TabButton({ active, onClick, label }: { active: boolean; onClick: () => void; label: string }) {
    return (
        <div
            onClick={onClick}
            style={{
                padding: "12px 28px",
                borderRadius: "10px",
                cursor: "pointer",
                fontWeight: "600",
                background: active ? "#86542B" : "transparent",
                color: active ? "white" : "#555",
                transition: "all 0.2s"
            }}
        >
            {label}
        </div>
    );
}