import { useState, useEffect } from "react";
import axiosInstance from '../../utils/axiosInstance';

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

export default function ServiceTab() {
    const [services, setServices] = useState<Service[]>([]);
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);

    // Modal
    const [showModal, setShowModal] = useState(false);
    const [editingService, setEditingService] = useState<Service | null>(null);

    // Form state
    const [form, setForm] = useState<Service>({
        id: "",
        name: "",
        description: "",
        price: 100000,
        durationMinutes: 30,
        type: 0,
        isActive: true,
        optionGroups: []
    });

    useEffect(() => {
        loadServices();
    }, []);

    const loadServices = async () => {
        setLoading(true);
        try {
            const res = await axiosInstance.get('/vendor/store/services');
            setServices(res.data || []);
        } catch (err: any) {
            console.error(err);
            alert("Lỗi tải danh sách dịch vụ");
        } finally {
            setLoading(false);
        }
    };

    const openModal = (service?: Service) => {
        if (service) {
            setEditingService(service);
            setForm(JSON.parse(JSON.stringify(service))); // deep copy
        } else {
            setEditingService(null);
            setForm({
                id: "",
                name: "",
                description: "",
                price: 100000,
                durationMinutes: 30,
                type: 0,
                isActive: true,
                optionGroups: []
            });
        }
        setShowModal(true);
    };

    const closeModal = () => {
        setShowModal(false);
        setEditingService(null);
    };

    // ==================== THAO TÁC NHÓM & OPTION (ĐÃ SỬA) ====================
    const addGroup = () => {
        setForm(prev => ({
            ...prev,
            optionGroups: [
                ...prev.optionGroups,
                {
                    id: "",
                    name: "Nhóm tùy chọn mới",
                    type: 0,
                    isRequired: true,
                    options: []
                }
            ]
        }));
    };

    const removeGroup = (groupIndex: number) => {
        if (!window.confirm("Xóa nhóm này và tất cả option bên trong?")) return;
        setForm(prev => ({
            ...prev,
            optionGroups: prev.optionGroups.filter((_, i) => i !== groupIndex)
        }));
    };

    const addOption = (groupIndex: number) => {
        setForm(prev => {
            const newOptionGroups = [...prev.optionGroups];

            // Tạo bản sao của mảng options
            const currentOptions = [...newOptionGroups[groupIndex].options];

            currentOptions.push({
                id: "",
                name: "Tùy chọn mới",
                price: 50000,
                durationMinutes: 15,
                isActive: true
            });

            // Gán lại group với options mới
            newOptionGroups[groupIndex] = {
                ...newOptionGroups[groupIndex],
                options: currentOptions
            };

            return {
                ...prev,
                optionGroups: newOptionGroups
            };
        });
    };

    const removeOption = (groupIndex: number, optIndex: number) => {
        if (!window.confirm("Xóa tùy chọn này?")) return;

        setForm(prev => {
            const newOptionGroups = [...prev.optionGroups];
            const currentOptions = [...newOptionGroups[groupIndex].options];

            currentOptions.splice(optIndex, 1);

            newOptionGroups[groupIndex] = {
                ...newOptionGroups[groupIndex],
                options: currentOptions
            };

            return {
                ...prev,
                optionGroups: newOptionGroups
            };
        });
    };

    // ==================== XỬ LÝ ONCHANGE CHO INPUT NESTED ====================
    const updateGroupName = (groupIndex: number, value: string) => {
        setForm(prev => {
            const newGroups = [...prev.optionGroups];
            newGroups[groupIndex] = { ...newGroups[groupIndex], name: value };
            return { ...prev, optionGroups: newGroups };
        });
    };

    const updateGroupType = (groupIndex: number, value: number) => {
        setForm(prev => {
            const newGroups = [...prev.optionGroups];
            newGroups[groupIndex] = { ...newGroups[groupIndex], type: value };
            return { ...prev, optionGroups: newGroups };
        });
    };

    const updateGroupRequired = (groupIndex: number, checked: boolean) => {
        setForm(prev => {
            const newGroups = [...prev.optionGroups];
            newGroups[groupIndex] = { ...newGroups[groupIndex], isRequired: checked };
            return { ...prev, optionGroups: newGroups };
        });
    };

    const updateOption = (groupIndex: number, optIndex: number, field: keyof Option, value: any) => {
        setForm(prev => {
            const newGroups = [...prev.optionGroups];
            const newOptions = [...newGroups[groupIndex].options];

            newOptions[optIndex] = {
                ...newOptions[optIndex],
                [field]: value
            };

            newGroups[groupIndex] = {
                ...newGroups[groupIndex],
                options: newOptions
            };

            return { ...prev, optionGroups: newGroups };
        });
    };

    const handleSave = async () => {
        if (!form.name.trim()) {
            alert("Vui lòng nhập tên dịch vụ");
            return;
        }

        setSaving(true);
        try {
            if (editingService) {
                await axiosInstance.put(`/vendor/store/services/${editingService.id}`, form);
                alert("✅ Cập nhật thành công!");
            } else {
                await axiosInstance.post('/vendor/store/services', form);
                alert("✅ Tạo dịch vụ mới thành công!");
            }
            closeModal();
            loadServices();
        } catch (err: any) {
            alert(err.response?.data?.message || "Lưu thất bại");
        } finally {
            setSaving(false);
        }
    };

    const deleteService = async (id: string) => {
        if (!window.confirm("Xóa dịch vụ này và tất cả nhóm/option bên trong?")) return;
        try {
            await axiosInstance.delete(`/vendor/store/services/${id}`);
            alert("Xóa thành công!");
            loadServices();
        } catch (err) {
            alert("Xóa thất bại");
        }
    };

    if (loading) return <div style={{ padding: "80px", textAlign: "center" }}>Đang tải dịch vụ...</div>;

    return (
        <div style={{ background: "white", padding: "32px", borderRadius: "20px", boxShadow: "0 4px 20px rgba(0,0,0,0.06)" }}>
            <div style={{ display: "flex", justifyContent: "space-between", marginBottom: "24px" }}>
                <h2>Quản lý Dịch vụ</h2>
                <button
                    onClick={() => openModal()}
                    style={{ padding: "12px 24px", background: "#86542B", color: "white", border: "none", borderRadius: "10px", fontWeight: "600" }}
                >
                    + Thêm dịch vụ mới
                </button>
            </div>

            <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(320px, 1fr))", gap: "20px" }}>
                {services.map(s => (
                    <div key={s.id} style={{ border: "1px solid #eee", borderRadius: "12px", padding: "20px", background: "#fff" }}>
                        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start" }}>
                            <div>
                                <h3 style={{ margin: 0 }}>{s.name}</h3>
                                <p style={{ color: "#666", fontSize: "14px" }}>{s.durationMinutes} phút • {s.price.toLocaleString('vi-VN')}đ</p>
                            </div>
                            <div style={{ background: s.isActive ? "#10b981" : "#ef4444", color: "white", padding: "4px 12px", borderRadius: "9999px", fontSize: "12px", fontWeight: "600" }}>
                                {s.isActive ? "Hoạt động" : "Tắt"}
                            </div>
                        </div>
                        <div style={{ marginTop: "16px", fontSize: "13px", color: "#555" }}>
                            {s.optionGroups.length} nhóm • {s.optionGroups.reduce((sum, g) => sum + g.options.length, 0)} option
                        </div>
                        <div style={{ marginTop: "20px", display: "flex", gap: "8px" }}>
                            <button onClick={() => openModal(s)} style={{ flex: 1, padding: "10px", background: "#3b82f6", color: "white", border: "none", borderRadius: "8px" }}>Sửa</button>
                            <button onClick={() => deleteService(s.id)} style={{ flex: 1, padding: "10px", background: "#ef4444", color: "white", border: "none", borderRadius: "8px" }}>Xóa</button>
                        </div>
                    </div>
                ))}
            </div>

            {/* ==================== MODAL ==================== */}
            {showModal && (
                <div style={{ position: "fixed", inset: 0, background: "rgba(0,0,0,0.7)", display: "flex", alignItems: "center", justifyContent: "center", zIndex: 1100 }}>
                    <div style={{ background: "white", width: "920px", maxHeight: "90vh", borderRadius: "16px", overflow: "hidden", display: "flex", flexDirection: "column" }}>

                        <div style={{ padding: "24px", borderBottom: "1px solid #eee", fontSize: "20px", fontWeight: "700" }}>
                            {editingService ? "Chỉnh sửa dịch vụ" : "Thêm dịch vụ mới"}
                        </div>

                        <div style={{ padding: "24px", overflowY: "auto", flex: 1 }}>

                            {/* Thông tin cơ bản dịch vụ */}
                            <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "20px" }}>
                                <div>
                                    <label>Tên dịch vụ <span style={{ color: "red" }}>*</span></label>
                                    <input
                                        value={form.name}
                                        onChange={e => setForm(prev => ({ ...prev, name: e.target.value }))}
                                        style={{ width: "100%", padding: "12px", marginTop: "8px", borderRadius: "8px", border: "1px solid #ddd" }}
                                    />
                                </div>
                                <div>
                                    <label>Giá gốc (đ) <span style={{ color: "red" }}>*</span></label>
                                    <input
                                        type="number"
                                        value={form.price}
                                        onChange={e => setForm(prev => ({ ...prev, price: parseFloat(e.target.value) || 0 }))}
                                        style={{ width: "100%", padding: "12px", marginTop: "8px", borderRadius: "8px", border: "1px solid #ddd" }}
                                    />
                                </div>
                            </div>

                            <div style={{ marginTop: "16px" }}>
                                <label>Mô tả</label>
                                <textarea
                                    value={form.description || ""}
                                    onChange={e => setForm(prev => ({ ...prev, description: e.target.value }))}
                                    rows={2}
                                    style={{ width: "100%", padding: "12px", marginTop: "8px", borderRadius: "8px", border: "1px solid #ddd" }}
                                />
                            </div>

                            <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr 1fr", gap: "20px", marginTop: "16px" }}>
                                <div>
                                    <label>Thời lượng (phút)</label>
                                    <input
                                        type="number"
                                        value={form.durationMinutes}
                                        onChange={e => setForm(prev => ({ ...prev, durationMinutes: parseInt(e.target.value) || 30 }))}
                                        style={{ width: "100%", padding: "12px", borderRadius: "8px", border: "1px solid #ddd" }}
                                    />
                                </div>
                                <div>
                                    <label>Loại dịch vụ</label>
                                    <select
                                        value={form.type}
                                        onChange={e => setForm(prev => ({ ...prev, type: parseInt(e.target.value) }))}
                                        style={{ width: "100%", padding: "12px", borderRadius: "8px", border: "1px solid #ddd" }}
                                    >
                                        <option value={0}>Single (Cơ bản)</option>
                                        <option value={1}>Multiple (Gói)</option>
                                    </select>
                                </div>
                                <div>
                                    <label style={{ display: "block", marginBottom: "8px" }}>
                                        <input
                                            type="checkbox"
                                            checked={form.isActive}
                                            onChange={e => setForm(prev => ({ ...prev, isActive: e.target.checked }))}
                                        />
                                        Hoạt động
                                    </label>
                                </div>
                            </div>

                            {/* ==================== NHÓM TÙY CHỌN ==================== */}
                            <div style={{ marginTop: "40px" }}>
                                <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "16px" }}>
                                    <h3>Nhóm tùy chọn</h3>
                                    <button onClick={addGroup} style={{ padding: "8px 16px", background: "#10b981", color: "white", border: "none", borderRadius: "8px" }}>
                                        + Thêm nhóm
                                    </button>
                                </div>

                                {form.optionGroups.map((group, gIdx) => (
                                    <div key={gIdx} style={{ border: "1px solid #ddd", borderRadius: "12px", padding: "20px", marginBottom: "24px" }}>

                                        {/* Header của nhóm */}
                                        <div style={{ display: "flex", gap: "12px", marginBottom: "16px" }}>
                                            <input
                                                value={group.name}
                                                onChange={(e) => updateGroupName(gIdx, e.target.value)}
                                                placeholder="Tên nhóm tùy chọn"
                                                style={{ flex: 1, padding: "10px", borderRadius: "8px", border: "1px solid #ccc" }}
                                            />
                                            <select
                                                value={group.type}
                                                onChange={(e) => updateGroupType(gIdx, parseInt(e.target.value))}
                                                style={{ padding: "10px", borderRadius: "8px", border: "1px solid #ccc", minWidth: "160px" }}
                                            >
                                                <option value={0}>Single Choice</option>
                                                <option value={1}>Multi Choice</option>
                                            </select>
                                            <label style={{ display: "flex", alignItems: "center", gap: "8px", whiteSpace: "nowrap" }}>
                                                <input
                                                    type="checkbox"
                                                    checked={group.isRequired}
                                                    onChange={(e) => updateGroupRequired(gIdx, e.target.checked)}
                                                />
                                                Bắt buộc
                                            </label>
                                            <button
                                                onClick={() => removeGroup(gIdx)}
                                                style={{ background: "#ef4444", color: "white", border: "none", borderRadius: "8px", padding: "0 14px" }}
                                            >
                                                Xóa nhóm
                                            </button>
                                        </div>

                                        {/* Bảng Options với tiêu đề cột */}
                                        <div style={{ marginTop: "12px" }}>
                                            <div style={{
                                                display: "grid",
                                                gridTemplateColumns: "auto 1fr 140px 120px 50px",
                                                gap: "12px",
                                                padding: "10px 12px",
                                                background: "#f8f9fa",
                                                borderRadius: "8px",
                                                fontWeight: "600",
                                                color: "#555",
                                                marginBottom: "8px"
                                            }}>
                                                <div></div>
                                                <div>Tên tùy chọn</div>
                                                <div>Giá (đ)</div>
                                                <div>Thời gian (phút)</div>
                                                <div></div>
                                            </div>

                                            {group.options.map((opt, oIdx) => (
                                                <div key={oIdx} style={{
                                                    display: "grid",
                                                    gridTemplateColumns: "auto 1fr 140px 120px 50px",
                                                    gap: "12px",
                                                    alignItems: "center",
                                                    marginBottom: "10px"
                                                }}>
                                                    <div style={{ color: "#888", fontSize: "14px", paddingLeft: "4px" }}>{oIdx + 1}.</div>

                                                    <input
                                                        value={opt.name}
                                                        onChange={(e) => updateOption(gIdx, oIdx, "name", e.target.value)}
                                                        placeholder="Nhập tên tùy chọn"
                                                        style={{ padding: "10px", borderRadius: "8px", border: "1px solid #ddd" }}
                                                    />
                                                    <input
                                                        type="number"
                                                        value={opt.price}
                                                        onChange={(e) => updateOption(gIdx, oIdx, "price", parseFloat(e.target.value) || 0)}
                                                        style={{ padding: "10px", borderRadius: "8px", border: "1px solid #ddd" }}
                                                    />
                                                    <input
                                                        type="number"
                                                        value={opt.durationMinutes}
                                                        onChange={(e) => updateOption(gIdx, oIdx, "durationMinutes", parseInt(e.target.value) || 0)}
                                                        style={{ padding: "10px", borderRadius: "8px", border: "1px solid #ddd" }}
                                                    />
                                                    <button
                                                        onClick={() => removeOption(gIdx, oIdx)}
                                                        style={{ background: "#ef4444", color: "white", border: "none", borderRadius: "6px", padding: "8px" }}
                                                    >
                                                        X
                                                    </button>
                                                </div>
                                            ))}

                                            <button
                                                onClick={() => addOption(gIdx)}
                                                style={{
                                                    marginTop: "12px",
                                                    padding: "10px 18px",
                                                    background: "#3b82f6",
                                                    color: "white",
                                                    border: "none",
                                                    borderRadius: "8px",
                                                    fontSize: "14px"
                                                }}
                                            >
                                                + Thêm tùy chọn
                                            </button>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </div>

                        {/* Footer buttons */}
                        <div style={{ padding: "24px", borderTop: "1px solid #eee", display: "flex", gap: "12px" }}>
                            <button
                                onClick={handleSave}
                                disabled={saving}
                                style={{ flex: 1, padding: "16px", background: "#86542B", color: "white", border: "none", borderRadius: "12px", fontWeight: "700" }}
                            >
                                {saving ? "Đang lưu..." : "Lưu dịch vụ"}
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