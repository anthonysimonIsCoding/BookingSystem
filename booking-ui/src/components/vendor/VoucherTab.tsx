import { useState, useEffect } from "react";
import axiosInstance from '../../utils/axiosInstance';

interface Voucher {
    id: string;
    code: string;
    name?: string;
    description?: string;
    discountType: number;        // 1 = Percent, 2 = Fixed
    discountValue: number;
    minOrderValue?: number;
    maxDiscountAmount?: number;
    usageLimitPerUser?: number;
    totalUsageLimit?: number;
    startDate: string;
    endDate?: string;
    isActive: boolean;
    applicableServiceName?: string;
    applicableSpeciesName?: string;
    usedCount: number;
    totalDiscountApplied: number;
}

export default function VoucherTab() {
    const [vouchers, setVouchers] = useState<Voucher[]>([]);
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);

    const [showModal, setShowModal] = useState(false);
    const [editingVoucher, setEditingVoucher] = useState<Voucher | null>(null);

    // Form state
    const [form, setForm] = useState({
        code: "",
        name: "",
        description: "",
        discountType: 1,           // 1 = Percent, 2 = Fixed
        discountValue: 10,
        minOrderValue: "",
        maxDiscountAmount: "",
        usageLimitPerUser: "",
        totalUsageLimit: "",
        startDate: new Date().toISOString().split('T')[0],
        endDate: "",
        isActive: true,
        applicableServiceId: "",   // Để sau này có thể chọn từ danh sách service
        applicableSpeciesId: ""    // Để sau này có thể chọn từ danh sách species
    });

    useEffect(() => {
        loadVouchers();
    }, []);

    const loadVouchers = async () => {
        setLoading(true);
        try {
            const res = await axiosInstance.get('/vendor/store/vouchers');
            setVouchers(res.data || []);
        } catch (err: any) {
            console.error(err);
            alert("Lỗi tải danh sách voucher");
        } finally {
            setLoading(false);
        }
    };

    const openModal = (voucher?: Voucher) => {
        if (voucher) {
            setEditingVoucher(voucher);
            setForm({
                code: voucher.code,
                name: voucher.name || "",
                description: voucher.description || "",
                discountType: voucher.discountType,
                discountValue: voucher.discountValue,
                minOrderValue: voucher.minOrderValue?.toString() || "",
                maxDiscountAmount: voucher.maxDiscountAmount?.toString() || "",
                usageLimitPerUser: voucher.usageLimitPerUser?.toString() || "",
                totalUsageLimit: voucher.totalUsageLimit?.toString() || "",
                startDate: voucher.startDate.split('T')[0],
                endDate: voucher.endDate ? voucher.endDate.split('T')[0] : "",
                isActive: voucher.isActive,
                applicableServiceId: "",   // Tạm thời để trống (có thể mở rộng sau)
                applicableSpeciesId: ""
            });
        } else {
            setEditingVoucher(null);
            setForm({
                code: "",
                name: "",
                description: "",
                discountType: 1,
                discountValue: 10,
                minOrderValue: "",
                maxDiscountAmount: "",
                usageLimitPerUser: "1",
                totalUsageLimit: "100",
                startDate: new Date().toISOString().split('T')[0],
                endDate: "",
                isActive: true,
                applicableServiceId: "",
                applicableSpeciesId: ""
            });
        }
        setShowModal(true);
    };

    const closeModal = () => {
        setShowModal(false);
        setEditingVoucher(null);
    };

    const handleInputChange = (field: string, value: any) => {
        setForm(prev => ({ ...prev, [field]: value }));
    };

    const handleSave = async () => {
    if (!form.code.trim()) {
        alert("Vui lòng nhập mã voucher");
        return;
    }
    if (form.discountValue <= 0) {
        alert("Giá trị giảm giá phải lớn hơn 0");
        return;
    }
    if (!form.startDate) {
        alert("Vui lòng chọn ngày bắt đầu");
        return;
    }

    setSaving(true);
    try {
        const payload = {
            code: form.code.trim().toUpperCase(),
            name: form.name.trim() || null,
            description: form.description.trim() || null,
            discountType: form.discountType,
            discountValue: form.discountValue,
            minOrderValue: form.minOrderValue ? parseFloat(form.minOrderValue) : null,
            maxDiscountAmount: form.maxDiscountAmount ? parseFloat(form.maxDiscountAmount) : null,
            usageLimitPerUser: form.usageLimitPerUser ? parseInt(form.usageLimitPerUser) : null,
            totalUsageLimit: form.totalUsageLimit ? parseInt(form.totalUsageLimit) : null,
            startDate: form.startDate,
            endDate: form.endDate && form.endDate.trim() !== "" ? form.endDate : null,   // ← Sửa ở đây
            isActive: form.isActive,
            applicableServiceId: form.applicableServiceId || null,
            applicableSpeciesId: form.applicableSpeciesId || null,
        };

        if (editingVoucher) {
            await axiosInstance.put(`/vendor/store/vouchers/${editingVoucher.id}`, payload);
            alert("✅ Cập nhật voucher thành công!");
        } else {
            await axiosInstance.post('/vendor/store/vouchers', payload);
            alert("✅ Tạo voucher mới thành công!");
        }

        closeModal();
        loadVouchers();
    } catch (err: any) {
        console.error(err.response?.data);
        alert(err.response?.data?.errors?.endDate?.[0] || err.response?.data?.message || "Lưu voucher thất bại");
    } finally {
        setSaving(false);
    }
};

    const deleteVoucher = async (id: string) => {
        if (!window.confirm("Bạn có chắc muốn xóa voucher này không?")) return;
        try {
            await axiosInstance.delete(`/vendor/store/vouchers/${id}`);
            alert("Xóa voucher thành công!");
            loadVouchers();
        } catch (err: any) {
            alert("Xóa thất bại");
        }
    };

    const formatDiscount = (type: number, value: number) => {
        return type === 1 ? `${value}%` : `${value.toLocaleString('vi-VN')}đ`;
    };

    if (loading) return <div style={{ padding: "80px", textAlign: "center" }}>Đang tải voucher...</div>;

    return (
        <div style={{ background: "white", padding: "32px", borderRadius: "20px", boxShadow: "0 4px 20px rgba(0,0,0,0.06)" }}>
            <div style={{ display: "flex", justifyContent: "space-between", marginBottom: "24px" }}>
                <h2>Quản lý Voucher Cửa Hàng</h2>
                <button
                    onClick={() => openModal()}
                    style={{ padding: "12px 24px", background: "#86542B", color: "white", border: "none", borderRadius: "10px", fontWeight: "600" }}
                >
                    + Tạo voucher mới
                </button>
            </div>

            {/* Bảng danh sách */}
            {/* Bảng danh sách */}
            <div style={{ overflowX: "auto" }}>
                <table style={{ width: "100%", borderCollapse: "collapse", background: "white" }}>
                    <thead>
                        <tr style={{ background: "#f8f9fa", textAlign: "left" }}>
                            <th style={{ padding: "14px 12px", borderBottom: "2px solid #ddd" }}>Mã Voucher</th>
                            <th style={{ padding: "14px 12px", borderBottom: "2px solid #ddd" }}>Tên</th>
                            <th style={{ padding: "14px 12px", borderBottom: "2px solid #ddd" }}>Giảm giá</th>
                            <th style={{ padding: "14px 12px", borderBottom: "2px solid #ddd" }}>Đơn tối thiểu</th>
                            <th style={{ padding: "14px 12px", borderBottom: "2px solid #ddd" }}>Ngày bắt đầu</th>
                            <th style={{ padding: "14px 12px", borderBottom: "2px solid #ddd" }}>Ngày kết thúc</th>
                            <th style={{ padding: "14px 12px", borderBottom: "2px solid #ddd", textAlign: "center" }}>Đã dùng</th>
                            <th style={{ padding: "14px 12px", borderBottom: "2px solid #ddd", textAlign: "right" }}>Tổng giảm</th>
                            <th style={{ padding: "14px 12px", borderBottom: "2px solid #ddd" }}>Trạng thái</th>
                            <th style={{ padding: "14px 12px", borderBottom: "2px solid #ddd", textAlign: "center" }}>Thao tác</th>
                        </tr>
                    </thead>
                    <tbody>
                        {vouchers.length === 0 ? (
                            <tr>
                                <td colSpan={9} style={{ padding: "40px", textAlign: "center", color: "#666" }}>
                                    Chưa có voucher nào. Hãy tạo voucher đầu tiên!
                                </td>
                            </tr>
                        ) : (
                            vouchers.map(v => (
                                <tr key={v.id} style={{ borderBottom: "1px solid #eee" }}>
                                    <td style={{ padding: "14px 12px", fontWeight: "600", fontFamily: "monospace" }}>{v.code}</td>
                                    <td style={{ padding: "14px 12px" }}>{v.name || "-"}</td>
                                    <td style={{ padding: "14px 12px", fontWeight: "600" }}>
                                        {v.discountType === 1 ? `${v.discountValue}%` : `${v.discountValue.toLocaleString('vi-VN')}đ`}
                                    </td>
                                    <td style={{ padding: "14px 12px" }}>
                                        {v.minOrderValue ? v.minOrderValue.toLocaleString('vi-VN') + "đ" : "Không giới hạn"}
                                    </td>
                                    <td style={{ padding: "14px 12px", fontSize: "13px" }}>
                                        {new Date(v.startDate).toLocaleDateString('vi-VN')}
                                    </td>
                                    <td style={{ padding: "14px 12px", fontSize: "13px" }}>
                                        {v.endDate && `${new Date(v.endDate).toLocaleDateString('vi-VN')}`}
                                    </td>
                                    <td style={{ padding: "14px 12px", textAlign: "center", fontWeight: "600" }}>
                                        {v.usedCount}
                                    </td>
                                    <td style={{ padding: "14px 12px", textAlign: "right", color: "#10b981", fontWeight: "600" }}>
                                        {v.totalDiscountApplied?.toLocaleString('vi-VN')}đ
                                    </td>
                                    <td style={{ padding: "14px 12px" }}>
                                        <span style={{
                                            padding: "6px 14px",
                                            borderRadius: "9999px",
                                            fontSize: "13px",
                                            backgroundColor: v.isActive ? "#10b981" : "#ef4444",
                                            color: "white"
                                        }}>
                                            {v.isActive ? "Hoạt động" : "Tắt"}
                                        </span>
                                    </td>
                                    <td style={{ padding: "14px 12px", textAlign: "center" }}>
                                        <button onClick={() => openModal(v)} style={{ marginRight: "8px", padding: "6px 14px", background: "#3b82f6", color: "white", border: "none", borderRadius: "6px" }}>Sửa</button>
                                        <button onClick={() => deleteVoucher(v.id)} style={{ padding: "6px 14px", background: "#ef4444", color: "white", border: "none", borderRadius: "6px" }}>Xóa</button>
                                    </td>
                                </tr>
                            ))
                        )}
                    </tbody>
                </table>
            </div>

            {/* ==================== MODAL TẠO / SỬA VOUCHER ==================== */}
            {showModal && (
                <div style={{ position: "fixed", inset: 0, background: "rgba(0,0,0,0.75)", display: "flex", alignItems: "center", justifyContent: "center", zIndex: 1200 }}>
                    <div style={{ background: "white", width: "680px", maxHeight: "92vh", borderRadius: "16px", overflow: "hidden", display: "flex", flexDirection: "column" }}>

                        <div style={{ padding: "24px", borderBottom: "1px solid #eee", fontSize: "20px", fontWeight: "700" }}>
                            {editingVoucher ? "Chỉnh sửa Voucher" : "Tạo Voucher Mới"}
                        </div>

                        <div style={{ padding: "24px", overflowY: "auto", flex: 1 }}>
                            <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "20px" }}>
                                <div>
                                    <label>Mã Voucher <span style={{ color: "red" }}>*</span></label>
                                    <input
                                        value={form.code}
                                        onChange={(e) => handleInputChange("code", e.target.value.toUpperCase())}
                                        style={{ width: "100%", padding: "12px", marginTop: "8px", borderRadius: "8px", border: "1px solid #ddd" }}
                                        placeholder="ví dụ: PET10, GROOM20"
                                    />
                                </div>
                                <div>
                                    <label>Tên voucher</label>
                                    <input
                                        value={form.name}
                                        onChange={(e) => handleInputChange("name", e.target.value)}
                                        style={{ width: "100%", padding: "12px", marginTop: "8px", borderRadius: "8px", border: "1px solid #ddd" }}
                                        placeholder="Giảm 10% cho lần đầu"
                                    />
                                </div>
                            </div>

                            <div style={{ marginTop: "20px" }}>
                                <label>Mô tả</label>
                                <textarea
                                    value={form.description}
                                    onChange={(e) => handleInputChange("description", e.target.value)}
                                    rows={3}
                                    style={{ width: "100%", padding: "12px", marginTop: "8px", borderRadius: "8px", border: "1px solid #ddd" }}
                                />
                            </div>

                            {/* Loại giảm giá */}
                            <div style={{ marginTop: "24px", display: "grid", gridTemplateColumns: "1fr 1fr", gap: "20px" }}>
                                <div>
                                    <label>Loại giảm giá</label>
                                    <select
                                        value={form.discountType}
                                        onChange={(e) => handleInputChange("discountType", parseInt(e.target.value))}
                                        style={{ width: "100%", padding: "12px", marginTop: "8px", borderRadius: "8px", border: "1px solid #ddd" }}
                                    >
                                        <option value={1}>Phần trăm (%)</option>
                                        <option value={2}>Số tiền cố định (đ)</option>
                                    </select>
                                </div>
                                <div>
                                    <label>Giá trị giảm <span style={{ color: "red" }}>*</span></label>
                                    <input
                                        type="number"
                                        value={form.discountValue}
                                        onChange={(e) => handleInputChange("discountValue", parseFloat(e.target.value) || 0)}
                                        style={{ width: "100%", padding: "12px", marginTop: "8px", borderRadius: "8px", border: "1px solid #ddd" }}
                                    />
                                </div>
                            </div>

                            {/* Điều kiện áp dụng */}
                            <div style={{ marginTop: "24px" }}>
                                <h4 style={{ marginBottom: "12px" }}>Điều kiện áp dụng</h4>
                                <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "20px" }}>
                                    <div>
                                        <label>Đơn tối thiểu (đ)</label>
                                        <input
                                            type="number"
                                            value={form.minOrderValue}
                                            onChange={(e) => handleInputChange("minOrderValue", e.target.value)}
                                            placeholder="0 = không giới hạn"
                                            style={{ width: "100%", padding: "12px", marginTop: "8px", borderRadius: "8px", border: "1px solid #ddd" }}
                                        />
                                    </div>
                                    <div>
                                        <label>Giới hạn giảm tối đa (đ)</label>
                                        <input
                                            type="number"
                                            value={form.maxDiscountAmount}
                                            onChange={(e) => handleInputChange("maxDiscountAmount", e.target.value)}
                                            placeholder="0 = không giới hạn"
                                            style={{ width: "100%", padding: "12px", marginTop: "8px", borderRadius: "8px", border: "1px solid #ddd" }}
                                        />
                                    </div>
                                </div>
                            </div>

                            {/* Giới hạn sử dụng */}
                            <div style={{ marginTop: "24px", display: "grid", gridTemplateColumns: "1fr 1fr", gap: "20px" }}>
                                <div>
                                    <label>Số lần dùng / user</label>
                                    <input
                                        type="number"
                                        value={form.usageLimitPerUser}
                                        onChange={(e) => handleInputChange("usageLimitPerUser", e.target.value)}
                                        style={{ width: "100%", padding: "12px", marginTop: "8px", borderRadius: "8px", border: "1px solid #ddd" }}
                                    />
                                </div>
                                <div>
                                    <label>Tổng số lần dùng</label>
                                    <input
                                        type="number"
                                        value={form.totalUsageLimit}
                                        onChange={(e) => handleInputChange("totalUsageLimit", e.target.value)}
                                        style={{ width: "100%", padding: "12px", marginTop: "8px", borderRadius: "8px", border: "1px solid #ddd" }}
                                    />
                                </div>
                            </div>

                            {/* Thời gian */}
                            <div style={{ marginTop: "24px", display: "grid", gridTemplateColumns: "1fr 1fr", gap: "20px" }}>
                                <div>
                                    <label>Ngày bắt đầu</label>
                                    <input
                                        type="date"
                                        value={form.startDate}
                                        onChange={(e) => handleInputChange("startDate", e.target.value)}
                                        style={{ width: "100%", padding: "12px", marginTop: "8px", borderRadius: "8px", border: "1px solid #ddd" }}
                                    />
                                </div>
                                <div>
                                    <label>Ngày kết thúc (tùy chọn)</label>
                                    <input
                                        type="date"
                                        value={form.endDate}
                                        onChange={(e) => handleInputChange("endDate", e.target.value)}
                                        style={{ width: "100%", padding: "12px", marginTop: "8px", borderRadius: "8px", border: "1px solid #ddd" }}
                                    />
                                </div>
                            </div>

                            <div style={{ marginTop: "24px" }}>
                                <label style={{ display: "flex", alignItems: "center", gap: "8px" }}>
                                    <input
                                        type="checkbox"
                                        checked={form.isActive}
                                        onChange={(e) => handleInputChange("isActive", e.target.checked)}
                                    />
                                    Voucher đang hoạt động
                                </label>
                            </div>
                        </div>

                        <div style={{ padding: "24px", borderTop: "1px solid #eee", display: "flex", gap: "12px" }}>
                            <button
                                onClick={handleSave}
                                disabled={saving}
                                style={{ flex: 1, padding: "16px", background: "#86542B", color: "white", border: "none", borderRadius: "12px", fontWeight: "700" }}
                            >
                                {saving ? "Đang lưu..." : editingVoucher ? "Cập nhật Voucher" : "Tạo Voucher"}
                            </button>
                            <button
                                onClick={closeModal}
                                style={{ flex: 1, padding: "16px", background: "#666", color: "white", border: "none", borderRadius: "12px" }}
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