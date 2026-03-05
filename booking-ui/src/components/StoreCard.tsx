import { useNavigate } from "react-router-dom";

interface Store {
    id: string;
    name: string;
    address: string;
}

interface Props {
    store: Store;
}

export default function StoreCard({ store }: Props) {
    const navigate = useNavigate();

    return (
        <div
            onClick={() => navigate(`/store/${store.id}`)}
            style={{
                background: "#fff",
                borderRadius: 18,
                overflow: "hidden",
                cursor: "pointer",
                boxShadow: "0 6px 18px rgba(0,0,0,0.08)",
                transition: "0.2s"
            }}
            onMouseEnter={e =>
                (e.currentTarget.style.transform = "translateY(-5px)")
            }
            onMouseLeave={e =>
                (e.currentTarget.style.transform = "none")
            }
        >
            {/* Ảnh lớn phía trên */}
            <div
                style={{
                    height: 180,
                    background:
                        "url(https://picsum.photos/600/400) center/cover"
                }}
            />

            {/* Info */}
            <div style={{ padding: 16 }}>
                <h3
                    style={{
                        fontSize: 17,
                        fontWeight: 600,
                        marginBottom: 6
                    }}
                >
                    {store.name}
                </h3>

                <p
                    style={{
                        fontSize: 13,
                        color: "#777",
                        marginBottom: 8
                    }}
                >
                    📍 {store.address}
                </p>

                {/* fake rating */}
                <div style={{ fontSize: 13, marginBottom: 8 }}>
                    ⭐ 4.8 • 200+ lượt đặt
                </div>

                {/* giá + CTA */}
                <div
                    style={{
                        display: "flex",
                        justifyContent: "space-between",
                        alignItems: "center"
                    }}
                >
                    <span
                        style={{
                            color: "#ff3b30",
                            fontWeight: 700,
                            fontSize: 18
                        }}
                    >
                        Đặt ngay
                    </span>

                    <span
                        style={{
                            background: "#ffe9d6",
                            color: "#ff6b00",
                            fontSize: 12,
                            padding: "4px 10px",
                            borderRadius: 999
                        }}
                    >
                        Còn chỗ
                    </span>
                </div>
            </div>
        </div>
    );
}