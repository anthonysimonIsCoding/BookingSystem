import React from "react";

interface TimeSlot {
    id: string;
    startTime: string;
    endTime: string;
    capacity: number;
    remainingCapacity: number;
    isAvailable: boolean;
    isOverridden?: boolean;
    overrideReason?: string | null;
    isDisabledByOverride?: boolean;  // field mới từ backend (tùy chọn)
}

interface TimeSlotCardProps {
    slot: TimeSlot;
    selectedDate: string;
    onBook: () => void;
}

const TimeSlotCard: React.FC<TimeSlotCardProps> = ({ slot, onBook }) => {
    const isDisabled = !slot.isAvailable;
    const hasOverride = slot.isOverridden || slot.isDisabledByOverride || false;
    const showReason = hasOverride && slot.overrideReason;

    let backgroundColor = "#ffffff";
    let borderColor = "#d9d9d9";
    let textColor = "#000000";
    let cursorStyle = "pointer";

    if (isDisabled) {
        backgroundColor = "#f0f2f5";      // xám nhạt
        borderColor = "#d9d9d9";
        textColor = "#8c8c8c";
        cursorStyle = "not-allowed";
    } else if (hasOverride) {
        backgroundColor = "#fffbe6";      // vàng nhạt cho override bình thường
        borderColor = "#ffe58f";
    }

    return (
        <div
            onClick={() => !isDisabled && onBook()}
            style={{
                border: `1px solid ${borderColor}`,
                borderRadius: 12,
                padding: "16px 20px",
                backgroundColor,
                textAlign: "center",
                cursor: cursorStyle,
                transition: "all 0.2s",
                boxShadow: hasOverride ? "0 2px 8px rgba(250, 173, 20, 0.15)" : "none",
                opacity: isDisabled ? 0.7 : 1,
            }}
        >
            <h3 style={{ margin: "0 0 8px", fontSize: 18, color: textColor }}>
                {slot.startTime} – {slot.endTime}
            </h3>

            <div style={{ marginBottom: 8, fontSize: 15 }}>
                Còn <strong>{slot.remainingCapacity}</strong> / {slot.capacity} chỗ
            </div>

            {/* Luôn hiển thị trạng thái nếu disable */}
            {isDisabled && (
                <div style={{
                    color: "#ff4d4f",
                    fontSize: 14,
                    fontWeight: 500,
                    marginBottom: showReason ? 4 : 0
                }}>
                    {slot.isDisabledByOverride ? "Tạm ngưng" : "Hết chỗ"}
                </div>
            )}

            {/* Hiển thị lý do nếu có */}
            {showReason && (
                <div
                    style={{
                        marginTop: 8,
                        fontSize: 13,
                        color: isDisabled ? "#ff7875" : "#fa8c16",
                        fontStyle: "italic",
                        background: isDisabled
                            ? "rgba(255, 77, 79, 0.08)"
                            : "rgba(250, 173, 20, 0.08)",
                        padding: "4px 8px",
                        borderRadius: 6,
                    }}
                >
                    Lý do: {slot.overrideReason}
                </div>
            )}

            {/* Fallback nếu override nhưng không có reason */}
            {hasOverride && !showReason && (
                <div
                    style={{
                        marginTop: 8,
                        fontSize: 13,
                        color: "#8c8c8c",
                        fontStyle: "italic",
                    }}
                >
                    {isDisabled ? "Slot bị tạm ngưng" : "Có thay đổi đặc biệt"}
                </div>
            )}
        </div>
    );
};

export default TimeSlotCard;