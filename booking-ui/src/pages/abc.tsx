import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";
import {
    Card, Row, Col, Modal, Button, Descriptions, Spin, Typography, Divider,
    message, Form, Input, DatePicker, List, Upload, Select, Tag
} from "antd";
import { PlusOutlined, LoadingOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import Navbar from '../components/Navbar';


const { Title } = Typography;

interface Pet {
    id: string;
    name: string;
    species: string;
    breed?: string;
    gender?: string;
    dateOfBirth?: string;
    color?: string;
    weight?: number;
    notes?: string;
    profileImageUrl?: string;
}

interface UserProfile {
    id: string;
    fullName: string;
    email: string;
    phoneNumber: string;
}

interface PetHistory {
    id: string;
    storeName: string;
    serviceNames: string[];
    bookingDate: string;
    startTime: string;
    status?: number;   // 0 = Active, 1 = Cancelled, 2 = Completed
}

export default function ProfilePage() {
    const [pets, setPets] = useState<Pet[]>([]);
    const [user, setUser] = useState<UserProfile | null>(null);
    const [loading, setLoading] = useState(true);

    const [editPet, setEditPet] = useState<Pet | null>(null);
    const [addPetOpen, setAddPetOpen] = useState(false);
    const [editUserOpen, setEditUserOpen] = useState(false);
    const [historyOpen, setHistoryOpen] = useState(false);
    const [historyPetName, setHistoryPetName] = useState("");
    const [petHistory, setPetHistory] = useState<PetHistory[]>([]);

    const [addPreviewUrl, setAddPreviewUrl] = useState<string | null>(null);
    const [editPreviewUrl, setEditPreviewUrl] = useState<string | null>(null);
    const [addFile, setAddFile] = useState<File | null>(null);
    const [editFile, setEditFile] = useState<File | null>(null);

    const [speciesList, setSpeciesList] = useState<any[]>([]);
    const [breedList, setBreedList] = useState<any[]>([]);

    const [petCurrentStatus, setPetCurrentStatus] = useState<Record<string, any>>({});

    const [isAdding, setIsAdding] = useState(false);
    const [isUpdating, setIsUpdating] = useState(false);

    const [petForm] = Form.useForm();
    const [addPetForm] = Form.useForm();
    const [userForm] = Form.useForm();

    const token = localStorage.getItem("token");
    const navigate = useNavigate();

    // ==================== LOAD DATA ====================
    const loadData = async () => {
        try {
            const petRes = await axios.get("http://localhost:5263/api/pets", {
                headers: { Authorization: `Bearer ${token}` }
            });
            setPets(petRes.data);

            const statusMap: Record<string, any> = {};

            for (const pet of petRes.data) {
                try {
                    const res = await axios.get(`http://localhost:5263/api/pets/${pet.id}/latest-booking`, {
                        headers: { Authorization: `Bearer ${token}` }
                    });
                    if (res.data) statusMap[pet.id] = res.data;
                } catch { }
            }
            setPetCurrentStatus(statusMap);

            const userRes = await axios.get("http://localhost:5263/api/users/me", {
                headers: { Authorization: `Bearer ${token}` }
            });
            setUser(userRes.data);
        } catch {
            message.error("Không tải được dữ liệu");
        } finally {
            setLoading(false);
        }
    };

    const loadSpecies = async () => {
        try {
            const res = await axios.get("http://localhost:5263/api/pets/species", {
                headers: { Authorization: `Bearer ${token}` }
            });
            setSpeciesList(res.data);
        } catch { message.error("Không tải loài"); }
    };

    const loadBreeds = async (speciesId: string) => {
        try {
            const res = await axios.get(`http://localhost:5263/api/pets/breeds/${speciesId}`, {
                headers: { Authorization: `Bearer ${token}` }
            });
            setBreedList(res.data);
        } catch { message.error("Không tải giống"); }
    };

    useEffect(() => {
        if (!token) {
            navigate("/login");
            return;
        }
        loadData();
        loadSpecies();
    }, [token]);

    // ==================== ADD & EDIT (giữ nguyên) ====================
    const openAddPet = () => {
        setAddPetOpen(true);
        setAddPreviewUrl(null);
        setAddFile(null);
        setBreedList([]);
        addPetForm.resetFields();
    };

    const handleAddPet = async (values: any) => {
        if (isAdding) return;
        setIsAdding(true);

        let imageUrl = null;
        if (addFile) {
            const formData = new FormData();
            formData.append("file", addFile);
            try {
                const res = await axios.post("http://localhost:5263/api/pets/upload-image", formData, {
                    headers: { Authorization: `Bearer ${token}` }
                });
                imageUrl = res.data.url;
            } catch {
                message.error("Upload ảnh thất bại");
                setIsAdding(false);
                return;
            }
        }

        try {
            await axios.post("http://localhost:5263/api/pets", {
                ...values,
                profileImageUrl: imageUrl,
                dateOfBirth: values.dateOfBirth?.format("YYYY-MM-DD")
            }, { headers: { Authorization: `Bearer ${token}` } });

            message.success("Thêm thú cưng thành công");
            setAddPetOpen(false);
            setAddFile(null);
            setAddPreviewUrl(null);
            loadData();
        } catch {
            message.error("Thêm thú cưng thất bại");
        } finally {
            setIsAdding(false);
        }
    };

    const openEditPet = (pet: Pet) => {
        setEditPet(pet);
        setEditPreviewUrl(pet.profileImageUrl || null);
        setEditFile(null);

        petForm.setFieldsValue({
            name: pet.name,
            species: pet.species,
            breed: pet.breed,
            gender: pet.gender,
            dateOfBirth: pet.dateOfBirth ? dayjs(pet.dateOfBirth) : null,
            color: pet.color,
            weight: pet.weight,
            notes: pet.notes,
            profileImageUrl: pet.profileImageUrl || ""
        });

        const current = speciesList.find(s => s.name === pet.species);
        if (current) loadBreeds(current.id);
        else setBreedList([]);
    };

    const handleUpdatePet = async (values: any) => {
        if (isUpdating) return;
        setIsUpdating(true);

        let imageUrl = values.profileImageUrl || null;
        if (editFile) {
            const formData = new FormData();
            formData.append("file", editFile);
            try {
                const res = await axios.post("http://localhost:5263/api/pets/upload-image", formData, {
                    headers: { Authorization: `Bearer ${token}` }
                });
                imageUrl = res.data.url;
            } catch {
                message.error("Upload ảnh thất bại");
                setIsUpdating(false);
                return;
            }
        }

        try {
            await axios.put(`http://localhost:5263/api/pets/${editPet?.id}`, {
                ...values,
                profileImageUrl: imageUrl,
                dateOfBirth: values.dateOfBirth?.format("YYYY-MM-DD")
            }, { headers: { Authorization: `Bearer ${token}` } });

            message.success("Cập nhật pet thành công");
            setEditPet(null);
            setEditFile(null);
            setEditPreviewUrl(null);
            loadData();
        } catch {
            message.error("Cập nhật pet thất bại");
        } finally {
            setIsUpdating(false);
        }
    };

    const openEditUser = () => {
        if (!user) return;
        userForm.setFieldsValue(user);
        setEditUserOpen(true);
    };

    const handleUpdateUser = async (values: any) => {
        try {
            await axios.put("http://localhost:5263/api/users/me", values, {
                headers: { Authorization: `Bearer ${token}` }
            });
            message.success("Cập nhật thông tin thành công");
            setEditUserOpen(false);
            loadData();
        } catch {
            message.error("Cập nhật thất bại");
        }
    };

    const openPetHistory = async (pet: Pet) => {
        try {
            const res = await axios.get(`http://localhost:5263/api/bookings/pet/${pet.id}`, {
                headers: { Authorization: `Bearer ${token}` }
            });
            setPetHistory(res.data);
            setHistoryPetName(pet.name);
            setHistoryOpen(true);
        } catch {
            message.error("Không tải được lịch sử");
        }
    };

    const handleLogout = () => {
        localStorage.removeItem("token");
        message.success("Đã đăng xuất");
        navigate("/");
    };

    if (loading) return <Spin style={{ marginTop: 80 }} size="large" />;

    return (
        <div className="min-h-screen bg-gray-100">
            <Navbar />
            <div style={{ padding: 40, maxWidth: 1200, margin: "0 auto" }}>
                <Title level={2}>Trang cá nhân</Title>
                <Divider />
                <Title level={3}>Thú cưng của bạn</Title>

                <Row gutter={[20, 20]}>
                    {pets.map(pet => {
                        const status = petCurrentStatus[pet.id];
                        const isPending = status?.status === 0;
                        const isReceive = status?.status === 1;
                        const isCaring = status?.status === 2;
                        const isWaitingPickup = status?.status === 4;
                        const isCompleted = status?.status === 5;

                        let dotColor = "#8c8c8c"; // xám mặc định
                        if (isCaring) dotColor = "#52c41a";     // xanh lá
                        if (isCompleted) dotColor = "#1890ff";  // xanh dương

                        const statusText = status ?
                            (isCaring ? `${pet.name} đang được chăm sóc tại ${status.storeName}` :
                                isCompleted ? `${pet.name} đã được chăm sóc chu đáo tại ${status.storeName} vào ${dayjs().diff(dayjs(status.bookingDate), 'day')} ngày trước` :
                                    "Sen ơi Boss muốn đi chơi...") :
                            isPending ? `${status.storeName} đang chờ ${pet.name} đến đó, sen ơi lẹ lên.` :
                                isReceive ? `${status.storeName} đã nhận bé ${pet.name} rồi nha, xíu nữa bé sẽ được chăm sóc nè` :
                                    isWaitingPickup ? `${pet.name} đang chờ sen đến đón về sau khi được chăm sóc xong tại ${status.storeName}`
                                        : "Sen ơi Boss muốn đi chơi...";

                        return (
                            <Col xs={24} sm={12} md={8} lg={6} key={pet.id}>
                                <Card
                                    cover={
                                        <div style={{ position: "relative" }}>
                                            <img
                                                src={pet.profileImageUrl || "https://cdn-icons-png.flaticon.com/512/616/616408.png"}
                                                style={{ height: 200, objectFit: "cover", width: "100%" }}
                                            />
                                            {/* CHẤM TRÒN GÓC TRÊN TRÁI */}
                                            <div style={{
                                                position: "absolute",
                                                top: 12,
                                                left: 12,
                                                width: 16,
                                                height: 16,
                                                backgroundColor: dotColor,
                                                borderRadius: "50%",
                                                border: "3px solid white",
                                                boxShadow: "0 2px 8px rgba(0,0,0,0.3)"
                                            }} />

                                            {/* TEXT STATUS DƯỚI ẢNH */}
                                            <div style={{
                                                position: "absolute",
                                                bottom: 12,
                                                left: 12,
                                                backgroundColor: "rgba(0,0,0,0.75)",
                                                color: "white",
                                                padding: "6px 14px",
                                                borderRadius: 8,
                                                fontSize: 13.5,
                                                fontWeight: 500,
                                                maxWidth: "88%",
                                                lineHeight: 1.4
                                            }}>
                                                {statusText}
                                            </div>
                                        </div>
                                    }
                                    actions={[
                                        <Button type="link" onClick={() => openEditPet(pet)}>Chỉnh sửa</Button>,
                                        <Button type="link" onClick={() => openPetHistory(pet)}>Lịch sử chăm sóc</Button>
                                    ]}
                                >
                                    <Descriptions column={1} size="small">
                                        <Descriptions.Item label="Tên">{pet.name}</Descriptions.Item>
                                        <Descriptions.Item label="Loài">{pet.species}</Descriptions.Item>
                                        <Descriptions.Item label="Giống">{pet.breed || "-"}</Descriptions.Item>
                                        <Descriptions.Item label="Giới tính">{pet.gender || "-"}</Descriptions.Item>
                                        <Descriptions.Item label="Ngày sinh">{pet.dateOfBirth || "-"}</Descriptions.Item>
                                        <Descriptions.Item label="Màu sắc">{pet.color || "-"}</Descriptions.Item>
                                        <Descriptions.Item label="Cân nặng">{pet.weight ? `${pet.weight} kg` : "-"}</Descriptions.Item>
                                        <Descriptions.Item label="Ghi chú">{pet.notes || "-"}</Descriptions.Item>
                                    </Descriptions>
                                </Card>
                            </Col>
                        );
                    })}

                    <Col xs={24} sm={12} md={8} lg={6}>
                        <Card hoverable style={{ height: "100%", borderStyle: "dashed", borderColor: "#00AEEF", cursor: "pointer" }}
                            cover={
                                <div style={{ height: 200, background: "#f0f8ff", display: "flex", alignItems: "center", justifyContent: "center" }}>
                                    <PlusOutlined style={{ fontSize: 80, color: "#00AEEF" }} />
                                </div>
                            }
                            onClick={openAddPet}>
                            <div style={{ textAlign: "center", padding: "16px 0" }}>
                                <Title level={4} style={{ color: "#00AEEF", margin: 0 }}>Thêm thú cưng mới</Title>
                                <p style={{ color: "#666", marginTop: 8 }}>Nhấn để thêm Boss mới</p>
                            </div>
                        </Card>
                    </Col>
                </Row>

                <Divider style={{ margin: "40px 0" }} />
                <Title level={3}>Thông tin cá nhân</Title>
                {user && (
                    <Card extra={<Button type="primary" onClick={openEditUser}>Chỉnh sửa</Button>}>
                        <Descriptions column={1} bordered>
                            <Descriptions.Item label="Họ tên">{user.fullName}</Descriptions.Item>
                            <Descriptions.Item label="Email">{user.email}</Descriptions.Item>
                            <Descriptions.Item label="Số điện thoại">{user.phoneNumber}</Descriptions.Item>
                        </Descriptions>
                    </Card>
                )}

                <div style={{ textAlign: "center", marginTop: 40 }}>
                    <Button type="primary" danger size="large" onClick={handleLogout}>Đăng xuất</Button>
                </div>

                {/* HISTORY MODAL - ĐÃ SỬA ĐÚNG STATUS */}
                <Modal open={historyOpen} title={`Lịch sử chăm sóc của ${historyPetName}`} onCancel={() => setHistoryOpen(false)} footer={null}>
                    <List
                        dataSource={petHistory}
                        renderItem={(item) => {
                            let statusText = "Không xác định";
                            let color = "grey";
                            if (item.status === 0) { statusText = "Đang chăm sóc"; color = "green"; }
                            if (item.status === 1) { statusText = "Đã hủy"; color = "red"; }
                            if (item.status === 2) { statusText = "Hoàn thành"; color = "blue"; }
                            return (
                                <List.Item>
                                    <List.Item.Meta
                                        title={item.storeName}
                                        description={
                                            <>
                                                <div>Dịch vụ: {item.serviceNames.join(", ")}</div>
                                                <div>Ngày: {item.bookingDate}</div>
                                                <div>Giờ: {item.startTime}</div>
                                                <Tag color={color} style={{ marginTop: 6 }}>{statusText}</Tag>
                                            </>
                                        }
                                    />
                                </List.Item>
                            );
                        }}
                    />
                </Modal>

                {/* ADD PET MODAL */}
                <Modal open={addPetOpen} title="Thêm thú cưng mới" onCancel={() => { setAddPetOpen(false); setAddPreviewUrl(null); setAddFile(null); setBreedList([]); }} footer={null}>
                    <Form form={addPetForm} layout="vertical" onFinish={handleAddPet}>
                        <Form.Item name="name" label="Tên" rules={[{ required: true }]}><Input /></Form.Item>
                        <Form.Item name="species" label="Loài" rules={[{ required: true }]}>
                            <Select placeholder="Chọn loài" onChange={(value, option: any) => {
                                if (option?.key) { loadBreeds(option.key); addPetForm.setFieldsValue({ breed: undefined }); }
                            }}>
                                {speciesList.map(s => <Select.Option key={s.id} value={s.name}>{s.name}</Select.Option>)}
                            </Select>
                        </Form.Item>
                        <Form.Item name="breed" label="Giống">
                            <Select placeholder="Chọn giống" disabled={breedList.length === 0}>
                                {breedList.map(b => <Select.Option key={b.id} value={b.name}>{b.name}</Select.Option>)}
                            </Select>
                        </Form.Item>
                        <Form.Item name="gender" label="Giới tính"><Input /></Form.Item>
                        <Form.Item name="dateOfBirth" label="Ngày sinh"><DatePicker style={{ width: "100%" }} /></Form.Item>
                        <Form.Item name="color" label="Màu sắc"><Input /></Form.Item>
                        <Form.Item name="weight" label="Cân nặng"><Input type="number" /></Form.Item>
                        <Form.Item name="notes" label="Ghi chú"><Input.TextArea /></Form.Item>

                        <Form.Item label="Ảnh đại diện pet">
                            <Upload
                                listType="picture-card"
                                beforeUpload={(file) => {
                                    const isImage = file.type.startsWith("image/");
                                    const isLt10M = file.size / 1024 / 1024 < 10;
                                    if (!isImage) message.error("Chỉ được upload ảnh!");
                                    if (!isLt10M) message.error("Ảnh phải < 10MB!");
                                    if (isImage && isLt10M) {
                                        setAddFile(file);
                                        setAddPreviewUrl(URL.createObjectURL(file));
                                    }
                                    return false;
                                }}
                                showUploadList={false}
                            >
                                {addPreviewUrl ? <img src={addPreviewUrl} alt="preview" style={{ width: "100%" }} /> : (
                                    <div><PlusOutlined style={{ fontSize: 30 }} /><div style={{ marginTop: 8 }}>Chọn ảnh</div></div>
                                )}
                            </Upload>
                        </Form.Item>

                        <Button type="primary" htmlType="submit" block disabled={isAdding} icon={isAdding ? <LoadingOutlined /> : null}>
                            {isAdding ? "Đang thêm..." : "Thêm thú cưng"}
                        </Button>
                    </Form>
                </Modal>

                {/* EDIT PET MODAL */}
                <Modal open={!!editPet} title="Chỉnh sửa pet" onCancel={() => { setEditPet(null); setEditPreviewUrl(null); setEditFile(null); setBreedList([]); }} footer={null}>
                    <Form form={petForm} layout="vertical" onFinish={handleUpdatePet}>
                        <Form.Item name="name" label="Tên" rules={[{ required: true }]}><Input /></Form.Item>
                        <Form.Item name="species" label="Loài">
                            <Select placeholder="Chọn loài" onChange={(value, option: any) => {
                                if (option?.key) { loadBreeds(option.key); petForm.setFieldsValue({ breed: undefined }); }
                            }}>
                                {speciesList.map(s => <Select.Option key={s.id} value={s.name}>{s.name}</Select.Option>)}
                            </Select>
                        </Form.Item>
                        <Form.Item name="breed" label="Giống">
                            <Select placeholder="Chọn giống" disabled={breedList.length === 0}>
                                {breedList.map(b => <Select.Option key={b.id} value={b.name}>{b.name}</Select.Option>)}
                            </Select>
                        </Form.Item>
                        <Form.Item name="gender" label="Giới tính"><Input /></Form.Item>
                        <Form.Item name="dateOfBirth" label="Ngày sinh"><DatePicker style={{ width: "100%" }} /></Form.Item>
                        <Form.Item name="color" label="Màu sắc"><Input /></Form.Item>
                        <Form.Item name="weight" label="Cân nặng"><Input type="number" /></Form.Item>
                        <Form.Item name="notes" label="Ghi chú"><Input.TextArea /></Form.Item>

                        <Form.Item label="Ảnh đại diện pet">
                            <Upload
                                listType="picture-card"
                                beforeUpload={(file) => {
                                    const isImage = file.type.startsWith("image/");
                                    const isLt10M = file.size / 1024 / 1024 < 10;
                                    if (!isImage || !isLt10M) {
                                        message.error(isImage ? "Ảnh phải < 10MB!" : "Chỉ upload ảnh!");
                                        return false;
                                    }
                                    setEditFile(file);
                                    setEditPreviewUrl(URL.createObjectURL(file));
                                    return false;
                                }}
                                showUploadList={false}
                            >
                                {editPreviewUrl ? <img src={editPreviewUrl} alt="preview" style={{ width: "100%" }} /> : (
                                    <div><PlusOutlined style={{ fontSize: 30 }} /><div style={{ marginTop: 8 }}>Chọn ảnh mới</div></div>
                                )}
                            </Upload>
                        </Form.Item>

                        <Button type="primary" htmlType="submit" block disabled={isUpdating} icon={isUpdating ? <LoadingOutlined /> : null}>
                            {isUpdating ? "Đang lưu..." : "Lưu thay đổi"}
                        </Button>
                    </Form>
                </Modal>

                {/* EDIT USER MODAL */}
                <Modal open={editUserOpen} title="Chỉnh sửa thông tin cá nhân" onCancel={() => setEditUserOpen(false)} footer={null}>
                    <Form form={userForm} layout="vertical" onFinish={handleUpdateUser}>
                        <Form.Item name="fullName" label="Họ tên"><Input /></Form.Item>
                        <Form.Item name="phoneNumber" label="Số điện thoại"><Input /></Form.Item>
                        <Button type="primary" htmlType="submit" block>Cập nhật</Button>
                    </Form>
                </Modal>
            </div>
        </div>
    );
}   