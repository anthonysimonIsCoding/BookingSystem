// src/components/vendor/StatCard.tsx
import React from 'react';

interface StatCardProps {
    title: string;
    value: string | number;
    suffix?: string;
}

const StatCard: React.FC<StatCardProps> = ({ title, value, suffix = '' }) => {
    return (
        <div style={{
            background: 'white',
            padding: '28px 24px',
            borderRadius: '20px',
            boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
            border: '1px solid #f0f0f0',
            textAlign: 'center'
        }}>
            <p style={{
                color: '#666',
                fontSize: '15px',
                marginBottom: '8px'
            }}>
                {title}
            </p>
            <p style={{
                fontSize: '32px',
                fontWeight: '700',
                color: '#333',
                margin: 0
            }}>
                {value}
                {suffix && <span style={{ fontSize: '24px', marginLeft: '4px' }}>{suffix}</span>}
            </p>
        </div>
    );
};

export default StatCard;