// src/components/admin/AdminVoucherTab.tsx
import { useState, useEffect } from "react";
import axiosInstance from "../../utils/axiosInstance";

export default function AdminVoucherTab({ storeId }: { storeId: string }) {
    const [vouchers, setVouchers] = useState<any[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        axiosInstance.get(`/admin/stores/${storeId}/vouchers`)
            .then(res => setVouchers(res.data))
            .catch(err => console.error(err))
            .finally(() => setLoading(false));
    }, [storeId]);

    if (loading) return <div style={{ padding: "80px", textAlign: "center" }}>Đang tải voucher...</div>;

    return (
        <div style={{ background: "white", padding: "32px", borderRadius: "20px" }}>
            <h2>Voucher của cửa hàng</h2>
            {vouchers.map(v => (
                <div key={v.id} style={{ padding: "16px", border: "1px solid #ddd", borderRadius: "10px", marginBottom: "12px" }}>
                    <strong>{v.code}</strong> - {v.name} ({v.discountType === 1 ? `${v.discountValue}%` : `${v.discountValue}đ`})
                    <br />
                    <small>Trạng thái: {v.isActive ? "Hoạt động" : "Tắt"}</small>
                </div>
            ))}
        </div>
    );
}