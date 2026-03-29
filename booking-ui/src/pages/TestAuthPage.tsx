import { useAuth } from '../hook/useAuth';

function TestAuthPage() {
    const {
        isCustomerLoggedIn,
        isVendorLoggedIn,
        logoutCustomer,
        logoutVendor
    } = useAuth();

    return (
        <div style={{ padding: '40px', fontFamily: 'Arial' }}>
            <h1>Test Auth Context</h1>

            <p><strong>Customer Logged In:</strong> {isCustomerLoggedIn ? '✅ Có' : '❌ Không'}</p>
            <p><strong>Vendor Logged In:</strong> {isVendorLoggedIn ? '✅ Có' : '❌ Không'}</p>

            <div style={{ marginTop: '20px' }}>
                <button onClick={logoutCustomer} style={{ marginRight: '10px', padding: '10px' }}>
                    Logout Customer
                </button>
                <button onClick={logoutVendor} style={{ padding: '10px' }}>
                    Logout Vendor
                </button>
            </div>

            <p style={{ marginTop: '30px', color: 'gray' }}>
                Mở console (F12) để xem log nếu có lỗi
            </p>
        </div>
    );
}

export default TestAuthPage;