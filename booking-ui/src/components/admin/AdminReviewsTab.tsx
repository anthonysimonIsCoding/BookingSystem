// src/components/admin/AdminReviewsTab.tsx
import { useState, useEffect } from "react";
import axiosInstance from "../../utils/axiosInstance";

export default function AdminReviewsTab({ storeId }: { storeId: string }) {
    const [reviews, setReviews] = useState<any[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        loadReviews();
    }, [storeId]);

    const loadReviews = async () => {
        setLoading(true);
        try {
            const res = await axiosInstance.get(`/admin/stores/${storeId}/reviews`);
            setReviews(res.data || []);
        } catch (err) {
            console.error(err);
            alert("Không thể tải đánh giá");
        } finally {
            setLoading(false);
        }
    };

    const formatDate = (dateString: string) => {
        if (!dateString) return "Không rõ";
        const date = new Date(dateString);
        return date.toLocaleDateString('vi-VN', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    };

    if (loading) return <div style={{ padding: "80px", textAlign: "center" }}>Đang tải đánh giá...</div>;

    return (
        <div style={{ background: "white", padding: "32px", borderRadius: "20px", boxShadow: "0 4px 20px rgba(0,0,0,0.06)" }}>
            <h2>Đánh giá từ khách hàng</h2>
            <p style={{ color: "#666", marginBottom: "24px" }}>Tổng số đánh giá: {reviews.length}</p>

            <div style={{ display: "flex", flexDirection: "column", gap: "16px" }}>
                {reviews.map(r => (
                    <div key={r.id} style={{
                        border: "1px solid #eee",
                        borderRadius: "16px",
                        padding: "24px",
                        background: "#fff"
                    }}>
                        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start" }}>
                            <div>
                                <div style={{ fontSize: "18px", fontWeight: "600" }}>
                                    {r.customer?.fullName || "Khách hàng ẩn danh"}
                                </div>
                                <div style={{ color: "#666", fontSize: "14px", marginTop: "4px" }}>
                                    {formatDate(r.createdAt)}
                                </div>
                            </div>

                            <div style={{ fontSize: "28px", fontWeight: "700", color: "#facc15" }}>
                                {r.rating} ⭐
                            </div>
                        </div>

                        {r.comment && (
                            <div style={{
                                marginTop: "16px",
                                padding: "16px",
                                background: "#f9f9f9",
                                borderRadius: "12px",
                                lineHeight: "1.6"
                            }}>
                                "{r.comment}"
                            </div>
                        )}
                    </div>
                ))}
            </div>

            {reviews.length === 0 && (
                <div style={{ textAlign: "center", padding: "80px", color: "#888" }}>
                    Chưa có đánh giá nào cho cửa hàng này.
                </div>
            )}
        </div>
    );
}