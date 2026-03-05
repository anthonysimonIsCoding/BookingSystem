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
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({ email, password })
            });

            if (!res.ok) {
                alert("Sai tài khoản hoặc mật khẩu");
                return;
            }

            const data = await res.json();

            // lưu token
            localStorage.setItem("token", data.token);
            localStorage.setItem("role", data.role);

            navigate("/");
        } catch (err) {
            console.error(err);
            alert("Lỗi server");
        }
    };

    return (
        <div style={{ padding: "40px" }}>
            <h2>Login</h2>
            <form onSubmit={handleLogin}>
                <div>
                    <input
                        type="email"
                        placeholder="Email"
                        value={email}
                        onChange={e => setEmail(e.target.value)}
                    />
                </div>

                <div>
                    <input
                        type="password"
                        placeholder="Password"
                        value={password}
                        onChange={e => setPassword(e.target.value)}
                    />
                </div>

                <button type="submit">Login</button>
            </form>
        </div>
    );
}

export default LoginPage;