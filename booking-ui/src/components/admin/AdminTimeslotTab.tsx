// src/components/admin/AdminTimeslotTab.tsx
import { useState, useEffect } from "react";
import axiosInstance from "../../utils/axiosInstance";

export default function AdminTimeslotTab({ storeId }: { storeId: string }) {
    const [timeSlots, setTimeSlots] = useState<any[]>([]);
    const [overrides, setOverrides] = useState<any[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        loadData();
    }, [storeId]);

    const loadData = async () => {
        try {
            const [slotsRes, overridesRes] = await Promise.all([
                axiosInstance.get(`/admin/stores/${storeId}/timeslots`),
                axiosInstance.get(`/admin/stores/${storeId}/timeslot-overrides`)
            ]);
            setTimeSlots(slotsRes.data || []);
            setOverrides(overridesRes.data || []);
        } catch (err) {
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    if (loading) return <div style={{ padding: "80px", textAlign: "center" }}>Đang tải...</div>;

    return (
        <div style={{ background: "white", padding: "32px", borderRadius: "20px", boxShadow: "0 4px 20px rgba(0,0,0,0.06)" }}>
            <h2>Timeslot & Lịch ngoại lệ</h2>
            <p style={{ color: "#666" }}>Admin chỉ được xem.</p>

            <h3>Khung giờ cố định</h3>
            {timeSlots.map(slot => (
                <div key={slot.id} style={{ padding: "12px", border: "1px solid #ddd", borderRadius: "8px", marginBottom: "8px", display: "flex", justifyContent: "space-between" }}>
                    <span>{slot.startTime} - {slot.endTime}</span>
                    <span>Sức chứa: {slot.capacity} | {slot.isActive ? "Hoạt động" : "Tắt"}</span>
                </div>
            ))}

            <h3 style={{ marginTop: "30px" }}>Khung giờ ngoại lệ</h3>
            {overrides.map(ov => (
                <div key={ov.id} style={{ padding: "14px", border: "1px solid #ddd", borderRadius: "10px", marginBottom: "12px" }}>
                    <strong>{ov.date}</strong> — {ov.isFullDayClosure ? "Nghỉ cả ngày" : `${ov.startTime || "--"} - ${ov.endTime || "--"}`}
                    <br />
                    <small>Lý do: {ov.reason || "Không có"}</small>
                </div>
            ))}
        </div>
    );
}