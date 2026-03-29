// src/pages/vendor/LoginVendorPage.tsx
import { useState } from "react";
import { useNavigate } from "react-router-dom";

function LoginVendorPage() {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [loading, setLoading] = useState(false);
    const navigate = useNavigate();

    const handleLogin = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);

        try {
            const res = await fetch("http://localhost:5263/api/auth/vendor/login", {   // ← THÊM "vendor/"
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ email, password })
            });

            if (!res.ok) {
                const errorData = await res.text();
                alert(errorData || "Sai email hoặc mật khẩu");
                return;
            }

            const data = await res.json();
            localStorage.setItem("vendorToken", data.token);   // bạn đang lưu "vendorToken" → OK

            navigate("/vendor");

        } catch (err) {
            console.error(err);
            alert("Lỗi kết nối server.");
        } finally {
            setLoading(false);
        }
    };

    return (
        <div style={{
            display: "flex",
            justifyContent: "center",
            alignItems: "center",
            minHeight: "100vh",
            background: "#F7CF9C",
            padding: "20px"
        }}>
            {/* Card chính */}
            <div style={{
                display: "flex",
                width: "100%",
                maxWidth: "1200px",
                borderRadius: "20px",
                overflow: "hidden",
                boxShadow: "0 10px 40px rgba(0, 0, 0, 0.4)"
            }}>
                {/* Bên trái - Form đăng nhập Vendor */}
                <div style={{
                    flex: 1,
                    maxWidth: "30%",
                    background: "white",
                    padding: "50px 40px",
                    minWidth: "400px"
                }}>
                    <h2 style={{
                        color: "#86542B",
                        textAlign: "center",
                        marginBottom: "30px",
                        fontSize: "28px"
                    }}>
                        Đăng nhập Vendor
                    </h2>

                    <form onSubmit={handleLogin}>
                        <div style={{ marginBottom: "20px" }}>
                            <label style={{ display: "block", marginBottom: "8px", fontWeight: "500" }}>
                                Email
                            </label>
                            <input
                                type="email"
                                placeholder="example@email.com"
                                value={email}
                                onChange={e => setEmail(e.target.value)}
                                style={{
                                    width: "100%",
                                    padding: "12px",
                                    border: "1px solid #ddd",
                                    borderRadius: "8px",
                                    fontSize: "16px"
                                }}
                                required
                            />
                        </div>

                        <div style={{ marginBottom: "20px" }}>
                            <label style={{ display: "block", marginBottom: "8px", fontWeight: "500" }}>
                                Mật khẩu
                            </label>
                            <input
                                type="password"
                                placeholder="••••••••••••"
                                value={password}
                                onChange={e => setPassword(e.target.value)}
                                style={{
                                    width: "100%",
                                    padding: "12px",
                                    border: "1px solid #ddd",
                                    borderRadius: "8px",
                                    fontSize: "16px"
                                }}
                                required
                            />
                        </div>

                        <p style={{ textAlign: "right", marginBottom: "25px" }}>
                            <a href="#" style={{ color: "#86542B", textDecoration: "none" }}>
                                Quên mật khẩu?
                            </a>
                        </p>

                        <button
                            type="submit"
                            disabled={loading}
                            style={{
                                width: "100%",
                                padding: "14px",
                                background: loading ? "#a67c5e" : "#86542B",
                                color: "white",
                                border: "none",
                                borderRadius: "30px",
                                fontSize: "16px",
                                cursor: loading ? "not-allowed" : "pointer",
                                fontWeight: "600"
                            }}
                        >
                            {loading ? "Đang đăng nhập..." : "Đăng nhập Vendor"}
                        </button>
                    </form>

                    <p style={{ textAlign: "center", marginTop: "30px", color: "#555" }}>
                        Chưa có tài khoản Vendor?{" "}
                        <span
                            onClick={() => navigate("/vendor/register")}
                            style={{
                                color: "#86542B",
                                cursor: "pointer",
                                fontWeight: "bold"
                            }}
                        >
                            Đăng ký ngay
                        </span>
                    </p>
                </div>

                {/* Bên phải - Hình nền */}
                <div style={{
                    flex: 1,
                    position: "relative",
                    minHeight: "560px",
                    overflow: "hidden"
                }}>
                    <img
                        src="/assets/thumbnail/LogReg.png"
                        alt="Pet shop background"
                        style={{
                            position: "absolute",
                            top: 0,
                            left: 0,
                            width: "100%",
                            height: "100%",
                            objectFit: "cover"
                        }}
                    />
                </div>
            </div>
        </div>
    );
}

export default LoginVendorPage;