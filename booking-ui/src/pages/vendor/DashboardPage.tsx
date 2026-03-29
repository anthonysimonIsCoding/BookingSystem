// src/pages/vendor/DashboardPage.tsx
import React, { useEffect, useState } from 'react';
import axiosInstance from '../../utils/axiosInstance';
import StatCard from '../../components/vendor/StatCard';
import TopServiceTable from '../../components/vendor/TopServiceTable';
import ReviewCard from '../../components/vendor/ReviewCard';
import VendorSidebar from '../../components/vendor/VendorSidebar';

interface VendorDashboardData {
    totalBookings: number;
    pendingBookings: number;
    totalRevenue: number;
    averageRating: number;
    topServices: Array<{
        rank: number;
        serviceName: string;
        price: number;
        bookingCount: number;
    }>;
    recentReviews: Array<{
        rating: number;
        comment?: string;
        customerName: string;
        petName?: string;
        createdAt: string;
    }>;
}

export default function DashboardPage() {
    const [dashboard, setDashboard] = useState<VendorDashboardData | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchDashboard = async () => {
            try {
                setLoading(true);
                const res = await axiosInstance.get('/vendor/dashboard');
                setDashboard(res.data);
            } catch (err: any) {
                console.error(err);
                setError("Không thể tải dữ liệu dashboard");
            } finally {
                setLoading(false);
            }
        };

        fetchDashboard();
    }, []);

    if (loading) return <div style={{ minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center', background: '#f8f5f0' }}>Đang tải...</div>;
    if (error) return <div style={{ color: 'red', padding: '40px' }}>{error}</div>;
    if (!dashboard) return <div>Không có dữ liệu</div>;

    return (
        <div style={{ display: "flex", minHeight: "100vh" }}>
            <VendorSidebar />
            <div style={{
                marginLeft: "260px",
                flex: 1,
                padding: "30px 40px",
                background: "#f8f5f0",
                minHeight: "100vh"
            }}>
                <div style={{ maxWidth: "1400px", margin: "0 auto" }}>
                <h1 style={{ fontSize: '36px', fontWeight: '700', color: '#333', marginBottom: '8px' }}>
                    Dashboard Cửa Hàng
                </h1>
                <p style={{ color: '#666', fontSize: '18px', marginBottom: '40px' }}>
                    Tổng quan hoạt động kinh doanh hôm nay
                </p>

                {/* Stats */}
                <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))', gap: '24px', marginBottom: '50px' }}>
                    <StatCard title="Tổng số đơn hàng" value={dashboard.totalBookings} />
                    <StatCard title="Đơn chưa hoàn thành" value={dashboard.pendingBookings} />
                    <StatCard title="Tổng doanh thu" value={dashboard.totalRevenue.toLocaleString('vi-VN')} suffix="đ" />
                    <StatCard title="Đánh giá trung bình" value={dashboard.averageRating.toFixed(1)} suffix="⭐" />
                </div>

                {/* Top Services */}
                <div style={{ marginBottom: '50px' }}>
                    <h2 style={{ fontSize: '24px', marginBottom: '20px', color: '#333' }}>
                        🏆 Top Dịch Vụ Được Đặt Nhiều Nhất
                    </h2>
                    <TopServiceTable services={dashboard.topServices} />
                </div>

                {/* Reviews */}
                <div>
                    <h2 style={{ fontSize: '24px', marginBottom: '20px', color: '#333' }}>
                        ⭐ Đánh Giá Gần Đây Từ Khách Hàng
                    </h2>
                    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(360px, 1fr))', gap: '24px' }}>
                        {dashboard.recentReviews.map((review, i) => (
                            <ReviewCard key={i} review={review} />
                        ))}
                    </div>
                </div>
            </div>
            </div>
        </div>
    );
}