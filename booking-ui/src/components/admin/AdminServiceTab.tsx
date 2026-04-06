// src/components/admin/AdminServiceTab.tsx
import { useState, useEffect } from "react";
import axiosInstance from "../../utils/axiosInstance";

interface Option {
    id: string;
    name: string;
    price: number;
    durationMinutes: number;
    isActive: boolean;
}

interface OptionGroup {
    id: string;
    name: string;
    type: number;
    isRequired: boolean;
    options: Option[];
}

interface Service {
    id: string;
    name: string;
    description?: string;
    price: number;
    durationMinutes: number;
    type: number;
    isActive: boolean;
    optionGroups: OptionGroup[];
}

export default function AdminServiceTab({ storeId }: { storeId: string }) {
    const [services, setServices] = useState<Service[]>([]);
    const [loading, setLoading] = useState(true);
    const [selectedService, setSelectedService] = useState<Service | null>(null);
    const [toggling, setToggling] = useState(false);

    useEffect(() => {
        loadServices();
    }, [storeId]);

    const loadServices = async () => {
        setLoading(true);
        try {
            const res = await axiosInstance.get(`/admin/stores/${storeId}/services`);
            setServices(res.data || []);
        } catch (err) {
            console.error(err);
            alert("Lỗi khi tải danh sách dịch vụ");
        } finally {
            setLoading(false);
        }
    };

    const toggleServiceStatus = async () => {
        if (!selectedService) return;
        const newStatus = !selectedService.isActive;

        if (!window.confirm(`Bạn muốn ${newStatus ? "MỞ" : "KHÓA"} dịch vụ này?`)) return;

        setToggling(true);
        try {
            await axiosInstance.put(`/admin/stores/${storeId}/services/${selectedService.id}/status`, {
                isActive: newStatus
            });

            alert(`Dịch vụ đã được ${newStatus ? "mở" : "khóa"} thành công!`);

            // Cập nhật lại state
            setSelectedService({ ...selectedService, isActive: newStatus });
            loadServices(); // Refresh danh sách
        } catch (err) {
            alert("Cập nhật thất bại");
        } finally {
            setToggling(false);
        }
    };

    if (loading) return <div style={{ padding: "80px", textAlign: "center" }}>Đang tải dịch vụ...</div>;

    return (
        <div style={{ background: "white", padding: "32px", borderRadius: "20px", boxShadow: "0 4px 20px rgba(0,0,0,0.06)" }}>
            <h2>Quản lý Dịch vụ</h2>
            <p style={{ color: "#666", marginBottom: "30px" }}>Nhấn vào dịch vụ để xem chi tiết và mở/khóa</p>

            {/* Danh sách dịch vụ */}
            <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(360px, 1fr))", gap: "20px" }}>
                {services.map(service => (
                    <div
                        key={service.id}
                        onClick={() => setSelectedService(service)}
                        style={{
                            border: "1px solid #eee",
                            borderRadius: "16px",
                            padding: "24px",
                            cursor: "pointer",
                            transition: "all 0.2s",
                            background: service.isActive ? "#fff" : "#f8f8f8"
                        }}
                    >
                        <div style={{ display: "flex", justifyContent: "space-between" }}>
                            <h3 style={{ margin: "0 0 8px 0" }}>{service.name}</h3>
                            <span style={{
                                padding: "6px 14px",
                                borderRadius: "9999px",
                                fontSize: "13px",
                                background: service.isActive ? "#10b981" : "#ef4444",
                                color: "white"
                            }}>
                                {service.isActive ? "Hoạt động" : "Đã khóa"}
                            </span>
                        </div>

                        <p style={{ color: "#555", margin: "8px 0" }}>
                            {service.durationMinutes} phút • {service.price.toLocaleString('vi-VN')}đ
                        </p>

                        {service.optionGroups.length > 0 && (
                            <small style={{ color: "#888" }}>
                                {service.optionGroups.length} nhóm • {service.optionGroups.reduce((sum, g) => sum + g.options.length, 0)} tùy chọn
                            </small>
                        )}
                    </div>
                ))}
            </div>

            {/* ==================== MODAL CHI TIẾT DỊCH VỤ ==================== */}
            {selectedService && (
                <div style={{ position: "fixed", inset: 0, background: "rgba(0,0,0,0.75)", display: "flex", alignItems: "center", justifyContent: "center", zIndex: 2000 }}>
                    <div style={{ background: "white", width: "920px", maxHeight: "92vh", borderRadius: "16px", overflow: "hidden", display: "flex", flexDirection: "column" }}>

                        {/* Header */}
                        <div style={{ padding: "24px", borderBottom: "1px solid #eee", display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                            <h2>{selectedService.name}</h2>
                            <button onClick={() => setSelectedService(null)} style={{ fontSize: "28px", background: "none", border: "none", cursor: "pointer" }}>×</button>
                        </div>

                        {/* Nội dung chi tiết */}
                        <div style={{ padding: "30px", overflowY: "auto", flex: 1 }}>
                            <div style={{ display: "flex", gap: "40px", marginBottom: "30px" }}>
                                <div>
                                    <strong>Giá gốc:</strong> {selectedService.price.toLocaleString('vi-VN')}đ<br />
                                    <strong>Thời lượng:</strong> {selectedService.durationMinutes} phút<br />
                                    <strong>Trạng thái:</strong>
                                    <span style={{ color: selectedService.isActive ? "green" : "red", fontWeight: "600" }}>
                                        {selectedService.isActive ? " Đang hoạt động" : " Đã khóa"}
                                    </span>
                                </div>
                            </div>

                            {selectedService.description && (
                                <div style={{ marginBottom: "30px" }}>
                                    <strong>Mô tả:</strong>
                                    <p>{selectedService.description}</p>
                                </div>
                            )}

                            <h3>Nhóm tùy chọn & Tùy chọn</h3>
                            {selectedService.optionGroups.map((group, gIdx) => (
                                <div key={gIdx} style={{ marginBottom: "28px", padding: "20px", border: "1px solid #eee", borderRadius: "12px" }}>
                                    <h4>{group.name} {group.isRequired && "(Bắt buộc)"}</h4>
                                    <div style={{ marginTop: "12px" }}>
                                        {group.options.map((opt, oIdx) => (
                                            <div key={oIdx} style={{ padding: "12px", borderBottom: "1px solid #f0f0f0", display: "flex", justifyContent: "space-between" }}>
                                                <div>{opt.name}</div>
                                                <div style={{ color: "#666" }}>
                                                    +{opt.price.toLocaleString('vi-VN')}đ • {opt.durationMinutes}p
                                                </div>
                                            </div>
                                        ))}
                                    </div>
                                </div>
                            ))}
                        </div>

                        {/* Footer - Nút hành động */}
                        <div style={{ padding: "24px", borderTop: "1px solid #eee", display: "flex", gap: "12px" }}>
                            <button
                                onClick={toggleServiceStatus}
                                disabled={toggling}
                                style={{
                                    flex: 1,
                                    padding: "16px",
                                    fontSize: "16px",
                                    fontWeight: "600",
                                    border: "none",
                                    borderRadius: "12px",
                                    background: selectedService.isActive ? "#ef4444" : "#10b981",
                                    color: "white"
                                }}
                            >
                                {toggling ? "Đang xử lý..." : selectedService.isActive ? "🔒 Khóa dịch vụ này" : "🔓 Mở dịch vụ này"}
                            </button>
                            <button
                                onClick={() => setSelectedService(null)}
                                style={{ flex: 1, padding: "16px", background: "#666", color: "white", border: "none", borderRadius: "12px" }}
                            >
                                Đóng
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}