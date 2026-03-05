interface TimeSlot {
    id: string;
    startTime: string;
    endTime: string;
}

interface Props {
    slot: TimeSlot;
    onBook: () => void;
    disabled?: boolean;
}

export default function TimeSlotCard({ slot, onBook, disabled }: Props) {
    const format = (t: string) => t.slice(0, 5);

    return (
        <button
            onClick={onBook}
            disabled={disabled}
            style={{
                padding: "14px 0",
                borderRadius: 12,
                border: "1px solid #ddd",
                background: "#fff",
                fontWeight: 600,
                cursor: "pointer"
            }}
        >
            {format(slot.startTime)} - {format(slot.endTime)}
        </button>
    );
}