// src/components/admin/StoreCategoryTab.tsx
import { useState, useEffect } from "react";
import axiosInstance from "../../utils/axiosInstance";

export default function StoreCategoryTab() {
    const [categories, setCategories] = useState<any[]>([]);
    const [loading, setLoading] = useState(true);
    const [showModal, setShowModal] = useState(false);
    const [editing, setEditing] = useState<any>(null);
    const [form, setForm] = useState({ name: "", description: "", isActive: true });

    useEffect(() => {
        loadCategories();
    }, []);

    const loadCategories = async () => {
        setLoading(true);
        try {
            const res = await axiosInstance.get("/admin/masterdata/store-categories");
            setCategories(res.data);
        } catch (err) {
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    const openModal = (cat?: any) => {
        if (cat) {
            setEditing(cat);
            setForm({ name: cat.name, description: cat.description || "", isActive: cat.isActive });
        } else {
            setEditing(null);
            setForm({ name: "", description: "", isActive: true });
        }
        setShowModal(true);
    };

    const handleSave = async () => {
        if (!form.name.trim()) return alert("Vui lòng nhập tên danh mục");

        try {
            if (editing) {
                await axiosInstance.put(`/admin/masterdata/store-categories/${editing.id}`, form);
            } else {
                await axiosInstance.post("/admin/masterdata/store-categories", form);
            }
            alert("Lưu thành công!");
            setShowModal(false);
            loadCategories();
        } catch (err) {
            alert("Lưu thất bại");
        }
    };

    const handleDelete = async (id: string) => {
        if (!window.confirm("Xóa danh mục này?")) return;
        try {
            await axiosInstance.delete(`/admin/masterdata/store-categories/${id}`);
            loadCategories();
        } catch (err) {
            alert("Xóa thất bại");
        }
    };

    if (loading) return <div style={{ padding: "80px", textAlign: "center" }}>Đang tải...</div>;

    return (
        <div style={{ background: "white", padding: "32px", borderRadius: "20px" }}>
            <div style={{ display: "flex", justifyContent: "space-between", marginBottom: "24px" }}>
                <h2>Danh mục cửa hàng</h2>
                <button onClick={() => openModal()} style={{ padding: "10px 20px", background: "#86542B", color: "white", border: "none", borderRadius: "8px" }}>
                    + Thêm danh mục
                </button>
            </div>

            <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(300px, 1fr))", gap: "16px" }}>
                {categories.map(c => (
                    <div key={c.id} style={{ border: "1px solid #ddd", borderRadius: "12px", padding: "20px" }}>
                        <h3>{c.name}</h3>
                        <p style={{ color: "#666", margin: "8px 0" }}>{c.description || "Không có mô tả"}</p>
                        <div style={{ marginTop: "12px" }}>
                            <span style={{ color: c.isActive ? "#10b981" : "#ef4444" }}>
                                {c.isActive ? "Hoạt động" : "Tạm tắt"}
                            </span>
                        </div>
                        <div style={{ marginTop: "16px", display: "flex", gap: "8px" }}>
                            <button onClick={() => openModal(c)} style={{ flex: 1, padding: "8px", background: "#3b82f6", color: "white", border: "none", borderRadius: "8px" }}>Sửa</button>
                            <button onClick={() => handleDelete(c.id)} style={{ flex: 1, padding: "8px", background: "#ef4444", color: "white", border: "none", borderRadius: "8px" }}>Xóa</button>
                        </div>
                    </div>
                ))}
            </div>

            {/* Modal Thêm/Sửa */}
            {showModal && (
                <div style={{ position: "fixed", inset: 0, background: "rgba(0,0,0,0.7)", display: "flex", alignItems: "center", justifyContent: "center", zIndex: 2000 }}>
                    <div style={{ background: "white", width: "500px", borderRadius: "16px", padding: "24px" }}>
                        <h3>{editing ? "Sửa danh mục" : "Thêm danh mục mới"}</h3>
                        <input value={form.name} onChange={e => setForm({ ...form, name: e.target.value })} placeholder="Tên danh mục" style={{ width: "100%", padding: "12px", margin: "12px 0", borderRadius: "8px", border: "1px solid #ddd" }} />
                        <textarea value={form.description} onChange={e => setForm({ ...form, description: e.target.value })} placeholder="Mô tả" rows={3} style={{ width: "100%", padding: "12px", borderRadius: "8px", border: "1px solid #ddd" }} />
                        <label><input type="checkbox" checked={form.isActive} onChange={e => setForm({ ...form, isActive: e.target.checked })} /> Hoạt động</label>

                        <div style={{ marginTop: "20px", display: "flex", gap: "12px" }}>
                            <button onClick={handleSave} style={{ flex: 1, padding: "12px", background: "#86542B", color: "white", border: "none", borderRadius: "8px" }}>Lưu</button>
                            <button onClick={() => setShowModal(false)} style={{ flex: 1, padding: "12px", background: "#666", color: "white", border: "none", borderRadius: "8px" }}>Hủy</button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}