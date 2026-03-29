// src/components/vendor/StoreInfoTab.tsx
import { useState, useEffect } from "react";
import axiosInstance from '../../utils/axiosInstance';

interface StoreInfo {
    id: string;
    name: string;
    address: string;
    averageRating: number;
    reviewCount: number;
    images: Array<{ id: string; imageUrl: string; isThumbnail: boolean }>;
}

export default function StoreInfoTab() {
    const [storeInfo, setStoreInfo] = useState<StoreInfo | null>(null);
    const [editMode, setEditMode] = useState(false);
    const [formData, setFormData] = useState({ name: "", address: "" });
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);

    useEffect(() => {
        loadStoreInfo();
    }, []);

    const loadStoreInfo = async () => {
        setLoading(true);
        try {
            const res = await axiosInstance.get('/vendor/store');
            setStoreInfo(res.data);
            setFormData({ name: res.data.name, address: res.data.address });
        } catch (err: any) {
            console.error("Lỗi tải thông tin cửa hàng:", err.response?.data || err.message);
        } finally {
            setLoading(false);
        }
    };

    const handleSave = async () => {
        if (!formData.name.trim() || !formData.address.trim()) {
            alert("Vui lòng nhập đầy đủ tên và địa chỉ");
            return;
        }

        setSaving(true);
        try {
            await axiosInstance.put('/vendor/store', formData);
            alert("Cập nhật thông tin cửa hàng thành công!");
            setEditMode(false);
            loadStoreInfo(); // reload lại dữ liệu
        } catch (err: any) {
            alert(err.response?.data?.message || "Cập nhật thất bại");
        } finally {
            setSaving(false);
        }
    };

    if (loading) return <div style={{ padding: "60px", textAlign: "center" }}>Đang tải thông tin cửa hàng...</div>;

    if (!storeInfo) {
        return (
            <div style={{ padding: "60px", textAlign: "center", background: "white", borderRadius: "20px" }}>
                <p>Chưa có thông tin cửa hàng.</p>
                <p style={{ fontSize: "14px", color: "#666" }}>Vui lòng kiểm tra tài khoản ServiceProvider đã liên kết với cửa hàng chưa.</p>
            </div>
        );
    }

    return (
        <div style={{ background: "white", padding: "32px", borderRadius: "20px", boxShadow: "0 4px 20px rgba(0,0,0,0.06)" }}>
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "24px" }}>
                <h2>Thông tin cửa hàng</h2>
                {!editMode && (
                    <button
                        onClick={() => setEditMode(true)}
                        style={{ padding: "10px 24px", background: "#86542B", color: "white", border: "none", borderRadius: "10px", cursor: "pointer" }}
                    >
                        Chỉnh sửa
                    </button>
                )}
            </div>

            {editMode ? (
                <div>
                    <div style={{ marginBottom: "16px" }}>
                        <label style={{ display: "block", marginBottom: "6px", fontWeight: "600" }}>Tên cửa hàng</label>
                        <input
                            type="text"
                            value={formData.name}
                            onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                            style={{ width: "100%", padding: "12px", borderRadius: "8px", border: "1px solid #ddd" }}
                        />
                    </div>

                    <div style={{ marginBottom: "24px" }}>
                        <label style={{ display: "block", marginBottom: "6px", fontWeight: "600" }}>Địa chỉ</label>
                        <input
                            type="text"
                            value={formData.address}
                            onChange={(e) => setFormData({ ...formData, address: e.target.value })}
                            style={{ width: "100%", padding: "12px", borderRadius: "8px", border: "1px solid #ddd" }}
                        />
                    </div>

                    <div style={{ display: "flex", gap: "12px" }}>
                        <button
                            onClick={handleSave}
                            disabled={saving}
                            style={{
                                flex: 1, padding: "14px", background: "#86542B", color: "white",
                                border: "none", borderRadius: "12px", fontWeight: "600",
                                cursor: saving ? "not-allowed" : "pointer"
                            }}
                        >
                            {saving ? "Đang lưu..." : "Cập nhật thông tin"}
                        </button>
                        <button
                            onClick={() => setEditMode(false)}
                            style={{ flex: 1, padding: "14px", background: "#666", color: "white", border: "none", borderRadius: "12px" }}
                        >
                            Hủy
                        </button>
                    </div>
                </div>
            ) : (
                <div>
                    <p><strong>Tên cửa hàng:</strong> {storeInfo.name}</p>
                    <p><strong>Địa chỉ:</strong> {storeInfo.address}</p>
                    <p><strong>Đánh giá trung bình:</strong> {storeInfo.averageRating} ⭐ ({storeInfo.reviewCount} đánh giá)</p>

                    {storeInfo.images && storeInfo.images.length > 0 && (
                        <div style={{ marginTop: "28px" }}>
                            <h3 style={{ marginBottom: "12px" }}>Hình ảnh cửa hàng</h3>
                            <div style={{ display: "flex", gap: "12px", flexWrap: "wrap" }}>
                                {storeInfo.images.map((img: any) => (
                                    <img
                                        key={img.id}
                                        src={img.imageUrl}
                                        style={{ width: "180px", height: "120px", objectFit: "cover", borderRadius: "12px" }}
                                        alt="Store"
                                    />
                                ))}
                            </div>
                        </div>
                    )}
                </div>
            )}
        </div>
    );
}