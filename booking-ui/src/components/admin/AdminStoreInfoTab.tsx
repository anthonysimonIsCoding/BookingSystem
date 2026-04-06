// src/components/admin/AdminStoreInfoTab.tsx
import { useState, useEffect } from "react";
import axiosInstance from "../../utils/axiosInstance";

interface StoreData {
    id: string;
    name: string;
    address: string;
    latitude?: number;
    longitude?: number;
    averageRating: number;
    reviewCount: number;
    images: any[];
    categories: { categoryId: string; name: string }[];
    species: { speciesId: string; name: string }[];
}

export default function AdminStoreInfoTab({ storeId }: { storeId: string }) {
    const [store, setStore] = useState<StoreData | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchStore = async () => {
            try {
                const res = await axiosInstance.get(`/admin/stores/${storeId}`);
                setStore(res.data);
            } catch (err) {
                console.error(err);
            } finally {
                setLoading(false);
            }
        };
        fetchStore();
    }, [storeId]);

    if (loading) return <div style={{ padding: "80px", textAlign: "center" }}>Đang tải thông tin cửa hàng...</div>;
    if (!store) return <div>Không tìm thấy thông tin cửa hàng</div>;

    const thumbnail = store.images?.find((img: any) => img.isThumbnail) || store.images?.[0];

    return (
        <div style={{ background: "white", borderRadius: "20px", overflow: "hidden", boxShadow: "0 8px 30px rgba(0,0,0,0.08)" }}>
            {thumbnail && (
                <div style={{ height: "280px", position: "relative" }}>
                    <img src={thumbnail.imageUrl} alt={store.name} style={{ width: "100%", height: "100%", objectFit: "cover" }} />
                    <div style={{ position: "absolute", inset: 0, background: "linear-gradient(to top, rgba(0,0,0,0.65), rgba(0,0,0,0.2))" }} />
                    <div style={{ position: "absolute", bottom: "24px", left: "32px", color: "white" }}>
                        <h1 style={{ fontSize: "32px", fontWeight: "700", margin: "0 0 8px 0" }}>{store.name}</h1>
                        <div style={{ display: "flex", alignItems: "center", gap: "8px", fontSize: "16px" }}>
                            📍 {store.address}
                        </div>
                    </div>
                </div>
            )}

            <div style={{ padding: "32px" }}>
                <div style={{ display: "flex", alignItems: "center", gap: "16px", marginBottom: "28px" }}>
                    <div style={{ fontSize: "42px", fontWeight: "700", color: "#86542B" }}>
                        {store.averageRating.toFixed(1)}
                    </div>
                    <div>
                        <div style={{ fontSize: "26px", color: "#facc15" }}>
                            {"★".repeat(Math.floor(store.averageRating))}
                        </div>
                        <div style={{ color: "#666", fontSize: "15px" }}>
                            Dựa trên {store.reviewCount} đánh giá
                        </div>
                    </div>
                </div>

                {store.categories?.length > 0 && (
                    <div style={{ marginBottom: "28px" }}>
                        <div style={{ fontWeight: "600", color: "#444", marginBottom: "12px" }}>🏷️ Danh mục cửa hàng</div>
                        <div style={{ display: "flex", flexWrap: "wrap", gap: "10px" }}>
                            {store.categories.map((cat: any, idx: number) => (
                                <span key={idx} style={{ background: "#fff4e6", color: "#b45309", padding: "10px 18px", borderRadius: "30px", fontSize: "15px", fontWeight: "500" }}>
                                    {cat.name}
                                </span>
                            ))}
                        </div>
                    </div>
                )}

                {store.species?.length > 0 && (
                    <div style={{ marginBottom: "32px" }}>
                        <div style={{ fontWeight: "600", color: "#444", marginBottom: "12px" }}>🐾 Chủng loài nhận chăm sóc</div>
                        <div style={{ display: "flex", flexWrap: "wrap", gap: "10px" }}>
                            {store.species.map((sp: any, idx: number) => (
                                <span key={idx} style={{ background: "#e6f4ea", color: "#166534", padding: "10px 18px", borderRadius: "30px", fontSize: "15px", fontWeight: "500" }}>
                                    {sp.name}
                                </span>
                            ))}
                        </div>
                    </div>
                )}

                {store.images && store.images.length > 0 && (
                    <div>
                        <div style={{ fontWeight: "600", color: "#444", marginBottom: "16px" }}>
                            Hình ảnh cửa hàng ({store.images.length})
                        </div>
                        <div style={{ display: "flex", gap: "12px", flexWrap: "wrap" }}>
                            {store.images.sort((a: any, b: any) => a.order - b.order).map((img: any, idx: number) => (
                                <div key={idx} style={{ position: "relative", width: "165px" }}>
                                    <img src={img.imageUrl} style={{ width: "100%", height: "120px", objectFit: "cover", borderRadius: "12px" }} />
                                    {img.isThumbnail && (
                                        <div style={{ position: "absolute", top: "8px", left: "8px", background: "#10b981", color: "white", padding: "4px 10px", borderRadius: "6px", fontSize: "12px", fontWeight: "600" }}>
                                            Ảnh chính
                                        </div>
                                    )}
                                </div>
                            ))}
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
}