// src/components/admin/AdminOrdersTab.tsx
import { useState, useEffect } from "react";
import axiosInstance from "../../utils/axiosInstance";

export default function AdminOrdersTab({ storeId }: { storeId: string }) {
    const [bookings, setBookings] = useState<any[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        loadBookings();
    }, [storeId]);

    const loadBookings = async () => {
        setLoading(true);
        try {
            const res = await axiosInstance.get(`/admin/stores/${storeId}/bookings`);
            setBookings(res.data || []);
        } catch (err) {
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    const getStatusText = (status: number) => {
        switch (status) {
            case 0: return { text: "Chờ xác nhận", color: "#f59e0b" };
            case 1: return { text: "Đã xác nhận", color: "#3b82f6" };
            case 2: return { text: "Đang thực hiện", color: "#8b5cf6" };
            case 3: return { text: "Hoàn thành", color: "#10b981" };
            case 4: return { text: "Đã hủy", color: "#ef4444" };
            default: return { text: "Không xác định", color: "#6b7280" };
        }
    };

    const formatPrice = (price: any) => {
        if (price == null || isNaN(price)) return "0đ";
        return Number(price).toLocaleString('vi-VN') + "đ";
    };

    if (loading) return <div style={{ padding: "80px", textAlign: "center" }}>Đang tải đơn hàng...</div>;

    return (
        <div style={{ background: "white", padding: "32px", borderRadius: "20px", boxShadow: "0 4px 20px rgba(0,0,0,0.06)" }}>
            <h2>Đơn hàng của cửa hàng</h2>
            <p style={{ color: "#666", marginBottom: "24px" }}>Tổng số đơn: {bookings.length}</p>

            <div style={{ display: "flex", flexDirection: "column", gap: "16px" }}>
                {bookings.map(b => {
                    const status = getStatusText(b.status || 0);
                    return (
                        <div key={b.id} style={{
                            border: "1px solid #eee",
                            borderRadius: "16px",
                            padding: "24px",
                            background: "#fff"
                        }}>
                            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start" }}>
                                <div>
                                    <strong style={{ fontSize: "18px" }}>
                                        {b.bookingDate} • {b.timeSlot?.startTime || "--"} - {b.timeSlot?.endTime || "--"}
                                    </strong>
                                    <div style={{ marginTop: "8px", color: "#555" }}>
                                        Khách hàng: <strong>{b.customer?.fullName || "Không rõ"}</strong>
                                        {b.customer?.phoneNumber && ` • ${b.customer.phoneNumber}`}
                                    </div>
                                    <div style={{ color: "#555" }}>
                                        Thú cưng: <strong>{b.pet?.name || "Không có"}</strong>
                                        {b.pet?.species && ` (${b.pet.species})`}
                                    </div>
                                </div>

                                <div style={{ textAlign: "right" }}>
                                    <div style={{ fontSize: "22px", fontWeight: "700", color: "#86542B" }}>
                                        {formatPrice(b.totalPrice)}
                                    </div>
                                    <div style={{
                                        display: "inline-block",
                                        padding: "6px 16px",
                                        borderRadius: "9999px",
                                        fontSize: "13px",
                                        backgroundColor: status.color + "20",
                                        color: status.color,
                                        marginTop: "8px"
                                    }}>
                                        {status.text}
                                    </div>
                                </div>
                            </div>

                            {/* Chi tiết dịch vụ */}
                            {b.services && b.services.length > 0 && (
                                <div style={{ marginTop: "18px", padding: "16px", background: "#f9f9f9", borderRadius: "12px" }}>
                                    <strong>Dịch vụ đã đặt:</strong>
                                    <ul style={{ margin: "10px 0 0 20px", padding: 0 }}>
                                        {b.services.map((s: any, idx: number) => (
                                            <li key={idx} style={{ marginBottom: "4px" }}>
                                                {s.serviceName} — {s.optionName}
                                                <span style={{ color: "#86542B", fontWeight: "600" }}> ({formatPrice(s.price)})</span>
                                            </li>
                                        ))}
                                    </ul>
                                </div>
                            )}

                            {b.notes && (
                                <div style={{ marginTop: "12px", fontStyle: "italic", color: "#666" }}>
                                    Ghi chú: {b.notes}
                                </div>
                            )}
                        </div>
                    );
                })}
            </div>

            {bookings.length === 0 && (
                <div style={{ textAlign: "center", padding: "80px", color: "#888" }}>
                    Chưa có đơn hàng nào cho cửa hàng này.
                </div>
            )}
        </div>
    );
}