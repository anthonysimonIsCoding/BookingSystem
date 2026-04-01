// src/components/vendor/TimeslotTab.tsx
import { useState, useEffect } from "react";
import axiosInstance from '../../utils/axiosInstance';

interface TimeSlot {
    id: string;
    startTime: string;
    endTime: string;
    capacity: number;
    isActive: boolean;
}

interface TimeSlotOverride {
    id: string;
    date: string;
    timeSlotId?: string;
    startTime?: string;
    endTime?: string;
    capacity?: number;
    isFullDayClosure: boolean;
    reason?: string;
}

export default function TimeslotTab() {
    const [timeSlots, setTimeSlots] = useState<TimeSlot[]>([]);
    const [overrides, setOverrides] = useState<TimeSlotOverride[]>([]);
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);
    const [hasChanges, setHasChanges] = useState(false);

    // Modal thêm timeslot mới
    const [showAddSlot, setShowAddSlot] = useState(false);
    const [newSlot, setNewSlot] = useState({
        startTime: "08:00",
        endTime: "09:00",
        capacity: 1,
        isActive: true
    });

    // Modal override
    const [showOverrideModal, setShowOverrideModal] = useState(false);
    const [editingOverride, setEditingOverride] = useState<TimeSlotOverride | null>(null);
    const [overrideForm, setOverrideForm] = useState({
        date: "",
        timeSlotId: "",
        startTime: "",
        endTime: "",
        capacity: 1,
        isFullDayClosure: false,
        reason: ""
    });

    const today = new Date().toISOString().split('T')[0];

    const getMaxDate = () => {
        const max = new Date();
        max.setMonth(max.getMonth() + 6);
        return max.toISOString().split('T')[0];
    };

    const maxDate = getMaxDate();

    useEffect(() => {
        loadData();
    }, []);

    const loadData = async () => {
        setLoading(true);
        try {
            const [slotsRes, overridesRes] = await Promise.all([
                axiosInstance.get('/vendor/timeslot'),                    // ← ĐÃ SỬA
                axiosInstance.get(`/vendor/timeslot/overrides?fromDate=${today}&toDate=${maxDate}`)  // ← ĐÃ SỬA
            ]);

            setTimeSlots(slotsRes.data || []);
            setOverrides(overridesRes.data || []);
            setHasChanges(false);
        } catch (err: any) {
            console.error("Lỗi load data:", err.response?.data || err.message);
            message.error("Không thể tải dữ liệu timeslot");
        } finally {
            setLoading(false);
        }
    };

    const formatTime = (time: string): string => {
        if (!time) return "00:00:00";
        if (time.length === 5) return `${time}:00`;
        return time;
    };

    const isValidTimeRange = (start: string, end: string): boolean => {
        if (!start || !end) return false;
        return start < end;
    };

    // ==================== TIMESLOT CỐ ĐỊNH ====================
    const handleSlotChange = (index: number, field: keyof TimeSlot, value: any) => {
        const updated = [...timeSlots];
        (updated[index] as any)[field] = value;
        setTimeSlots(updated);
        setHasChanges(true);
    };

    const addNewTimeSlot = async () => {
        if (!isValidTimeRange(newSlot.startTime, newSlot.endTime)) {
            alert("Giờ kết thúc phải lớn hơn giờ bắt đầu!");
            return;
        }

        const isDuplicate = timeSlots.some(slot =>
            slot.startTime === newSlot.startTime && slot.endTime === newSlot.endTime
        );

        if (isDuplicate) {
            alert("Khung giờ này đã tồn tại!");
            return;
        }

        setSaving(true);
        try {
            const payload = [{
                startTime: formatTime(newSlot.startTime),
                endTime: formatTime(newSlot.endTime),
                capacity: newSlot.capacity,
                isActive: newSlot.isActive
            }];

            await axiosInstance.post('/vendor/timeslot', payload);   // ← ĐÃ SỬA
            alert("Thêm timeslot mới thành công!");
            setShowAddSlot(false);
            loadData();
        } catch (err: any) {
            alert(err.response?.data?.message || "Thêm timeslot thất bại");
        } finally {
            setSaving(false);
        }
    };

    const saveTimeSlots = async () => {
        for (let slot of timeSlots) {
            if (!isValidTimeRange(slot.startTime, slot.endTime)) {
                alert(`Timeslot ${slot.startTime} - ${slot.endTime} không hợp lệ!`);
                return;
            }
        }

        setSaving(true);
        try {
            const payload = timeSlots.map(slot => ({
                id: slot.id,
                startTime: formatTime(slot.startTime),
                endTime: formatTime(slot.endTime),
                capacity: slot.capacity,
                isActive: slot.isActive
            }));

            await axiosInstance.post('/vendor/timeslot', payload);   // ← ĐÃ SỬA
            alert("Lưu thay đổi thành công!");
            setHasChanges(false);
            loadData();
        } catch (err: any) {
            alert(err.response?.data?.message || "Lưu thất bại");
        } finally {
            setSaving(false);
        }
    };

    // ==================== OVERRIDE ====================
    const openOverrideModal = (ov?: TimeSlotOverride) => {
        if (ov) {
            setEditingOverride(ov);
            setOverrideForm({
                date: ov.date,
                timeSlotId: ov.timeSlotId || "",
                startTime: ov.startTime || "",
                endTime: ov.endTime || "",
                capacity: ov.capacity || 1,
                isFullDayClosure: ov.isFullDayClosure,
                reason: ov.reason || ""
            });
        } else {
            setEditingOverride(null);
            setOverrideForm({
                date: today,
                timeSlotId: "",
                startTime: "",
                endTime: "",
                capacity: 1,
                isFullDayClosure: false,
                reason: ""
            });
        }
        setShowOverrideModal(true);
    };

    const saveOverride = async () => {
        if (!overrideForm.date) {
            alert("Vui lòng chọn ngày");
            return;
        }
        if (!overrideForm.isFullDayClosure && !isValidTimeRange(overrideForm.startTime, overrideForm.endTime)) {
            alert("Giờ kết thúc phải lớn hơn giờ bắt đầu!");
            return;
        }

        const payload = {
            date: overrideForm.date,
            timeSlotId: overrideForm.timeSlotId || null,
            startTime: overrideForm.isFullDayClosure ? null : formatTime(overrideForm.startTime),
            endTime: overrideForm.isFullDayClosure ? null : formatTime(overrideForm.endTime),
            capacity: overrideForm.isFullDayClosure ? null : overrideForm.capacity,
            isFullDayClosure: overrideForm.isFullDayClosure,
            reason: overrideForm.reason || null
        };

        try {
            if (editingOverride) {
                await axiosInstance.put(`/vendor/timeslot/overrides/${editingOverride.id}`, payload);  // ← ĐÃ SỬA
                alert("Cập nhật override thành công!");
            } else {
                await axiosInstance.post('/vendor/timeslot/overrides', payload);   // ← ĐÃ SỬA
                alert("Tạo override thành công!");
            }
            setShowOverrideModal(false);
            loadData();
        } catch (err: any) {
            alert(err.response?.data?.message || "Thao tác thất bại");
        }
    };

    const deleteOverride = async (id: string) => {
        if (!window.confirm("Xóa override này?")) return;
        try {
            await axiosInstance.delete(`/vendor/timeslot/overrides/${id}`);   // ← ĐÃ SỬA
            alert("Xóa thành công!");
            loadData();
        } catch (err: any) {
            alert("Xóa thất bại");
        }
    };
    return (
        <div style={{ background: "white", padding: "32px", borderRadius: "20px", boxShadow: "0 4px 20px rgba(0,0,0,0.06)" }}>
            <h2 style={{ marginBottom: "24px" }}>Quản lý Timeslot</h2>
            <p style={{ color: "#666", marginBottom: "24px" }}>
                Đây là khung giờ nhận Pet, thời gian trả pet tùy thuộc vào dịch vụ khách chọn.
            </p>

            {/* Timeslot cố định */}
            <div style={{ marginBottom: "40px" }}>
                <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "16px" }}>
                    <h3>Timeslot cố định</h3>
                    <button
                        onClick={() => setShowAddSlot(true)}
                        style={{ padding: "8px 16px", background: "#3b82f6", color: "white", border: "none", borderRadius: "8px", cursor: "pointer" }}
                    >
                        + Add New Timeslot
                    </button>
                </div>

                <div style={{ border: "1px solid #eee", borderRadius: "12px", overflow: "hidden" }}>
                    {timeSlots.map((slot, idx) => (
                        <div key={idx} style={{ padding: "16px", borderBottom: idx < timeSlots.length - 1 ? "1px solid #eee" : "none", display: "flex", alignItems: "center", gap: "16px" }}>
                            <div style={{ flex: 1, display: "flex", alignItems: "center", gap: "8px" }}>
                                <input
                                    type="time"
                                    step="60"
                                    value={slot.startTime}
                                    onChange={(e) => handleSlotChange(idx, "startTime", e.target.value)}
                                    style={{ padding: "8px", border: "1px solid #ddd", borderRadius: "6px" }}
                                />
                                <span>-</span>
                                <input
                                    type="time"
                                    step="60"
                                    value={slot.endTime}
                                    onChange={(e) => handleSlotChange(idx, "endTime", e.target.value)}
                                    style={{ padding: "8px", border: "1px solid #ddd", borderRadius: "6px" }}
                                />
                            </div>
                            <input
                                type="number"
                                value={slot.capacity}
                                onChange={(e) => handleSlotChange(idx, "capacity", parseInt(e.target.value) || 1)}
                                style={{ width: "80px", padding: "8px", border: "1px solid #ddd", borderRadius: "6px" }}
                            />
                            <div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
                                <div style={{ width: "16px", height: "16px", background: slot.isActive ? "#10b981" : "#ef4444", borderRadius: "50%" }} />
                                <select
                                    value={slot.isActive.toString()}
                                    onChange={(e) => handleSlotChange(idx, "isActive", e.target.value === "true")}
                                    style={{ padding: "8px", border: "1px solid #ddd", borderRadius: "6px" }}
                                >
                                    <option value="true">Bookable</option>
                                    <option value="false">Disabled</option>
                                </select>
                            </div>
                        </div>
                    ))}
                </div>

                {hasChanges && (
                    <button
                        onClick={saveTimeSlots}
                        disabled={saving}
                        style={{ marginTop: "16px", padding: "12px 24px", background: "#86542B", color: "white", border: "none", borderRadius: "10px" }}
                    >
                        {saving ? "Đang lưu..." : "Lưu Timeslot"}
                    </button>
                )}
            </div>

            {/* Khung giờ ngoại lệ */}
            <div>
                <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "16px" }}>
                    <h3>Khung giờ ngoại lệ</h3>
                    <button onClick={() => openOverrideModal()} style={{ padding: "8px 16px", background: "#3b82f6", color: "white", border: "none", borderRadius: "8px", cursor: "pointer" }}>
                        + Thêm khung giờ ngoại lệ
                    </button>
                </div>

                <div style={{ border: "1px solid #eee", borderRadius: "12px", overflow: "hidden" }}>
                    {overrides.length === 0 ? (
                        <div style={{ padding: "20px", textAlign: "center", color: "#666" }}>Chưa có khung giờ ngoại lệ nào</div>
                    ) : (
                        overrides.map((ov, idx) => (
                            <div key={idx} style={{ padding: "16px", borderBottom: idx < overrides.length - 1 ? "1px solid #eee" : "none", display: "flex", alignItems: "center", gap: "16px" }}>
                                <div style={{ flex: 1 }}>
                                    <strong>{ov.date}</strong><br />
                                    {ov.isFullDayClosure ? (
                                        <span style={{ color: "#ef4444", fontWeight: "600" }}>Nghỉ cả ngày</span>
                                    ) : (
                                        <span>{ov.startTime || "--:--"} - {ov.endTime || "--:--"}</span>
                                    )}
                                </div>
                                <div style={{ flex: 2, color: "#555" }}>
                                    {ov.reason || "Không có lý do"}
                                </div>
                                <div style={{ display: "flex", gap: "8px" }}>
                                    <button onClick={() => openOverrideModal(ov)} style={{ padding: "6px 12px", background: "#3b82f6", color: "white", border: "none", borderRadius: "6px" }}>Sửa</button>
                                    <button onClick={() => deleteOverride(ov.id)} style={{ padding: "6px 12px", background: "#ef4444", color: "white", border: "none", borderRadius: "6px" }}>Xóa</button>
                                </div>
                            </div>
                        ))
                    )}
                </div>
            </div>

            {/* Modal thêm timeslot mới */}
            {showAddSlot && (
                <div style={{ position: "fixed", inset: 0, background: "rgba(0,0,0,0.7)", display: "flex", alignItems: "center", justifyContent: "center", zIndex: 1000 }}>
                    <div style={{ background: "white", padding: "24px", borderRadius: "16px", width: "400px" }}>
                        <h3>Thêm Timeslot mới</h3>
                        <div style={{ margin: "16px 0" }}>
                            <label>Giờ bắt đầu (24h)</label>
                            <input type="time" step="60" value={newSlot.startTime} onChange={(e) => setNewSlot(prev => ({ ...prev, startTime: e.target.value }))} style={{ width: "100%", padding: "10px", marginTop: "4px" }} />
                        </div>
                        <div style={{ margin: "16px 0" }}>
                            <label>Giờ kết thúc (24h)</label>
                            <input type="time" step="60" value={newSlot.endTime} onChange={(e) => setNewSlot(prev => ({ ...prev, endTime: e.target.value }))} style={{ width: "100%", padding: "10px", marginTop: "4px" }} />
                        </div>
                        <div style={{ margin: "16px 0" }}>
                            <label>Sức chứa</label>
                            <input type="number" value={newSlot.capacity} onChange={(e) => setNewSlot(prev => ({ ...prev, capacity: parseInt(e.target.value) || 1 }))} style={{ width: "100%", padding: "10px", marginTop: "4px" }} />
                        </div>
                        <div style={{ margin: "16px 0" }}>
                            <label>
                                <input type="checkbox" checked={newSlot.isActive} onChange={(e) => setNewSlot(prev => ({ ...prev, isActive: e.target.checked }))} />
                                Cho phép đặt
                            </label>
                        </div>
                        <div style={{ display: "flex", gap: "12px", marginTop: "20px" }}>
                            <button onClick={addNewTimeSlot} style={{ flex: 1, padding: "12px", background: "#86542B", color: "white", border: "none", borderRadius: "10px" }}>Thêm ngay</button>
                            <button onClick={() => setShowAddSlot(false)} style={{ flex: 1, padding: "12px", background: "#666", color: "white", border: "none", borderRadius: "10px" }}>Hủy</button>
                        </div>
                    </div>
                </div>
            )}

            {/* Modal Override */}
            {showOverrideModal && (
                <div style={{ position: "fixed", inset: 0, background: "rgba(0,0,0,0.7)", display: "flex", alignItems: "center", justifyContent: "center", zIndex: 1000 }}>
                    <div style={{ background: "white", padding: "24px", borderRadius: "16px", width: "460px" }}>
                        <h3 style={{ marginBottom: "20px" }}>Thêm khung giờ ngoại lệ</h3>

                        <input
                            type="date"
                            value={overrideForm.date}
                            min={today}
                            max={maxDate}
                            onChange={(e) => setOverrideForm(prev => ({ ...prev, date: e.target.value }))}
                            style={{ width: "100%", padding: "12px", marginBottom: "16px", borderRadius: "8px", border: "1px solid #ddd" }}
                        />

                        <select
                            value={overrideForm.timeSlotId}
                            onChange={(e) => setOverrideForm(prev => ({ ...prev, timeSlotId: e.target.value }))}
                            disabled={overrideForm.isFullDayClosure}
                            style={{ width: "100%", padding: "12px", marginBottom: "16px", borderRadius: "8px", border: "1px solid #ddd" }}
                        >
                            <option value="">Chọn khung giờ cố định</option>
                            {timeSlots.map(slot => (
                                <option key={slot.id} value={slot.id}>
                                    {slot.startTime} - {slot.endTime}
                                </option>
                            ))}
                        </select>

                        <label style={{ display: "flex", alignItems: "center", gap: "8px", marginBottom: "16px" }}>
                            <input
                                type="checkbox"
                                checked={overrideForm.isFullDayClosure}
                                onChange={(e) => setOverrideForm(prev => ({ ...prev, isFullDayClosure: e.target.checked }))}
                            />
                            Nghỉ cả ngày
                        </label>

                        {!overrideForm.isFullDayClosure && (
                            <>
                                <div style={{ marginBottom: "16px" }}>
                                    <label>Khung giờ mới (24h)</label>
                                    <div style={{ display: "flex", gap: "8px", marginTop: "4px" }}>
                                        <input
                                            type="time"
                                            step="60"
                                            value={overrideForm.startTime}
                                            onChange={(e) => setOverrideForm(prev => ({ ...prev, startTime: e.target.value }))}
                                            style={{ flex: 1, padding: "10px", borderRadius: "8px", border: "1px solid #ddd" }}
                                        />
                                        <span style={{ alignSelf: "center" }}>-</span>
                                        <input
                                            type="time"
                                            step="60"
                                            value={overrideForm.endTime}
                                            onChange={(e) => setOverrideForm(prev => ({ ...prev, endTime: e.target.value }))}
                                            style={{ flex: 1, padding: "10px", borderRadius: "8px", border: "1px solid #ddd" }}
                                        />
                                    </div>
                                </div>

                                <div style={{ marginBottom: "16px" }}>
                                    <label>Capacity mới</label>
                                    <input
                                        type="number"
                                        value={overrideForm.capacity}
                                        onChange={(e) => setOverrideForm(prev => ({ ...prev, capacity: parseInt(e.target.value) || 1 }))}
                                        style={{ width: "100%", padding: "10px", borderRadius: "8px", border: "1px solid #ddd" }}
                                    />
                                </div>
                            </>
                        )}

                        <input
                            type="text"
                            placeholder="Lý do"
                            value={overrideForm.reason}
                            onChange={(e) => setOverrideForm(prev => ({ ...prev, reason: e.target.value }))}
                            style={{ width: "100%", padding: "12px", marginBottom: "20px", borderRadius: "8px", border: "1px solid #ddd" }}
                        />

                        <div style={{ display: "flex", gap: "12px" }}>
                            <button
                                onClick={saveOverride}
                                style={{ flex: 1, padding: "14px", background: "#86542B", color: "white", border: "none", borderRadius: "10px", fontWeight: "600" }}
                            >
                                {editingOverride ? "Cập nhật" : "Tạo mới"}
                            </button>
                            <button
                                onClick={() => {
                                    setShowOverrideModal(false);
                                    setEditingOverride(null);
                                }}
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