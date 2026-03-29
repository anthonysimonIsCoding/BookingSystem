import React from "react";
import dayjs from "dayjs";

interface TimeSlot {
    id: string;
    startTime: string;
    endTime: string;
    capacity: number;
    remainingCapacity: number;
    isAvailable: boolean;
    isOverridden?: boolean;
    overrideReason?: string | null;
    isDisabledByOverride?: boolean;
}

interface TimeSlotCardProps {
    slot: TimeSlot;
    selectedDate: string;
    onBook: () => void;
    disabled?: boolean;           // ← Prop mới từ StoreDetailPage
}

const TimeSlotCard: React.FC<TimeSlotCardProps> = ({
    slot,
    selectedDate,
    onBook,
    disabled = false,
}) => {
    // Kiểm tra slot đã qua giờ (dùng client time + serverTime đồng bộ)
    const isPast = selectedDate === dayjs().format("YYYY-MM-DD") &&
        dayjs(`${selectedDate} ${slot.startTime}`).isBefore(dayjs());

    const effectiveDisabled = disabled || isPast || !slot.isAvailable || !!slot.isDisabledByOverride;

    let statusText = "";
    let statusColor = "#8c8c8c";

    if (isPast) {
        statusText = "Đã qua giờ";
        statusColor = "#ff4d4f";
    } else if (!slot.isAvailable) {
        statusText = slot.remainingCapacity <= 0 ? "Hết chỗ" : "Tạm ngưng";
        statusColor = "#ff4d4f";
    } else if (slot.isDisabledByOverride) {
        statusText = "Tạm ngưng";
        statusColor = "#fa8c16";
    }

    return (
        <div
            onClick={() => !effectiveDisabled && onBook()}
            style={{
                border: `1px solid ${effectiveDisabled ? "#d9d9d9" : "#d9d9d9"}`,
                borderRadius: 12,
                padding: "16px 20px",
                backgroundColor: effectiveDisabled ? "#f0f2f5" : "#ffffff",
                textAlign: "center",
                cursor: effectiveDisabled ? "not-allowed" : "pointer",
                transition: "all 0.2s",
                opacity: effectiveDisabled ? 0.75 : 1,
                boxShadow: slot.isOverridden ? "0 2px 8px rgba(250, 173, 20, 0.15)" : "none",
            }}
        >
            <h3 style={{ margin: "0 0 8px", fontSize: 18, color: effectiveDisabled ? "#8c8c8c" : "#000" }}>
                {slot.startTime} – {slot.endTime}
            </h3>

            <div style={{ marginBottom: 8, fontSize: 15 }}>
                Còn <strong>{slot.remainingCapacity}</strong> / {slot.capacity} chỗ
            </div>

            {effectiveDisabled && (
                <div style={{
                    color: statusColor,
                    fontSize: 14,
                    fontWeight: 600,
                    marginTop: 8,
                }}>
                    {statusText}
                </div>
            )}

            {slot.overrideReason && (
                <div style={{
                    marginTop: 8,
                    fontSize: 13,
                    color: "#fa8c16",
                    fontStyle: "italic",
                    background: "rgba(250, 173, 20, 0.08)",
                    padding: "4px 8px",
                    borderRadius: 6,
                }}>
                    Lý do: {slot.overrideReason}
                </div>
            )}
        </div>
    );
};

export default TimeSlotCard;