// src/pages/vendor/OrdersPage.tsx
import { useState, useEffect } from "react";
import axiosInstance from '../../utils/axiosInstance';
import VendorSidebar from '../../components/vendor/VendorSidebar';
interface Booking {
    id: string;
    petName: string;
    customerName: string;
    serviceName: string;
    bookingDate: string;
    startTime: string;
    endTime: string;
    status: number;
    totalPrice: number;
}

interface OrderDetail {
    id: string;
    bookingDate: string;
    startTime: string;
    endTime: string;
    status: number;
    totalPrice: number;
    notes?: string;

    petName: string;
    petSpecies: string;
    petBreed?: string;
    petGender?: string;
    petDateOfBirth?: string;
    petColor?: string;
    petWeight?: number;
    petNotes?: string;
    petProfileImageUrl?: string;

    customerName: string;

    mainServiceName: string;
    options: Array<{ optionName: string; price: number }>;

    platformVoucherCode?: string;
    storeVoucherCode?: string;
    platformDiscount?: number;
    storeDiscount?: number;
}

const statusMap: any = {
    0: { text: "Chờ xác nhận", color: "#f59e0b" },
    1: { text: "Đã nhận", color: "#3b82f6" },
    2: { text: "Đang chăm sóc", color: "#8b5cf6" },
    3: { text: "Chờ lấy", color: "#ec4899" },
    4: { text: "Hoàn thành", color: "#10b981" },
    5: { text: "Đã hủy", color: "#ef4444" }
};

const nextStatusMap: any = {
    0: 1, // Pending → Received
    1: 2, // Received → Caring
    2: 3, // Caring → WaitingPickup
    3: 4  // WaitingPickup → Completed
};

export default function OrdersPage() {
    const [activeTab, setActiveTab] = useState<"calendar" | "pending" | "completed" | "cancelled">("calendar");
    const [orders, setOrders] = useState<Booking[]>([]);
    const [loading, setLoading] = useState(true);
    const [selectedOrderDetail, setSelectedOrderDetail] = useState<OrderDetail | null>(null);
    const [updating, setUpdating] = useState(false);

    const [currentWeekStart, setCurrentWeekStart] = useState<Date>(getThisWeekStart());

    const loadData = async () => {
        setLoading(true);
        let url = '';

        if (activeTab === "calendar") {
            const startStr = currentWeekStart.toISOString().split('T')[0];
            url = `/vendor/orders/calendar?weekStart=${startStr}`;
        } else if (activeTab === "pending") url = '/vendor/orders/pending';
        else if (activeTab === "completed") url = '/vendor/orders/completed';
        else if (activeTab === "cancelled") url = '/vendor/orders/cancelled';

        try {
            const res = await axiosInstance.get(url);
            setOrders(res.data || []);
        } catch (err) {
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadData();
    }, [activeTab, currentWeekStart]);

    const changeWeek = (days: number) => {
        const newDate = new Date(currentWeekStart);
        newDate.setDate(newDate.getDate() + days);
        setCurrentWeekStart(newDate);
    };

    const openDetail = async (id: string) => {
        try {
            const res = await axiosInstance.get(`/vendor/orders/${id}`);
            setSelectedOrderDetail(res.data);
        } catch (err) {
            alert("Không tải được chi tiết đơn hàng");
        }
    };

    const closeModal = () => setSelectedOrderDetail(null);

    const updateStatus = async () => {
        if (!selectedOrderDetail || updating) return;
        if (selectedOrderDetail.status === 4 || selectedOrderDetail.status === 5) {
            alert("Không thể thay đổi trạng thái của đơn đã hoàn thành hoặc đã hủy");
            return;
        }

        const nextStatus = nextStatusMap[selectedOrderDetail.status];
        if (nextStatus === undefined) {
            alert("Không thể chuyển sang trạng thái tiếp theo");
            return;
        }

        setUpdating(true);
        try {
            await axiosInstance.put(`/vendor/orders/${selectedOrderDetail.id}/status`, { status: nextStatus });
            alert("Cập nhật trạng thái thành công!");
            closeModal();
            loadData(); // reload danh sách
        } catch (err: any) {
            alert(err.response?.data || "Cập nhật trạng thái thất bại");
        } finally {
            setUpdating(false);
        }
    };

    const groupedByDate = orders.reduce((acc: any, order) => {
        if (!acc[order.bookingDate]) acc[order.bookingDate] = [];
        acc[order.bookingDate].push(order);
        return acc;
    }, {});

    return (
        <div style={{ display: "flex", minHeight: "100vh" }}>
            <VendorSidebar />
            <div style={{
                marginLeft: "260px",
                flex: 1,
                padding: "30px 40px",
                background: "#f8f5f0",
                minHeight: "100vh"
            }}>
                <div style={{ maxWidth: "1400px", margin: "0 auto" }}>

                <h1 style={{ fontSize: "36px", fontWeight: "700", color: "#333", marginBottom: "8px" }}>Quản Lý Đơn Hàng</h1>

                <div style={{ display: "flex", gap: "8px", background: "white", padding: "8px", borderRadius: "12px", width: "fit-content", margin: "30px 0", boxShadow: "0 2px 10px rgba(0,0,0,0.05)" }}>
                    <TabButton active={activeTab === "calendar"} onClick={() => setActiveTab("calendar")} label="📅 Lịch Tuần" />
                    <TabButton active={activeTab === "pending"} onClick={() => setActiveTab("pending")} label="⏳ Chưa hoàn thành" />
                    <TabButton active={activeTab === "completed"} onClick={() => setActiveTab("completed")} label="✅ Đã hoàn thành" />
                    <TabButton active={activeTab === "cancelled"} onClick={() => setActiveTab("cancelled")} label="❌ Đã hủy" />
                </div>

                {/* Lịch Tuần */}
                {activeTab === "calendar" && (
                    <div>
                        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "24px" }}>
                            <button onClick={() => changeWeek(-7)} style={{ padding: "10px 24px", borderRadius: "8px", border: "1px solid #ddd", background: "white", cursor: "pointer" }}>← Tuần trước</button>
                            <h2 style={{ margin: 0, color: "#333" }}>{currentWeekStart.toLocaleDateString('vi-VN', { month: 'long', year: 'numeric' })}</h2>
                            <button onClick={() => changeWeek(7)} style={{ padding: "10px 24px", borderRadius: "8px", border: "1px solid #ddd", background: "white", cursor: "pointer" }}>Tuần sau →</button>
                        </div>

                        <div style={{ display: "grid", gridTemplateColumns: "repeat(7, 1fr)", gap: "12px" }}>
                            {Array.from({ length: 7 }).map((_, i) => {
                                const date = new Date(currentWeekStart);
                                date.setDate(date.getDate() + i);
                                const dateStr = date.toISOString().split('T')[0];
                                const dayOrders = groupedByDate[dateStr] || [];

                                return (
                                    <div key={i} style={{ background: "white", borderRadius: "16px", padding: "16px", boxShadow: "0 4px 15px rgba(0,0,0,0.08)", minHeight: "380px" }}>
                                        <div style={{ textAlign: "center", padding: "12px", background: "#f9f7f4", borderRadius: "12px", marginBottom: "16px", fontWeight: "600", color: "#86542B" }}>
                                            {date.toLocaleDateString('vi-VN', { weekday: 'short', day: 'numeric', month: 'short' })}
                                        </div>

                                        {dayOrders.length > 0 ? dayOrders.sort((a, b) => a.startTime.localeCompare(b.startTime)).map(order => (
                                            <div key={order.id} onClick={() => openDetail(order.id)} style={{
                                                padding: "14px", background: "#fdfaf5", borderRadius: "12px", marginBottom: "10px",
                                                borderLeft: `4px solid ${statusMap[order.status].color}`, cursor: "pointer"
                                            }}>
                                                <div style={{ fontWeight: "600", fontSize: "15px" }}>{order.petName}</div>
                                                <div style={{ fontSize: "13px", color: "#555" }}>
                                                    {order.startTime} - {order.endTime} • {order.customerName}
                                                </div>
                                                <div style={{ fontSize: "13px", color: statusMap[order.status].color, marginTop: "4px" }}>
                                                    {statusMap[order.status].text}
                                                </div>
                                            </div>
                                        )) : (
                                            <div style={{ color: "#aaa", textAlign: "center", padding: "60px 20px", fontSize: "14px" }}>Không có đơn nào</div>
                                        )}
                                    </div>
                                );
                            })}
                        </div>
                    </div>
                )}

                {/* Danh sách các tab khác */}
                {activeTab !== "calendar" && (
                    <div>
                        {loading ? <div style={{ textAlign: "center", padding: "80px" }}>Đang tải...</div> : orders.length === 0 ? (
                            <div style={{ textAlign: "center", padding: "80px", background: "white", borderRadius: "20px" }}>Không có đơn hàng</div>
                        ) : (
                            <div style={{ display: "flex", flexDirection: "column", gap: "16px" }}>
                                {orders.map(o => (
                                    <div key={o.id} onClick={() => openDetail(o.id)} style={{
                                        background: "white", padding: "24px", borderRadius: "20px",
                                        boxShadow: "0 4px 15px rgba(0,0,0,0.06)", display: "flex",
                                        justifyContent: "space-between", alignItems: "center", cursor: "pointer"
                                    }}>
                                        <div>
                                            <div style={{ fontWeight: "600" }}>{o.petName} • {o.customerName}</div>
                                            <div style={{ color: "#555" }}>
                                                {o.serviceName} • {o.bookingDate} | {o.startTime} - {o.endTime}
                                            </div>
                                        </div>
                                        <div style={{ textAlign: "right" }}>
                                            <div style={{ background: statusMap[o.status].bg || "#f3f4f6", color: statusMap[o.status].color, padding: "6px 18px", borderRadius: "30px", display: "inline-block" }}>
                                                {statusMap[o.status].text}
                                            </div>
                                            <div style={{ marginTop: "10px", fontWeight: "700", color: "#86542B" }}>
                                                {o.totalPrice.toLocaleString()}đ
                                            </div>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>
                )}

                {/* ==================== MODAL CHI TIẾT ĐẦY ĐỦ ==================== */}
                {/* ==================== MODAL CHI TIẾT ĐƠN HÀNG ==================== */}
                {selectedOrderDetail && (
                    <div style={{
                        position: "fixed", top: 0, left: 0, width: "100%", height: "100%",
                        background: "rgba(0,0,0,0.75)", display: "flex", alignItems: "center",
                        justifyContent: "center", zIndex: 1000
                    }} onClick={closeModal}>
                        <div style={{
                            background: "white", width: "90%", maxWidth: "760px", borderRadius: "20px",
                            padding: "30px", maxHeight: "92vh", overflowY: "auto"
                        }} onClick={e => e.stopPropagation()}>

                            <h2 style={{ marginBottom: "24px", color: "#333" }}>Chi Tiết Đơn Hàng</h2>

                            {/* ==================== THÔNG TIN THÚ CƯNG (Card đẹp) ==================== */}
                            <div style={{
                                background: "#fafafa", borderRadius: "16px", padding: "20px", marginBottom: "28px",
                                border: "1px solid #eee"
                            }}>
                                <div style={{ position: "relative", marginBottom: "16px" }}>
                                    <img
                                        src={selectedOrderDetail.petProfileImageUrl || "https://cdn-icons-png.flaticon.com/512/616/616408.png"}
                                        style={{ width: "100%", height: "220px", objectFit: "cover", borderRadius: "12px" }}
                                        alt="Pet"
                                    />
                                    <div style={{
                                        position: "absolute", top: "16px", right: "16px",
                                        background: "rgba(0,0,0,0.75)", color: "white", padding: "6px 14px",
                                        borderRadius: "8px", fontSize: "14px", fontWeight: "500"
                                    }}>
                                        {statusMap[selectedOrderDetail.status].text}
                                    </div>
                                </div>

                                <div style={{ fontSize: "18px", fontWeight: "600", marginBottom: "12px" }}>
                                    {selectedOrderDetail.petName}
                                </div>

                                <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "12px", fontSize: "14.5px" }}>
                                    <div><strong>Loài:</strong> {selectedOrderDetail.petSpecies}</div>
                                    <div><strong>Giống:</strong> {selectedOrderDetail.petBreed || "-"}</div>
                                    <div><strong>Giới tính:</strong> {selectedOrderDetail.petGender || "-"}</div>
                                    <div><strong>Ngày sinh:</strong> {selectedOrderDetail.petDateOfBirth || "-"}</div>
                                    <div><strong>Màu lông:</strong> {selectedOrderDetail.petColor || "-"}</div>
                                    <div><strong>Cân nặng:</strong> {selectedOrderDetail.petWeight ? `${selectedOrderDetail.petWeight} kg` : "-"}</div>
                                </div>

                                {selectedOrderDetail.petNotes && (
                                    <div style={{ marginTop: "12px" }}>
                                        <strong>Ghi chú:</strong> {selectedOrderDetail.petNotes}
                                    </div>
                                )}
                            </div>

                            {/* ==================== THÔNG TIN ĐẶT LỊCH ==================== */}
                            <div style={{ marginBottom: "24px" }}>
                                <h3 style={{ marginBottom: "12px" }}>Thông tin đặt lịch</h3>
                                <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "16px", fontSize: "15px" }}>
                                    <div><strong>Khách hàng:</strong> {selectedOrderDetail.customerName}</div>
                                    <div><strong>Ngày:</strong> {selectedOrderDetail.bookingDate}</div>
                                    <div><strong>Giờ:</strong> {selectedOrderDetail.startTime} - {selectedOrderDetail.endTime}</div>
                                    
                                </div>
                            </div>

                            {/* ==================== CHI TIẾT DỊCH VỤ ==================== */}
                            {selectedOrderDetail.options.length > 0 && (
                                <div style={{ marginBottom: "24px" }}>
                                    <h3 style={{ marginBottom: "12px" }}>Chi tiết dịch vụ</h3>
                                    <div><strong>Dịch vụ chính:</strong> {selectedOrderDetail.mainServiceName}</div>
                                    <div style={{ background: "#f9f9f9", padding: "16px", borderRadius: "12px" }}>
                                        {selectedOrderDetail.options.map((opt, idx) => (
                                            <div key={idx} style={{
                                                display: "flex", justifyContent: "space-between", padding: "8px 0",
                                                borderBottom: idx !== selectedOrderDetail.options.length - 1 ? "1px solid #eee" : "none"
                                            }}>
                                                <span>{opt.optionName}</span>
                                                <span style={{ fontWeight: "600" }}>+{opt.price.toLocaleString()}đ</span>
                                            </div>
                                        ))}
                                    </div>
                                </div>
                            )}

                            {/* ==================== VOUCHER & TỔNG TIỀN ==================== */}
                            {(selectedOrderDetail.platformVoucherCode || selectedOrderDetail.storeVoucherCode) && (
                                <div style={{ marginBottom: "20px" }}>
                                    <h3>Voucher đã áp dụng</h3>
                                    {selectedOrderDetail.platformVoucherCode && (
                                        <div>Voucher sàn: <strong>{selectedOrderDetail.platformVoucherCode}</strong> (-{selectedOrderDetail.platformDiscount?.toLocaleString()}đ)</div>
                                    )}
                                    {selectedOrderDetail.storeVoucherCode && (
                                        <div>Voucher cửa hàng: <strong>{selectedOrderDetail.storeVoucherCode}</strong> (-{selectedOrderDetail.storeDiscount?.toLocaleString()}đ)</div>
                                    )}
                                </div>
                            )}

                            <div style={{ fontSize: "24px", fontWeight: "700", textAlign: "right", color: "#86542B", margin: "24px 0" }}>
                                Tổng tiền: {selectedOrderDetail.totalPrice.toLocaleString()}đ
                            </div>

                            {/* ==================== NÚT CẬP NHẬT TRẠNG THÁI ==================== */}
                            {selectedOrderDetail.status !== 4 && selectedOrderDetail.status !== 5 && (
                                <button
                                    onClick={updateStatus}
                                    disabled={updating}
                                    style={{
                                        width: "100%", padding: "16px", marginBottom: "12px",
                                        background: "#86542B", color: "white", border: "none",
                                        borderRadius: "12px", fontSize: "16px", fontWeight: "600",
                                        cursor: updating ? "not-allowed" : "pointer"
                                    }}
                                >
                                    {updating ? "Đang cập nhật..." : getNextStatusText(selectedOrderDetail.status)}
                                </button>
                            )}

                            <button onClick={closeModal} style={{
                                width: "100%", padding: "16px", background: "#666", color: "white",
                                border: "none", borderRadius: "12px", fontSize: "16px", cursor: "pointer"
                            }}>
                                Đóng
                            </button>
                        </div>
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

function getThisWeekStart() {
    const today = new Date();
    const day = today.getDay();
    const diff = today.getDate() - day + (day === 0 ? -6 : 1);
    const monday = new Date(today);
    monday.setDate(diff);
    return monday;
}

const getNextStatusText = (currentStatus: number) => {
    switch (currentStatus) {
        case 0: return "Shop Đã Nhận Bé";
        case 1: return "Bắt đầu chăm sóc cho bé";
        case 2: return "Hoàn thành chăm sóc cho bé";
        case 3: return "Bé đã được đón";
        default: return "Chuyển trạng thái tiếp theo";
    }
};