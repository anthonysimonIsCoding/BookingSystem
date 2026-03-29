import { useState } from "react";
import { useNavigate } from "react-router-dom";

function RegisterPage() {
    const [fullName, setFullName] = useState("");
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [confirmPassword, setConfirmPassword] = useState("");
    const navigate = useNavigate();

    const handleRegister = async (e: React.FormEvent) => {
        e.preventDefault();

        if (password !== confirmPassword) {
            alert("Mật khẩu xác nhận không khớp!");
            return;
        }

        try {
            const res = await fetch("http://localhost:5263/api/auth/register", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ email, fullName, password })
            });

            if (!res.ok) {
                const errorText = await res.text();
                alert(errorText || "Đăng ký thất bại");
                return;
            }

            alert("Đăng ký thành công! Vui lòng đăng nhập.");
            navigate("/login");
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
                    <h2 style={{ color: "#86542B", textAlign: "center", marginBottom: "30px" }}>Đăng ký</h2>

                <form onSubmit={handleRegister}>
                    <div style={{ marginBottom: "20px" }}>
                        <label>Họ và tên</label>
                        <input
                            type="text"
                            placeholder="Nhập họ và tên"
                            value={fullName}
                            onChange={e => setFullName(e.target.value)}
                            style={{ width: "100%", padding: "12px", border: "1px solid #ddd", borderRadius: "8px" }}
                        />
                    </div>

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

                    <div style={{ marginBottom: "30px" }}>
                        <label>Xác nhận mật khẩu</label>
                        <input
                            type="password"
                            placeholder="••••••••••••"
                            value={confirmPassword}
                            onChange={e => setConfirmPassword(e.target.value)}
                            style={{ width: "100%", padding: "12px", border: "1px solid #ddd", borderRadius: "8px" }}
                        />
                    </div>

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
                        Đăng ký
                    </button>
                </form>

                <p style={{ textAlign: "center", marginTop: "20px" }}>
                    Đã có tài khoản?{" "}
                    <span
                        onClick={() => navigate("/login")}
                            style={{ color: "#86542B", cursor: "pointer", fontWeight: "bold" }}
                    >
                        Đăng nhập ngay
                    </span>
                </p>
            </div>

            {/* Bên phải - Giữ nguyên background màu + động vật giống ảnh */}
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
            </div></div>
    );
}

export default RegisterPage;