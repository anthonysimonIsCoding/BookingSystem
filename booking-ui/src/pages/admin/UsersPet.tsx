// src/pages/admin/UsersPage.tsx
import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import axiosInstance from "../../utils/axiosInstance";
import AdminSidebar from "../../components/admin/AdminSidebar";

export default function UsersPet() {
    const navigate = useNavigate();
    const [activeTab, setActiveTab] = useState<boolean>(true); // true = Hoạt động
    const [users, setUsers] = useState<any[]>([]);
    const [loading, setLoading] = useState(false);
    const [currentPage, setCurrentPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const pageSize = 20;

    const loadUsers = async () => {
        setLoading(true);
        try {
            const res = await axiosInstance.get("/admin/users", {
                params: {
                    isActive: activeTab,
                    page: currentPage,
                    pageSize
                }
            });
            setUsers(res.data.items || []);
            setTotalPages(res.data.totalPages || 1);
        } catch (err) {
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadUsers();
    }, [activeTab, currentPage]);

    return (
        <div style={{ display: "flex", minHeight: "100vh" }}>
            <AdminSidebar />

            <div style={{ marginLeft: "260px", flex: 1, padding: "30px 40px", background: "#f8f5f0" }}>
                <h1 style={{ marginBottom: "8px" }}>Quản lý Người dùng (Customer)</h1>
                <p style={{ color: "#666" }}>Chỉ hiển thị tài khoản khách hàng</p>

                {/* Tabs */}
                <div style={{ display: "flex", gap: "10px", margin: "30px 0" }}>
                    <TabButton
                        active={activeTab === true}
                        label="Người dùng hoạt động"
                        onClick={() => { setActiveTab(true); setCurrentPage(1); }}
                    />
                    <TabButton
                        active={activeTab === false}
                        label="Người dùng bị hạn chế"
                        onClick={() => { setActiveTab(false); setCurrentPage(1); }}
                    />
                </div>

                <table style={{ width: "100%", background: "white", borderRadius: "12px", overflow: "hidden" }}>
                    <thead>
                        <tr style={{ background: "#f0e9df" }}>
                            <th style={{ padding: "18px", textAlign: "left" }}>Họ tên</th>
                            <th style={{ padding: "18px", textAlign: "left" }}>Email</th>
                            <th style={{ padding: "18px" }}>Số điện thoại</th>
                            <th style={{ padding: "18px" }}>Số Pet</th>
                            <th style={{ padding: "18px" }}>Ngày tạo</th>
                            <th style={{ padding: "18px" }}>Hành động</th>
                        </tr>
                    </thead>
                    <tbody>
                        {users.map(u => (
                            <tr key={u.id} style={{ borderTop: "1px solid #eee" }}>
                                <td style={{ padding: "18px" }}>{u.fullName}</td>
                                <td style={{ padding: "18px" }}>{u.email}</td>
                                <td style={{ padding: "18px", textAlign: "center" }}>{u.phoneNumber || "-"}</td>
                                <td style={{ padding: "18px", textAlign: "center" }}>{u.petCount}</td>
                                <td style={{ padding: "18px", textAlign: "center", color: "#666" }}>
                                    {new Date(u.createdAt).toLocaleDateString('vi-VN')}
                                </td>
                                <td style={{ padding: "18px", textAlign: "center" }}>
                                    <button
                                        onClick={() => navigate(`/admin/users/${u.id}`)}
                                        style={{ padding: "8px 20px", background: "#86542B", color: "white", border: "none", borderRadius: "8px", cursor: "pointer" }}
                                    >
                                        Xem chi tiết
                                    </button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>

                {/* Pagination */}
                {totalPages > 1 && (
                    <div style={{ marginTop: "30px", textAlign: "center" }}>
                        <button onClick={() => setCurrentPage(p => Math.max(1, p - 1))} disabled={currentPage === 1}>← Trước</button>
                        <span style={{ margin: "0 20px", fontSize: "16px" }}>Trang {currentPage} / {totalPages}</span>
                        <button onClick={() => setCurrentPage(p => Math.min(totalPages, p + 1))} disabled={currentPage === totalPages}>Sau →</button>
                    </div>
                )}
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