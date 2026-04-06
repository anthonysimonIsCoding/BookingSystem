// src/pages/admin/StoreDetailPage.tsx
import { useState, useEffect } from "react";
import { useParams, useSearchParams, useNavigate } from "react-router-dom";
import axiosInstance from "../../utils/axiosInstance";


import AdminStoreInfoTab from "../../components/admin/AdminStoreInfoTab";
import AdminTimeslotTab from "../../components/admin/AdminTimeslotTab";
import AdminServiceTab from "../../components/admin/AdminServiceTab";
import AdminVoucherTab from "../../components/admin/AdminVoucherTab";
import AdminOrdersTab from "../../components/admin/AdminOrdersTab";
import AdminReviewsTab from "../../components/admin/AdminReviewsTab";
import AdminSidebar from "../../components/admin/AdminSidebar";
export default function StoreDetailPageAdmin() {
    const { storeId } = useParams<{ storeId: string }>();
    const [searchParams, setSearchParams] = useSearchParams();
    const navigate = useNavigate();

    const currentTab = (searchParams.get("tab") as "info" | "timeslot" | "service" | "voucher" | "orders" | "reviews") || "info";

    const [storeName, setStoreName] = useState("Đang tải...");

    useEffect(() => {
        const fetchName = async () => {
            if (!storeId) return;
            try {
                const res = await axiosInstance.get(`/admin/stores/${storeId}`);
                setStoreName(res.data.name || "Cửa hàng");
            } catch (err) {
                setStoreName("Không tìm thấy cửa hàng");
            }
        };
        fetchName();
    }, [storeId]);

    const setActiveTab = (tab: "info" | "timeslot" | "service" | "voucher" | "orders" | "reviews") => {
        setSearchParams({ tab });
    };

    if (!storeId) return <div>Không tìm thấy cửa hàng</div>;

    return (
        <div style={{ display: "flex", minHeight: "100vh" }}>
            <AdminSidebar />

            <div style={{ marginLeft: "260px", flex: 1, padding: "30px 40px", background: "#f8f5f0" }}>
                <div style={{ maxWidth: "1400px", margin: "0 auto" }}>
                    <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "20px" }}>
                        <div>
                            <h1 style={{ fontSize: "36px", fontWeight: "700", color: "#333" }}>{storeName}</h1>
                            <p style={{ color: "#666" }}>ID: {storeId}</p>
                        </div>
                        <button onClick={() => navigate("/admin")} style={{ padding: "10px 24px", background: "#666", color: "white", border: "none", borderRadius: "10px" }}>
                            ← Quay lại danh sách
                        </button>
                    </div>

                    {/* Tabs */}
                    <div style={{ display: "flex", gap: "8px", background: "white", padding: "10px", borderRadius: "12px", width: "fit-content", marginBottom: "30px" }}>
                        <TabButton active={currentTab === "info"} label="Thông tin cửa hàng" onClick={() => setActiveTab("info")} />
                        <TabButton active={currentTab === "timeslot"} label="Timeslot" onClick={() => setActiveTab("timeslot")} />
                        <TabButton active={currentTab === "service"} label="Dịch vụ" onClick={() => setActiveTab("service")} />
                        <TabButton active={currentTab === "voucher"} label="Voucher" onClick={() => setActiveTab("voucher")} />
                        <TabButton active={currentTab === "orders"} label="Đơn hàng" onClick={() => setActiveTab("orders")} />
                        <TabButton active={currentTab === "reviews"} label="Đánh giá" onClick={() => setActiveTab("reviews")} />
                    </div>

                    {currentTab === "info" && <AdminStoreInfoTab storeId={storeId} />}
                    {currentTab === "timeslot" && <AdminTimeslotTab storeId={storeId} />}
                    {currentTab === "service" && <AdminServiceTab storeId={storeId} />}
                    {currentTab === "voucher" && <AdminVoucherTab storeId={storeId} />}
                    {currentTab === "orders" && <AdminOrdersTab storeId={storeId} />}
                    {currentTab === "reviews" && <AdminReviewsTab storeId={storeId} />}
                </div>
            </div>
        </div>
    );
}

function TabButton({ active, label, onClick }: { active: boolean; label: string; onClick: () => void }) {
    return (
        <div onClick={onClick} style={{
            padding: "12px 28px",
            borderRadius: "10px",
            cursor: "pointer",
            fontWeight: active ? "700" : "600",
            background: active ? "#86542B" : "transparent",
            color: active ? "white" : "#555"
        }}>
            {label}
        </div>
    );
}