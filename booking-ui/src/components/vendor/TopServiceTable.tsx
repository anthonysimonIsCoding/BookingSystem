// src/components/vendor/TopServiceTable.tsx
import React from 'react';

interface TopService {
    rank: number;
    serviceName: string;
    price: number;
    bookingCount: number;
}

interface TopServiceTableProps {
    services: TopService[];
}

const TopServiceTable: React.FC<TopServiceTableProps> = ({ services }) => {
    return (
        <div style={{
            background: 'white',
            borderRadius: '20px',
            boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
            overflow: 'hidden',
            border: '1px solid #f0f0f0'
        }}>
            <div style={{
                padding: '20px 28px',
                background: '#f9f7f4',
                borderBottom: '1px solid #eee'
            }}>
                <h3 style={{ margin: 0, fontSize: '20px', color: '#333' }}>
                    🏆 Top Dịch Vụ Được Đặt Nhiều Nhất
                </h3>
            </div>

            <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                <thead>
                    <tr style={{ background: '#f9f7f4' }}>
                        <th style={{ textAlign: 'left', padding: '18px 28px', fontWeight: '500', color: '#666' }}>STT</th>
                        <th style={{ textAlign: 'left', padding: '18px 28px', fontWeight: '500', color: '#666' }}>Tên Dịch Vụ</th>
                        <th style={{ textAlign: 'right', padding: '18px 28px', fontWeight: '500', color: '#666' }}>Giá</th>
                        <th style={{ textAlign: 'right', padding: '18px 28px', fontWeight: '500', color: '#666' }}>Số Lần Đặt</th>
                    </tr>
                </thead>
                <tbody>
                    {services.length > 0 ? services.map((service, index) => (
                        <tr key={index} style={{ borderBottom: '1px solid #f0f0f0' }}>
                            <td style={{ padding: '18px 28px', fontWeight: '600', color: '#86542B' }}>
                                #{service.rank}
                            </td>
                            <td style={{ padding: '18px 28px', fontWeight: '500' }}>
                                {service.serviceName}
                            </td>
                            <td style={{ padding: '18px 28px', textAlign: 'right', fontWeight: '600', color: '#e67e22' }}>
                                {service.price.toLocaleString()}đ
                            </td>
                            <td style={{ padding: '18px 28px', textAlign: 'right', fontWeight: '600', color: '#86542B' }}>
                                {service.bookingCount} đơn
                            </td>
                        </tr>
                    )) : (
                        <tr>
                            <td colSpan={4} style={{ padding: '60px 20px', textAlign: 'center', color: '#999' }}>
                                Chưa có dữ liệu dịch vụ
                            </td>
                        </tr>
                    )}
                </tbody>
            </table>
        </div>
    );
};

export default TopServiceTable;