// src/components/vendor/VendorSidebar.tsx
import { useNavigate, useLocation } from "react-router-dom";

const menuItems = [
    { key: "storelist", label: "Danh Sách Cửa Hàng", icon: "🏬", path: "/admin" },
    { key: "userlist", label: "Danh Sách Người Dùng", icon: "👤", path: "/admin/users" },
    { key: "catelist", label: "Quản Lý Danh Mục", icon: "🗂️", path: "/admin/md" },
    { key: "voucherlist", label: "Quản Lý Voucher", icon: "🧶", path: "/admin/vouchers" },
];

export default function AdminSidebar() {
    const navigate = useNavigate();
    const location = useLocation();

    return (
        <div style={{
            position: "fixed",
            left: 0,
            top: 0,
            bottom: 0,
            width: "260px",
            background: "#fff",
            boxShadow: "2px 0 12px rgba(0,0,0,0.08)",
            padding: "24px 0",
            zIndex: 100,
            overflowY: "auto"
        }}>
            {/* Logo / Tiêu đề */}
            <div style={{
                padding: "0 28px 32px",
                borderBottom: "1px solid #eee",
                marginBottom: "20px"
            }}>
                <h2 style={{ margin: 0, color: "#86542B", fontSize: "26px", fontWeight: "700" }}>
                    PetBooking
                </h2>
                <p style={{ margin: "6px 0 0", color: "#666", fontSize: "14px" }}>Vendor Dashboard</p>
            </div>

            {/* Menu */}
            <div style={{ padding: "0 12px" }}>
                {menuItems.map(item => {
                    const isActive = location.pathname === item.path ||
                        (item.key === "dashboard" && location.pathname === "/vendor");

                    return (
                        <div
                            key={item.key}
                            onClick={() => navigate(item.path)}
                            style={{
                                display: "flex",
                                alignItems: "center",
                                gap: "14px",
                                padding: "14px 20px",
                                margin: "4px 8px",
                                borderRadius: "12px",
                                cursor: "pointer",
                                background: isActive ? "#86542B" : "transparent",
                                color: isActive ? "white" : "#333",
                                fontWeight: isActive ? "600" : "500",
                                transition: "all 0.2s"
                            }}
                        >
                            <span style={{ fontSize: "20px" }}>{item.icon}</span>
                            <span>{item.label}</span>
                        </div>
                    );
                })}
            </div>

            {/* Footer */}
            <div style={{
                position: "absolute",
                bottom: "30px",
                left: "28px",
                right: "28px",
                fontSize: "13px",
                color: "#999",
                textAlign: "center"
            }}>
                © 2026 PetBooking Vendor
            </div>
        </div>
    );
}