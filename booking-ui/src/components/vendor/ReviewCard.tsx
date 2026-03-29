// src/components/vendor/ReviewCard.tsx
import React from 'react';

interface Review {
    rating: number;
    comment?: string;
    customerName: string;
    petName?: string;
    createdAt: string;
}

interface ReviewCardProps {
    review: Review;
}

const ReviewCard: React.FC<ReviewCardProps> = ({ review }) => {
    const stars = Array.from({ length: 5 }, (_, i) => (
        <span key={i} style={{ color: i < Math.floor(review.rating) ? '#f1c40f' : '#ddd', fontSize: '22px' }}>
            ★
        </span>
    ));

    return (
        <div style={{
            background: 'white',
            padding: '24px',
            borderRadius: '20px',
            boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
            border: '1px solid #f0f0f0'
        }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px' }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
                    <div style={{ fontSize: '26px' }}>{stars}</div>
                    <span style={{ fontSize: '20px', fontWeight: '600' }}>{review.rating}</span>
                </div>
                <span style={{ fontSize: '13px', color: '#999' }}>
                    {new Date(review.createdAt).toLocaleDateString('vi-VN')}
                </span>
            </div>

            {review.comment && (
                <p style={{
                    fontStyle: 'italic',
                    color: '#555',
                    lineHeight: '1.5',
                    marginBottom: '16px'
                }}>
                    "{review.comment}"
                </p>
            )}

            <div>
                <p style={{ fontWeight: '600', color: '#333', marginBottom: '4px' }}>
                    {review.customerName}
                </p>
                {review.petName && (
                    <p style={{ color: '#777', fontSize: '14px' }}>
                        Thú cưng: {review.petName}
                    </p>
                )}
            </div>
        </div>
    );
};

export default ReviewCard;