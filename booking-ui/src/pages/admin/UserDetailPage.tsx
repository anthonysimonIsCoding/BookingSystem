// src/pages/admin/UserDetailPage.tsx
import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import axiosInstance from "../../utils/axiosInstance";
import AdminSidebar from "../../components/admin/AdminSidebar";

interface Pet {
    id: string;
    name: string;
    species?: string;
    breed?: string;
    gender?: string;
    dateOfBirth?: string;
    color?: string;
    weight?: number;
    profileImageUrl?: string;
    isActive: boolean;
    notes?: string;
}

export default function UserDetailPage() {
    const { userId } = useParams<{ userId: string }>();
    const navigate = useNavigate();

    const [user, setUser] = useState<any>(null);
    const [pets, setPets] = useState<Pet[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        if (!userId) return;
        loadUserDetail();
    }, [userId]);

    const loadUserDetail = async () => {
        setLoading(true);
        try {
            const res = await axiosInstance.get(`/admin/users/${userId}`);
            setUser(res.data);
            setPets(res.data.pets || []);
        } catch (err) {
            console.error(err);
            alert("Không thể tải thông tin người dùng");
        } finally {
            setLoading(false);
        }
    };

    if (loading) return <div style={{ padding: "100px", textAlign: "center" }}>Đang tải thông tin người dùng...</div>;
    if (!user) return <div>Không tìm thấy người dùng</div>;

    return (
        <div style={{ display: "flex", minHeight: "100vh" }}>
            <AdminSidebar />

            <div style={{ marginLeft: "260px", flex: 1, padding: "30px 40px", background: "#f8f5f0" }}>
                <div style={{ maxWidth: "1200px", margin: "0 auto" }}>
                    <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "30px" }}>
                        <div>
                            <h1 style={{ fontSize: "36px", fontWeight: "700", color: "#333" }}>{user.fullName}</h1>
                            <p style={{ color: "#666" }}>ID: {user.id}</p>
                        </div>
                        <button
                            onClick={() => navigate("/admin/users")}
                            style={{ padding: "12px 24px", background: "#666", color: "white", border: "none", borderRadius: "10px", cursor: "pointer" }}
                        >
                            ← Quay lại danh sách người dùng
                        </button>
                    </div>

                    {/* Thông tin cá nhân */}
                    <div style={{ background: "white", borderRadius: "20px", padding: "32px", marginBottom: "30px", boxShadow: "0 4px 20px rgba(0,0,0,0.06)" }}>
                        <h2>Thông tin cá nhân</h2>
                        <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "20px", marginTop: "20px" }}>
                            <div><strong>Email:</strong> {user.email}</div>
                            <div><strong>Số điện thoại:</strong> {user.phoneNumber || "Chưa cập nhật"}</div>
                            {/*<div><strong>Trạng thái:</strong>*/}
                            {/*    <span style={{ color: user.isActive ? "#10b981" : "#ef4444", fontWeight: "600" }}>*/}
                            {/*        {user.isActive ? " Hoạt động" : " Bị hạn chế"}*/}
                            {/*    </span>*/}
                            {/*</div>*/}
                            <div><strong>Ngày tạo:</strong> {new Date(user.createdAt).toLocaleDateString('vi-VN')}</div>
                        </div>
                    </div>

                    {/* Danh sách Pet */}
                    <div style={{ background: "white", borderRadius: "20px", padding: "32px", boxShadow: "0 4px 20px rgba(0,0,0,0.06)" }}>
                        <h2>Thú cưng của người dùng ({pets.length})</h2>

                        {pets.length === 0 ? (
                            <div style={{ textAlign: "center", padding: "60px", color: "#888" }}>
                                Người dùng này chưa có thú cưng nào.
                            </div>
                        ) : (
                            <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(300px, 1fr))", gap: "20px", marginTop: "20px" }}>
                                {pets.map(pet => (
                                    <div key={pet.id} style={{
                                        border: "1px solid #eee",
                                        borderRadius: "16px",
                                        padding: "20px",
                                        background: pet.isActive ? "#fff" : "#f9f9f9"
                                    }}>
                                        <div style={{ textAlign: "center", marginBottom: "16px" }}>
                                            <img
                                                src={pet.profileImageUrl || "https://cdn-icons-png.flaticon.com/512/616/616408.png"}
                                                alt={pet.name}
                                                style={{ width: "140px", height: "140px", objectFit: "cover", borderRadius: "50%", border: "4px solid #f0f0f0" }}
                                            />
                                        </div>

                                        <h3 style={{ textAlign: "center", margin: "0 0 12px 0" }}>{pet.name}</h3>

                                        <div style={{ fontSize: "14px", lineHeight: "1.8" }}>
                                            <div><strong>Loài:</strong> {pet.species || "Không rõ"}</div>
                                            <div><strong>Giống:</strong> {pet.breed || "Không rõ"}</div>
                                            <div><strong>Giới tính:</strong> {pet.gender || "Không rõ"}</div>
                                            <div><strong>Ngày sinh:</strong> {pet.dateOfBirth || "Không rõ"}</div>
                                            <div><strong>Cân nặng:</strong> {pet.weight ? `${pet.weight} kg` : "Không rõ"}</div>
                                            <div><strong>Trạng thái:</strong>
                                                <span style={{ color: pet.isActive ? "#10b981" : "#ef4444" }}>
                                                    {pet.isActive ? " Hoạt động" : " Không hoạt động"}
                                                </span>
                                            </div>
                                        </div>

                                        {pet.notes && (
                                            <div style={{ marginTop: "16px", fontSize: "13px", color: "#555" }}>
                                                <strong>Ghi chú:</strong> {pet.notes}
                                            </div>
                                        )}
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
}