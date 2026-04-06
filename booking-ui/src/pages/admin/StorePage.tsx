// src/pages/admin/StoresPage.tsx
import { useState, useEffect } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import axiosInstance from "../../utils/axiosInstance";
import AdminSidebar from "../../components/admin/AdminSidebar";

const statusLabels: Record<number, string> = {
    0: "Cần phê duyệt",
    1: "Đang hoạt động",
    2: "Bị từ chối",
    3: "Bị khóa"
};

const statusColors: Record<number, string> = {
    0: "#f59e0b",
    1: "#10b981",
    2: "#ef4444",
    3: "#ef4444"
};

interface StoreItem {
    id: string;
    name: string;
    ownerName: string;
    ownerEmail: string;
    address: string;
    status: number;
    averageRating: number;
    reviewCount: number;
    totalCompletedBookings: number;
    createdAt: string;
}

export default function StoresPage() {
    const navigate = useNavigate();
    const [searchParams, setSearchParams] = useSearchParams();

    const getCurrentTab = (): 0 | 1 | 2 | 3 => {
        const tabParam = searchParams.get("tab");
        const tabNumber = tabParam ? parseInt(tabParam) : 1;
        return [0, 1, 2, 3].includes(tabNumber) ? (tabNumber as 0 | 1 | 2 | 3) : 1;
    };

    const currentTab = getCurrentTab();

    const [searchTerm, setSearchTerm] = useState("");
    const [stores, setStores] = useState<StoreItem[]>([]);
    const [loading, setLoading] = useState(false);

    const [currentPage, setCurrentPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const pageSize = 20;

    const setActiveTab = (tab: 0 | 1 | 2 | 3) => {
        setSearchParams({ tab: tab.toString() });
        setCurrentPage(1);
    };

    const loadStores = async () => {
        setLoading(true);
        try {
            const params: any = {
                page: currentPage,
                pageSize: pageSize,
            };

            if (searchTerm.trim()) params.search = searchTerm.trim();
            params.status = currentTab;

            const res = await axiosInstance.get("/admin/stores", { params });

            const responseData = res.data;
            const data = Array.isArray(responseData) ? responseData : (responseData.items || []);

            setStores(data);
            setTotalPages(responseData.totalPages || Math.ceil(data.length / pageSize) || 1);

        } catch (err: any) {
            console.error("Lỗi load stores:", err);
            alert("Không thể tải danh sách cửa hàng");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadStores();
    }, [currentTab, currentPage]);

    const handleSearch = () => {
        setCurrentPage(1);
        loadStores();
    };

    const changePage = (newPage: number) => {
        if (newPage < 1 || newPage > totalPages) return;
        setCurrentPage(newPage);
    };

    const updateStoreStatus = async (storeId: string, newStatus: number) => {
        const action = newStatus === 1 ? "Duyệt"
            : newStatus === 2 ? "Từ chối"
                : newStatus === 3 ? "Khóa"
                    : "Cập nhật";

        if (!window.confirm(`Xác nhận ${action} cửa hàng này?`)) return;

        try {
            await axiosInstance.put(`/admin/stores/${storeId}/status`, { status: newStatus });
            alert("Cập nhật trạng thái thành công!");
            loadStores();
        } catch (err: any) {
            alert(err.response?.data?.message || "Cập nhật thất bại");
        }
    };

    const viewDetail = (id: string) => {
        navigate(`/admin/stores/${id}`);        // ← Link này rất quan trọng
    };

    return (
        <div style={{ display: "flex", minHeight: "100vh" }}>
            <AdminSidebar />
            <div style={{ marginLeft: "260px", flex: 1, padding: "30px 40px", background: "#f8f5f0" }}>
                <h1 style={{ fontSize: "36px", fontWeight: "700", color: "#333", marginBottom: "10px" }}>
                    Quản lý Cửa hàng
                </h1>

                <div style={{ display: "flex", gap: "8px", background: "white", padding: "10px", borderRadius: "12px", width: "fit-content", marginBottom: "30px" }}>
                    <TabButton active={currentTab === 1} label="Đang hoạt động" onClick={() => setActiveTab(1)} />
                    <TabButton active={currentTab === 0} label="Cần phê duyệt" onClick={() => setActiveTab(0)} />
                    <TabButton active={currentTab === 2} label="Bị từ chối" onClick={() => setActiveTab(2)} />
                    <TabButton active={currentTab === 3} label="Bị khóa" onClick={() => setActiveTab(3)} />
                </div>

                {/* Search */}
                <div style={{ display: "flex", gap: "12px", marginBottom: "30px" }}>
                    <input
                        type="text"
                        placeholder="Tìm theo tên cửa hàng hoặc địa chỉ..."
                        value={searchTerm}
                        onChange={(e) => setSearchTerm(e.target.value)}
                        style={{
                            padding: "14px 16px",
                            width: "420px",
                            borderRadius: "10px",
                            border: "1px solid #ddd",
                            fontSize: "16px"
                        }}
                    />
                    <button
                        onClick={handleSearch}
                        style={{
                            padding: "14px 32px",
                            background: "#86542B",
                            color: "white",
                            border: "none",
                            borderRadius: "10px",
                            fontWeight: "600",
                            cursor: "pointer"
                        }}
                    >
                        🔍 Tìm kiếm
                    </button>
                </div>

                {loading ? (
                    <div style={{ textAlign: "center", padding: "100px", fontSize: "18px" }}>Đang tải dữ liệu...</div>
                ) : stores.length === 0 ? (
                    <div style={{ textAlign: "center", padding: "80px", background: "white", borderRadius: "16px" }}>
                        Không tìm thấy cửa hàng nào
                    </div>
                ) : (
                    <>
                        <table style={{ width: "100%", background: "white", borderRadius: "16px", overflow: "hidden", boxShadow: "0 4px 20px rgba(0,0,0,0.08)" }}>
                            <thead>
                                <tr style={{ background: "#f9f7f4" }}>
                                    <th style={{ textAlign: "left", padding: "18px 24px" }}>Tên cửa hàng</th>
                                    <th style={{ textAlign: "left", padding: "18px 24px" }}>Chủ sở hữu</th>
                                    <th style={{ textAlign: "left", padding: "18px 24px" }}>Địa chỉ</th>
                                    <th style={{ textAlign: "center", padding: "18px 24px" }}>Trạng thái</th>
                                    <th style={{ textAlign: "center", padding: "18px 24px" }}>Đánh giá</th>
                                    <th style={{ textAlign: "center", padding: "18px 24px" }}>Đơn hoàn thành</th>
                                    <th style={{ textAlign: "center", padding: "18px 24px" }}>Hành động</th>
                                </tr>
                            </thead>
                            <tbody>
                                {stores.map((store) => (
                                    <tr key={store.id} style={{ borderBottom: "1px solid #f0f0f0" }}>
                                        <td style={{ padding: "18px 24px", fontWeight: "600" }}>{store.name}</td>
                                        <td style={{ padding: "18px 24px" }}>
                                            {store.ownerName}<br />
                                            <small style={{ color: "#666" }}>{store.ownerEmail}</small>
                                        </td>
                                        <td style={{ padding: "18px 24px" }}>{store.address}</td>
                                        <td style={{ padding: "18px 24px", textAlign: "center" }}>
                                            <span style={{
                                                padding: "8px 18px",
                                                borderRadius: "9999px",
                                                fontSize: "14px",
                                                color: "white",
                                                backgroundColor: statusColors[store.status] || "#666"
                                            }}>
                                                {statusLabels[store.status]}
                                            </span>
                                        </td>
                                        <td style={{ padding: "18px 24px", textAlign: "center", fontWeight: "600" }}>
                                            {store.averageRating.toFixed(1)} ⭐
                                        </td>
                                        <td style={{ padding: "18px 24px", textAlign: "center", fontWeight: "600" }}>
                                            {store.totalCompletedBookings}
                                        </td>
                                        <td style={{ padding: "18px 24px", textAlign: "center" }}>
                                            <button
                                                onClick={() => viewDetail(store.id)}
                                                style={{
                                                    margin: "0 4px",
                                                    padding: "8px 16px",
                                                    background: "#3b82f6",
                                                    color: "white",
                                                    border: "none",
                                                    borderRadius: "8px",
                                                    cursor: "pointer"
                                                }}
                                            >
                                                Xem chi tiết
                                            </button>

                                            {store.status === 0 && (
                                                <>
                                                    <button onClick={() => updateStoreStatus(store.id, 1)} style={{ margin: "0 4px", padding: "8px 16px", background: "#10b981", color: "white", border: "none", borderRadius: "8px" }}>
                                                        Duyệt
                                                    </button>
                                                    <button onClick={() => updateStoreStatus(store.id, 2)} style={{ margin: "0 4px", padding: "8px 16px", background: "#ef4444", color: "white", border: "none", borderRadius: "8px" }}>
                                                        Từ chối
                                                    </button>
                                                </>
                                            )}

                                            {store.status === 1 && (
                                                <button onClick={() => updateStoreStatus(store.id, 3)} style={{ margin: "0 4px", padding: "8px 16px", background: "#ef4444", color: "white", border: "none", borderRadius: "8px" }}>
                                                    Khóa
                                                </button>
                                            )}

                                            {(store.status === 2 || store.status === 3) && (
                                                <button onClick={() => updateStoreStatus(store.id, 1)} style={{ margin: "0 4px", padding: "8px 16px", background: "#10b981", color: "white", border: "none", borderRadius: "8px" }}>
                                                    Mở lại
                                                </button>
                                            )}
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>

                        {totalPages > 1 && (
                            <div style={{ marginTop: "40px", display: "flex", justifyContent: "center", gap: "16px", alignItems: "center" }}>
                                <button onClick={() => changePage(currentPage - 1)} disabled={currentPage === 1} style={{ padding: "10px 20px", borderRadius: "8px" }}>
                                    ← Trang trước
                                </button>
                                <span style={{ fontSize: "16px" }}>Trang {currentPage} / {totalPages}</span>
                                <button onClick={() => changePage(currentPage + 1)} disabled={currentPage === totalPages} style={{ padding: "10px 20px", borderRadius: "8px" }}>
                                    Trang sau →
                                </button>
                            </div>
                        )}
                    </>
                )}
            </div>
        </div>
    );
}

function TabButton({ active, label, onClick }: { active: boolean; label: string; onClick: () => void }) {
    return (
        <div
            onClick={onClick}
            style={{
                padding: "12px 28px",
                borderRadius: "10px",
                cursor: "pointer",
                fontWeight: active ? "700" : "600",
                background: active ? "#86542B" : "transparent",
                color: active ? "white" : "#555",
            }}
        >
            {label}
        </div>
    );
}