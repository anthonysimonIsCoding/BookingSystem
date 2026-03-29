import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';

interface Service {
    id: string;
    name: string;
}

const Navbar: React.FC = () => {
    const [isLoggedIn, setIsLoggedIn] = useState(false);
    const [showServicesDropdown, setShowServicesDropdown] = useState(false);
    const [services, setServices] = useState<Service[]>([]);
    const [loadingServices, setLoadingServices] = useState(true);

    const navigate = useNavigate();

    // Kiểm tra đăng nhập (cập nhật mỗi khi token thay đổi)
    useEffect(() => {
        const checkLogin = () => {
            const token = localStorage.getItem("token");
            setIsLoggedIn(!!token);
        };
        checkLogin();

        // Theo dõi thay đổi token (trường hợp logout từ tab khác)
        window.addEventListener('storage', checkLogin);
        return () => window.removeEventListener('storage', checkLogin);
    }, []);

    // CALL API lấy danh sách dịch vụ
    useEffect(() => {
        fetch('http://localhost:5263/api/navbar/store-categories')
            .then(res => res.json())
            .then(data => {
                setServices(data);
                setLoadingServices(false);
            })
            .catch(err => {
                console.error('Lỗi load categories:', err);
                setLoadingServices(false);
            });
    }, []);

    const handleSearch = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        alert('Tìm dịch vụ cho Boss...');
    };

    return (
        <nav style={{ backgroundColor: '#ffffff', boxShadow: '0 2px 8px rgba(0,0,0,0.08)', position: 'sticky', top: 0, zIndex: 1000 }}>

            {/* TOP */}
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', padding: '12px 24px', maxWidth: '1400px', margin: '0 auto' }}>
                {/* LOGO */}
                <Link to="/" style={{ display: 'flex', alignItems: 'center', gap: '12px', textDecoration: 'none' }}>
                    <img
                        src="../../src/assets/logos/logo.png"
                        alt="Logo"
                        style={{ width: '70px', height: '70px', objectFit: 'contain', borderRadius: '12px', boxShadow: '0 2px 6px rgba(0,174,239,0.2)' }}
                    />
                    <div>
                        <div style={{ fontSize: '40px', fontWeight: 'bold', color: '#00AEEF' }}>PETZON</div>
                        <div style={{ fontSize: '16px', color: '#6c757d' }}>MULTIVENDOR</div>
                    </div>
                </Link>

                {/* SEARCH */}
                <form onSubmit={handleSearch} style={{ flex: 1, maxWidth: '520px', margin: '0 32px' }} />

                {/* USER ICON */}
                <div style={{ display: 'flex', alignItems: 'center', gap: '24px' }}>
                    <button
                        style={{ background: 'none', border: 'none', fontSize: '24px', cursor: 'pointer', color: '#00AEEF' }}
                        onClick={() => isLoggedIn ? navigate('/profile') : navigate('/login')}
                    >
                        👤
                    </button>
                </div>
            </div>

            {/* MENU */}
            <div style={{ backgroundColor: '#000000', color: '#ffffff', display: 'flex', alignItems: 'center', padding: '0 24px', height: '48px' }}>
                <div style={{ maxWidth: '1400px', margin: '0 auto', width: '100%', display: 'flex', alignItems: 'center', gap: '32px' }}>

                    {/* LINKS */}
                    <div style={{ display: 'flex', alignItems: 'center', gap: '28px', fontSize: '15px', fontWeight: '500' }}>
                        <Link to="/" style={{ color: '#ffffff' }}>Trang chủ</Link>
                        <Link to="/list" style={{ color: '#FF6B00' }}>Cửa hàng</Link>

                        {/* DROPDOWN DỊCH VỤ */}
                        <div style={{ position: 'relative' }}>
                            <button
                                onClick={() => setShowServicesDropdown(!showServicesDropdown)}
                                style={{ background: 'none', border: 'none', color: '#ffffff', cursor: 'pointer' }}
                            >
                                Dịch vụ ▼
                            </button>

                            {showServicesDropdown && (
                                <div style={{ position: 'absolute', top: '100%', left: 0, backgroundColor: '#ffffff', boxShadow: '0 8px 16px rgba(0,0,0,0.2)', borderRadius: '8px', padding: '8px 0', minWidth: '220px' }}>
                                    {loadingServices ? (
                                        <div style={{ padding: '10px 20px' }}>Đang tải...</div>
                                    ) : services.length === 0 ? (
                                        <div style={{ padding: '10px 20px' }}>Không có dữ liệu</div>
                                    ) : (
                                        services.map((s) => (
                                            <Link
                                                key={s.id}
                                                to={`/list?categoryIds=${s.id}`}
                                                style={{ display: 'block', padding: '10px 20px', color: '#333', textDecoration: 'none' }}
                                                onClick={() => setShowServicesDropdown(false)}
                                            >
                                                {s.name}
                                            </Link>
                                        ))
                                    )}
                                </div>
                            )}
                        </div>

                        <Link to="/nearme" style={{ color: '#ffffff' }}>Gần tôi</Link>
                        <Link to="#" style={{ color: '#FF6B00' }}>Khuyến mãi</Link>
                        <Link to="#" style={{ color: '#ffffff' }}>Trở thành đối tác</Link>
                    </div>

                    {/* AUTH - chỉ hiện khi CHƯA đăng nhập */}
                    <div style={{ marginLeft: 'auto', display: 'flex', gap: '16px' }}>
                        {!isLoggedIn && (
                            <button
                                onClick={() => navigate('/login')}
                                style={{ color: '#ffffff', background: 'none', border: 'none' }}
                            >
                                Đăng nhập
                            </button>
                        )}
                    </div>
                </div>
            </div>
        </nav>
    );
};

export default Navbar;