// src/pages/admin/PlatformVoucherPage.tsx
import { useState, useEffect } from "react";
import axiosInstance from "../../utils/axiosInstance";
import AdminSidebar from "../../components/admin/AdminSidebar";

export default function PlatformVoucherPage() {
    const [vouchers, setVouchers] = useState<any[]>([]);
    const [loading, setLoading] = useState(true);
    const [showModal, setShowModal] = useState(false);
    const [editing, setEditing] = useState<any>(null);

    const [form, setForm] = useState({
        code: "",
        name: "",
        description: "",
        discountType: 1,           // 1 = Percent, 2 = Fixed
        discountValue: 0,
        minOrderValue: null as number | null,
        maxDiscountAmount: null as number | null,
        usageLimitPerUser: null as number | null,
        totalUsageLimit: null as number | null,
        startDate: new Date().toISOString().split('T')[0],
        endDate: "",
        isActive: true
    });

    useEffect(() => {
        loadVouchers();
    }, []);

    const loadVouchers = async () => {
        setLoading(true);
        try {
            const res = await axiosInstance.get("/admin/platform-vouchers");
            setVouchers(res.data);
        } catch (err) {
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    const openModal = (voucher?: any) => {
        if (voucher) {
            setEditing(voucher);
            setForm({
                code: voucher.code || "",
                name: voucher.name || "",
                description: voucher.description || "",
                discountType: (voucher.discountType === 1 || voucher.discountType === "Percent") ? 1 : 2,
                discountValue: Number(voucher.discountValue) || 0,
                minOrderValue: voucher.minOrderValue ?? null,
                maxDiscountAmount: voucher.maxDiscountAmount ?? null,
                usageLimitPerUser: voucher.usageLimitPerUser ?? null,
                totalUsageLimit: voucher.totalUsageLimit ?? null,
                startDate: voucher.startDate ? voucher.startDate.split('T')[0] : new Date().toISOString().split('T')[0],
                endDate: voucher.endDate ? voucher.endDate.split('T')[0] : "",
                isActive: voucher.isActive !== false
            });
        } else {
            setEditing(null);
            setForm({
                code: "",
                name: "",
                description: "",
                discountType: 1,
                discountValue: 0,
                minOrderValue: null,
                maxDiscountAmount: null,
                usageLimitPerUser: null,
                totalUsageLimit: null,
                startDate: new Date().toISOString().split('T')[0],
                endDate: "",
                isActive: true
            });
        }
        setShowModal(true);
    };

    const handleSave = async () => {
        if (!form.code.trim()) return alert("Vui lòng nhập mã voucher");
        if (form.discountValue <= 0) return alert("Giá trị giảm phải lớn hơn 0");
        if (form.discountType === 1 && form.discountValue > 100) return alert("Phần trăm giảm không được vượt quá 100%");

        try {
            if (editing) {
                await axiosInstance.put(`/admin/platform-vouchers/${editing.id}`, form);
            } else {
                await axiosInstance.post("/admin/platform-vouchers", form);
            }
            alert("Lưu voucher thành công!");
            setShowModal(false);
            loadVouchers();
        } catch (err: any) {
            console.error(err);
            const msg = err.response?.data?.message
                || err.response?.data?.title
                || Object.values(err.response?.data?.errors || {}).flat().join("\n")
                || "Lưu voucher thất bại";
            alert(msg);
        }
    };

    const deleteVoucher = async (id: string) => {
        if (!window.confirm("Bạn có chắc muốn xóa voucher này không?")) return;
        try {
            await axiosInstance.delete(`/admin/platform-vouchers/${id}`);
            loadVouchers();
        } catch (err) {
            alert("Xóa voucher thất bại");
        }
    };

    return (
        <div style={{ display: "flex", minHeight: "100vh" }}>
            <AdminSidebar />

            <div style={{ marginLeft: "260px", flex: 1, padding: "30px 40px", background: "#f8f5f0" }}>
                <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "30px" }}>
                    <h1>Quản lý Platform Voucher (Toàn sàn)</h1>
                    <button
                        onClick={() => openModal()}
                        style={{ padding: "12px 24px", background: "#86542B", color: "white", border: "none", borderRadius: "10px", cursor: "pointer" }}
                    >
                        + Thêm Voucher Mới
                    </button>
                </div>

                <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(380px, 1fr))", gap: "20px" }}>
                    {vouchers.map(v => (
                        <div key={v.id} style={{
                            background: "white",
                            borderRadius: "16px",
                            padding: "24px",
                            border: "1px solid #eee"
                        }}>
                            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                                <h3 style={{ margin: 0, fontSize: "20px" }}>{v.code}</h3>
                                <span style={{
                                    padding: "6px 14px",
                                    borderRadius: "9999px",
                                    fontSize: "13px",
                                    backgroundColor: v.isActive ? "#10b98120" : "#ef444420",
                                    color: v.isActive ? "#10b981" : "#ef4444"
                                }}>
                                    {v.isActive ? "Hoạt động" : "Đã tắt"}
                                </span>
                            </div>

                            {v.name && <p style={{ margin: "8px 0", color: "#444" }}>{v.name}</p>}

                            <p style={{ fontSize: "19px", fontWeight: "700", color: "#d97706", margin: "12px 0" }}>
                                {v.discountType === 1 || v.discountType === "Percent"
                                    ? `${v.discountValue}%`
                                    : `${Number(v.discountValue).toLocaleString('vi-VN')}đ`}
                            </p>

                            <div style={{ fontSize: "13.5px", color: "#555", lineHeight: "1.6" }}>
                                <div>Từ: {new Date(v.startDate).toLocaleDateString('vi-VN')}</div>
                                {v.endDate && <div>Đến: {new Date(v.endDate).toLocaleDateString('vi-VN')}</div>}
                                {v.minOrderValue && <div>Đơn tối thiểu: {Number(v.minOrderValue).toLocaleString('vi-VN')}đ</div>}
                                {v.usageLimitPerUser && <div>Mỗi user dùng tối đa: {v.usageLimitPerUser} lần</div>}
                                {v.totalUsageLimit && <div>Tổng số lần dùng toàn sàn: {v.totalUsageLimit} lần</div>}
                            </div>

                            <div style={{ marginTop: "20px", display: "flex", gap: "10px" }}>
                                <button
                                    onClick={() => openModal(v)}
                                    style={{ flex: 1, padding: "10px", background: "#3b82f6", color: "white", border: "none", borderRadius: "8px" }}
                                >
                                    Sửa
                                </button>
                                <button
                                    onClick={() => deleteVoucher(v.id)}
                                    style={{ flex: 1, padding: "10px", background: "#ef4444", color: "white", border: "none", borderRadius: "8px" }}
                                >
                                    Xóa
                                </button>
                            </div>
                        </div>
                    ))}
                </div>

                {vouchers.length === 0 && !loading && (
                    <div style={{ textAlign: "center", padding: "80px", color: "#888" }}>
                        Chưa có Platform Voucher nào.
                    </div>
                )}
            </div>

            {/* ==================== MODAL ==================== */}
            {showModal && (
                <div style={{ position: "fixed", inset: 0, background: "rgba(0,0,0,0.75)", display: "flex", alignItems: "center", justifyContent: "center", zIndex: 2000 }}>
                    <div style={{ background: "white", width: "640px", borderRadius: "16px", padding: "28px", maxHeight: "92vh", overflowY: "auto" }}>
                        <h2>{editing ? "Sửa Platform Voucher" : "Thêm Platform Voucher Mới"}</h2>

                        <div style={{ marginTop: "20px" }}>
                            <label>Mã Voucher <span style={{ color: "red" }}>*</span></label>
                            <input
                                value={form.code}
                                onChange={e => setForm({ ...form, code: e.target.value.toUpperCase().trim() })}
                                placeholder="Ví dụ: WELCOME2025"
                                style={{ width: "100%", padding: "12px", marginTop: "6px", borderRadius: "8px", border: "1px solid #ddd" }}
                            />
                        </div>

                        <div style={{ marginTop: "16px" }}>
                            <label>Tên voucher</label>
                            <input
                                value={form.name}
                                onChange={e => setForm({ ...form, name: e.target.value })}
                                placeholder="Tên hiển thị (không bắt buộc)"
                                style={{ width: "100%", padding: "12px", marginTop: "6px", borderRadius: "8px", border: "1px solid #ddd" }}
                            />
                        </div>

                        <div style={{ marginTop: "16px" }}>
                            <label>Loại giảm giá</label>
                            <select
                                value={form.discountType}
                                onChange={e => setForm({ ...form, discountType: parseInt(e.target.value) })}
                                style={{ width: "100%", padding: "12px", marginTop: "6px", borderRadius: "8px", border: "1px solid #ddd" }}
                            >
                                <option value={1}>Giảm theo phần trăm (%)</option>
                                <option value={2}>Giảm số tiền cố định (đ)</option>
                            </select>
                        </div>

                        <div style={{ marginTop: "16px" }}>
                            <label>Giá trị giảm <span style={{ color: "red" }}>*</span></label>
                            <input
                                type="number"
                                value={form.discountValue}
                                onChange={e => setForm({ ...form, discountValue: parseFloat(e.target.value) || 0 })}
                                style={{ width: "100%", padding: "12px", marginTop: "6px", borderRadius: "8px", border: "1px solid #ddd" }}
                            />
                        </div>

                        <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "16px", marginTop: "16px" }}>
                            <div>
                                <label>Đơn tối thiểu (đ)</label>
                                <input
                                    type="number"
                                    value={form.minOrderValue || ""}
                                    onChange={e => setForm({ ...form, minOrderValue: e.target.value ? parseFloat(e.target.value) : null })}
                                    style={{ width: "100%", padding: "12px", marginTop: "6px", borderRadius: "8px", border: "1px solid #ddd" }}
                                />
                            </div>
                            <div>
                                <label>Giới hạn giảm tối đa (đ)</label>
                                <input
                                    type="number"
                                    value={form.maxDiscountAmount || ""}
                                    onChange={e => setForm({ ...form, maxDiscountAmount: e.target.value ? parseFloat(e.target.value) : null })}
                                    style={{ width: "100%", padding: "12px", marginTop: "6px", borderRadius: "8px", border: "1px solid #ddd" }}
                                />
                            </div>
                        </div>

                        <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "16px", marginTop: "16px" }}>
                            <div>
                                <label>Số lần dùng tối đa / user</label>
                                <input
                                    type="number"
                                    value={form.usageLimitPerUser || ""}
                                    onChange={e => setForm({ ...form, usageLimitPerUser: e.target.value ? parseInt(e.target.value) : null })}
                                    style={{ width: "100%", padding: "12px", marginTop: "6px", borderRadius: "8px", border: "1px solid #ddd" }}
                                />
                            </div>
                            <div>
                                <label>Tổng số lần dùng toàn sàn</label>
                                <input
                                    type="number"
                                    value={form.totalUsageLimit || ""}
                                    onChange={e => setForm({ ...form, totalUsageLimit: e.target.value ? parseInt(e.target.value) : null })}
                                    style={{ width: "100%", padding: "12px", marginTop: "6px", borderRadius: "8px", border: "1px solid #ddd" }}
                                />
                            </div>
                        </div>

                        <div style={{ marginTop: "20px" }}>
                            <label>Thời gian hiệu lực</label>
                            <div style={{ display: "flex", gap: "12px", marginTop: "8px" }}>
                                <input
                                    type="date"
                                    value={form.startDate}
                                    onChange={e => setForm({ ...form, startDate: e.target.value })}
                                    style={{ flex: 1, padding: "10px", borderRadius: "8px", border: "1px solid #ddd" }}
                                />
                                <input
                                    type="date"
                                    value={form.endDate}
                                    onChange={e => setForm({ ...form, endDate: e.target.value })}
                                    style={{ flex: 1, padding: "10px", borderRadius: "8px", border: "1px solid #ddd" }}
                                />
                            </div>
                        </div>

                        <div style={{ marginTop: "24px" }}>
                            <label>
                                <input
                                    type="checkbox"
                                    checked={form.isActive}
                                    onChange={e => setForm({ ...form, isActive: e.target.checked })}
                                />
                                Voucher đang hoạt động
                            </label>
                        </div>

                        <div style={{ marginTop: "30px", display: "flex", gap: "12px" }}>
                            <button
                                onClick={handleSave}
                                style={{ flex: 1, padding: "14px", background: "#86542B", color: "white", border: "none", borderRadius: "10px", fontWeight: "600" }}
                            >
                                {editing ? "Cập nhật Voucher" : "Tạo Voucher Mới"}
                            </button>
                            <button
                                onClick={() => setShowModal(false)}
                                style={{ flex: 1, padding: "14px", background: "#666", color: "white", border: "none", borderRadius: "10px" }}
                            >
                                Hủy
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}