import { useState } from "react";
import { useNavigate } from "react-router-dom";

function LoginPage() {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const navigate = useNavigate();

    const handleLogin = async (e: React.FormEvent) => {
        e.preventDefault();

        try {
            const res = await fetch("http://localhost:5263/api/auth/login", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ email, password })
            });

            if (!res.ok) {
                alert("Sai tài khoản hoặc mật khẩu");
                return;
            }

            const data = await res.json();
            localStorage.setItem("token", data.token);
            // Nếu bạn muốn lấy role từ token, có thể decode JWT ở đây (hoặc sửa backend trả thêm role)

            navigate("/");
        } catch (err) {
            console.error(err);
            alert("Lỗi server");
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
            {/* Card chính - chỉ chiếm ~50% màn hình */}
            <div style={{
                display: "flex",
                width: "100%",
                maxWidth: "1200px",           // ← đây là dòng quan trọng
                borderRadius: "20px",
                overflow: "hidden",
                boxShadow: "0 10px 40px rgba(0, 0, 0, 0.4)"
            }}>
                {/* Bên trái - Form */}
                <div style={{
                    flex: 1,
                    maxWidth: "30%",
                    background: "white",
                    padding: "50px 40px",
                    minWidth: "400px"
                }}>
                    <h2 style={{ color: "#86542B", textAlign: "center", marginBottom: "30px" }}>Đăng nhập</h2>

                    <form onSubmit={handleLogin}>
                        <div style={{ marginBottom: "20px" }}>
                            <label>Email</label>
                            <input
                                type="email"
                                placeholder="example@email.com"
                                value={email}
                                onChange={e => setEmail(e.target.value)}
                                style={{ width: "100%", padding: "12px", border: "1px solid #ddd", borderRadius: "8px" }}
                            />
                        </div>

                        <div style={{ marginBottom: "20px" }}>
                            <label>Mật khẩu</label>
                            <input
                                type="password"
                                placeholder="••••••••••••"
                                value={password}
                                onChange={e => setPassword(e.target.value)}
                                style={{ width: "100%", padding: "12px", border: "1px solid #ddd", borderRadius: "8px" }}
                            />
                        </div>

                        <p style={{ textAlign: "right", marginBottom: "20px" }}>
                            <a href="#" style={{ color: "#86542B", textDecoration: "none" }}>Quên mật khẩu</a>
                        </p>

                        <button
                            type="submit"
                            style={{
                                width: "100%",
                                padding: "14px",
                                background: "#86542B",
                                color: "white",
                                border: "none",
                                borderRadius: "30px",
                                fontSize: "16px",
                                cursor: "pointer"
                            }}
                        >
                            Đăng nhập
                        </button>
                    </form>

                    <p style={{ textAlign: "center", marginTop: "25px" }}>
                        Chưa có tài khoản?{" "}
                        <span
                            onClick={() => navigate("/register")}
                            style={{ color: "#86542B", cursor: "pointer", fontWeight: "bold" }}
                        >
                            Tạo tài khoản
                        </span>
                    </p>
                </div>

                {/* Bên phải - Hình nền (giữ nguyên như ảnh mẫu) */}
                <div style={{
                    flex: 1,
                    position: "relative",
                    minHeight: "560px",           // chiều cao vừa đủ với ảnh mẫu
                    overflow: "hidden"
                }}>
                    <img
                        src="/assets/thumbnail/LogReg.png"
                        alt="animals"
                        style={{
                            position: "absolute",
                            top: 0,
                            left: 0,
                            width: "100%",
                            height: "100%",
                            objectFit: "cover"        // ← quan trọng nhất: ảnh full, không méo, không nhỏ
                        }}
                    />
                </div>
            </div>
        </div>
    );
}

export default LoginPage;