import { useParams } from "react-router-dom";
import { useEffect, useState } from "react";
import axios from "axios";
import TimeSlotCard from "../components/TimeSlotCard";

interface TimeSlot {
    id: string;
    startTime: string;
    endTime: string;
}

export default function StoreDetailPage() {
    const { id } = useParams();
    const [slots, setSlots] = useState<TimeSlot[]>([]);
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        const loadSlots = async () => {
            try {
                setLoading(true);

                const token = localStorage.getItem("token");

                const res = await axios.get(
                    `http://localhost:5263/api/timeslots/store/${id}`,
                    {
                        headers: { Authorization: `Bearer ${token}` }
                    }
                );

                setSlots(res.data);
            } catch (err) {
                console.error(err);
                alert("API die hoặc token sai đó bro 💀");
            } finally {
                setLoading(false);
            }
        };

        if (id) loadSlots();
    }, [id]);

    const handleBook = async (timeSlotId: string) => {
        try {
            setLoading(true);

            const token = localStorage.getItem("token");

            await axios.post(
                "http://localhost:5263/api/bookings",
                {
                    storeId: id,
                    timeSlotId,
                    bookingDate: new Date().toISOString().split("T")[0]
                },
                {
                    headers: { Authorization: `Bearer ${token}` }
                }
            );

            alert("Booked thành công 🔥");
        } finally {
            setLoading(false);
        }
    };

    return (
        <div
            style={{
                background: "#f6f7fb",
                minHeight: "100vh",
                padding: 32
            }}
        >
            <div style={{ maxWidth: 1100, margin: "0 auto" }}>

                {/* ===== Gallery ===== */}
                <div
                    style={{
                        display: "grid",
                        gridTemplateColumns: "2fr 1fr",
                        gap: 12,
                        marginBottom: 24
                    }}
                >
                    <img
                        src="https://picsum.photos/900/500"
                        style={{ width: "100%", borderRadius: 16 }}
                    />

                    <div
                        style={{
                            display: "grid",
                            gap: 12
                        }}
                    >
                        <img src="https://picsum.photos/400/240" style={{ borderRadius: 16 }} />
                        <img src="https://picsum.photos/401/240" style={{ borderRadius: 16 }} />
                    </div>
                </div>

                {/* ===== Info ===== */}
                <h1 style={{ fontSize: 28, fontWeight: 700 }}>
                    🐶 Pet Care Center
                </h1>

                <p style={{ color: "#666", marginBottom: 30 }}>
                    📍 123 Nguyễn Trãi, Q1 • ⭐ 4.9 (200+ lượt đặt)
                </p>

                {/* ===== Time slots ===== */}
                <h2 style={{ marginBottom: 16 }}>
                    Chọn khung giờ
                </h2>

                {slots.length === 0 && <p>Hết slot rồi 😢</p>}

                <div
                    style={{
                        display: "grid",
                        gridTemplateColumns:
                            "repeat(auto-fit, minmax(120px, 1fr))",
                        gap: 12
                    }}
                >
                    {slots.map(slot => (
                        <TimeSlotCard
                            key={slot.id}
                            slot={slot}
                            onBook={() => handleBook(slot.id)}
                            disabled={loading}
                        />
                    ))}
                </div>
            </div>
        </div>
    );
}