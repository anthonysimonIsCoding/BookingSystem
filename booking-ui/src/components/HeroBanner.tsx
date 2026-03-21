// src/components/HeroBanner.tsx

import React, { useState, useEffect } from 'react';

const slides = [
    {
        imageUrl: 'https://images.unsplash.com/photo-1583511655857-d19b40a7a54e?w=1600&q=80', // ảnh chó mèo cute
        alt: 'Boss đang vui vẻ',
    },
    {
        imageUrl: 'https://images.unsplash.com/photo-1587300003388-59208cc962cb?w=1600&q=80', // pet grooming
        alt: 'Tắm tỉa Boss',
    },
    {
        imageUrl: 'https://images.unsplash.com/photo-1517849845537-4d257902454a?w=1600&q=80', // pet hotel vibe
        alt: 'Pet Hotel thoải mái',
    },
    // Thêm ảnh của bạn vào đây, ví dụ từ public folder:
    // { imageUrl: '/images/banner-pet-hotel.jpg', alt: 'Pet Hotel' },
];

const HeroBanner: React.FC = () => {
    const [currentIndex, setCurrentIndex] = useState(0);

    // Tự động chuyển slide mỗi 5 giây
    useEffect(() => {
        const interval = setInterval(() => {
            setCurrentIndex((prev) => (prev + 1) % slides.length);
        }, 5000);
        return () => clearInterval(interval);
    }, []);

    const goToPrev = () => {
        setCurrentIndex((prev) => (prev - 1 + slides.length) % slides.length);
    };

    const goToNext = () => {
        setCurrentIndex((prev) => (prev + 1) % slides.length);
    };

    const goToSlide = (index: number) => {
        setCurrentIndex(index);
    };

    return (
        <div
            style={{
                position: 'relative',
                height: '500px', // chỉnh chiều cao tùy ý
                width: '100%',
                overflow: 'hidden',
            }}
        >
            {/* Các slide ảnh */}
            {slides.map((slide, index) => (
                <div
                    key={index}
                    style={{
                        position: 'absolute',
                        inset: 0,
                        backgroundImage: `url(${slide.imageUrl})`,
                        backgroundSize: 'cover',
                        backgroundPosition: 'center',
                        opacity: index === currentIndex ? 1 : 0,
                        transition: 'opacity 1s ease-in-out',
                        zIndex: 1,
                    }}
                />
            ))}

            {/* Overlay gradient để text nổi bật */}
            <div
                style={{
                    position: 'absolute',
                    inset: 0,
                    background: 'linear-gradient(to right, rgba(0,0,0,0.5), rgba(0,0,0,0.2))',
                    zIndex: 2,
                }}
            />

            {/* Nội dung text overlay */}
            <div
                style={{
                    position: 'absolute',
                    inset: 0,
                    zIndex: 3,
                    display: 'flex',
                    flexDirection: 'column',
                    justifyContent: 'center',
                    padding: '0 40px',
                    maxWidth: '1200px',
                    margin: '0 auto',
                    color: '#ffffff',
                }}
            >
                <h1
                    style={{
                        fontSize: 'clamp(2.5rem, 6vw, 4.5rem)',
                        fontWeight: 'bold',
                        marginBottom: '16px',
                        lineHeight: 1.1,
                    }}
                >
                    Chăm sóc Boss<br />chỉ trong 1 phút
                </h1>

                <p
                    style={{
                        fontSize: 'clamp(1.2rem, 3vw, 1.8rem)',
                        marginBottom: '32px',
                        maxWidth: '600px',
                    }}
                >
                    Hàng trăm đối tác tại Bình Tân & toàn TP.HCM<br />
                    Tắm tỉa • Pet Hotel • Thú y • Đi dạo • Spa...
                </p>

                <button
                    style={{
                        backgroundColor: '#FF6B00',
                        color: '#ffffff',
                        fontWeight: 'bold',
                        fontSize: '1.3rem',
                        padding: '16px 40px',
                        borderRadius: '9999px',
                        border: 'none',
                        cursor: 'pointer',
                        boxShadow: '0 4px 12px rgba(0,0,0,0.3)',
                        transition: 'all 0.3s ease',
                        alignSelf: 'flex-start',
                    }}
                    onMouseEnter={(e) => {
                        e.currentTarget.style.backgroundColor = '#e65c00';
                        e.currentTarget.style.transform = 'scale(1.05)';
                    }}
                    onMouseLeave={(e) => {
                        e.currentTarget.style.backgroundColor = '#FF6B00';
                        e.currentTarget.style.transform = 'scale(1)';
                    }}
                >
                    TÌM DỊCH VỤ NGAY! 🐾
                </button>
            </div>

            {/* Nút prev/next */}
            <button
                onClick={goToPrev}
                style={{
                    position: 'absolute',
                    top: '50%',
                    left: '20px',
                    transform: 'translateY(-50%)',
                    backgroundColor: 'rgba(0,0,0,0.4)',
                    color: '#fff',
                    border: 'none',
                    borderRadius: '50%',
                    width: '50px',
                    height: '50px',
                    fontSize: '24px',
                    cursor: 'pointer',
                    zIndex: 4,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    transition: 'background 0.3s',
                }}
                onMouseEnter={(e) => (e.currentTarget.style.backgroundColor = 'rgba(0,0,0,0.6)')}
                onMouseLeave={(e) => (e.currentTarget.style.backgroundColor = 'rgba(0,0,0,0.4)')}
            >
                ←
            </button>

            <button
                onClick={goToNext}
                style={{
                    position: 'absolute',
                    top: '50%',
                    right: '20px',
                    transform: 'translateY(-50%)',
                    backgroundColor: 'rgba(0,0,0,0.4)',
                    color: '#fff',
                    border: 'none',
                    borderRadius: '50%',
                    width: '50px',
                    height: '50px',
                    fontSize: '24px',
                    cursor: 'pointer',
                    zIndex: 4,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    transition: 'background 0.3s',
                }}
                onMouseEnter={(e) => (e.currentTarget.style.backgroundColor = 'rgba(0,0,0,0.6)')}
                onMouseLeave={(e) => (e.currentTarget.style.backgroundColor = 'rgba(0,0,0,0.4)')}
            >
                →
            </button>

            {/* Dots indicator */}
            <div
                style={{
                    position: 'absolute',
                    bottom: '20px',
                    left: '50%',
                    transform: 'translateX(-50%)',
                    display: 'flex',
                    gap: '12px',
                    zIndex: 4,
                }}
            >
                {slides.map((_, index) => (
                    <button
                        key={index}
                        onClick={() => goToSlide(index)}
                        style={{
                            width: '12px',
                            height: '12px',
                            borderRadius: '50%',
                            backgroundColor: index === currentIndex ? '#FF6B00' : 'rgba(255,255,255,0.5)',
                            border: 'none',
                            cursor: 'pointer',
                            transition: 'all 0.3s',
                        }}
                    />
                ))}
            </div>
        </div>
    );
};

export default HeroBanner;