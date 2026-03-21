import { useEffect, useState } from "react";
import axios from "axios";
import {
    Card,
    Row,
    Col,
    Modal,
    Button,
    Descriptions,
    Spin,
    Typography,
    Divider,
    message,
    Form,
    Input,
    DatePicker,
    List
} from "antd";
import dayjs from "dayjs";

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
}

export default function ProfilePage() {

    const [pets, setPets] = useState<Pet[]>([]);
    const [user, setUser] = useState<UserProfile | null>(null);

    const [loading, setLoading] = useState(true);

    const [editPet, setEditPet] = useState<Pet | null>(null);
    const [editUserOpen, setEditUserOpen] = useState(false);

    const [historyOpen, setHistoryOpen] = useState(false);
    const [historyPetName, setHistoryPetName] = useState("");
    const [petHistory, setPetHistory] = useState<PetHistory[]>([]);

    const [petForm] = Form.useForm();
    const [userForm] = Form.useForm();

    const token = localStorage.getItem("token");

    const loadData = async () => {

        try {

            const petRes = await axios.get(
                "http://localhost:5263/api/pets",
                { headers: { Authorization: `Bearer ${token}` } }
            );

            setPets(petRes.data);

            const userRes = await axios.get(
                "http://localhost:5263/api/users/me",
                { headers: { Authorization: `Bearer ${token}` } }
            );

            setUser(userRes.data);

        }
        catch {

            message.error("Không tải được dữ liệu");

        }
        finally {

            setLoading(false);

        }

    };

    useEffect(() => {
        loadData();
    }, []);

    const openEditPet = (pet: Pet) => {

        setEditPet(pet);

        petForm.setFieldsValue({
            ...pet,
            dateOfBirth: pet.dateOfBirth ? dayjs(pet.dateOfBirth) : null
        });

    };

    const handleUpdatePet = async (values: any) => {

        try {

            await axios.put(
                `http://localhost:5263/api/pets/${editPet?.id}`,
                {
                    ...values,
                    dateOfBirth: values.dateOfBirth?.format("YYYY-MM-DD")
                },
                { headers: { Authorization: `Bearer ${token}` } }
            );

            message.success("Cập nhật pet thành công");

            setEditPet(null);

            loadData();

        }
        catch {

            message.error("Cập nhật pet thất bại");

        }

    };

    const openEditUser = () => {

        if (!user) return;

        userForm.setFieldsValue(user);

        setEditUserOpen(true);

    };

    const handleUpdateUser = async (values: any) => {

        try {

            await axios.put(
                "http://localhost:5263/api/users/me",
                values,
                { headers: { Authorization: `Bearer ${token}` } }
            );

            message.success("Cập nhật thông tin thành công");

            setEditUserOpen(false);

            loadData();

        }
        catch {

            message.error("Cập nhật thất bại");

        }

    };

    const openPetHistory = async (pet: Pet) => {

        try {

            const res = await axios.get(
                `http://localhost:5263/api/bookings/pet/${pet.id}`,
                { headers: { Authorization: `Bearer ${token}` } }
            );

            setPetHistory(res.data);
            setHistoryPetName(pet.name);
            setHistoryOpen(true);

        }
        catch {

            message.error("Không tải được lịch sử");

        }

    };

    if (loading)
        return <Spin style={{ marginTop: 80 }} size="large" />;

    return (

        <div style={{ padding: 40, maxWidth: 1200, margin: "0 auto" }}>

            <Title level={2}>Trang cá nhân</Title>

            <Divider />

            <Title level={3}>Thú cưng của bạn</Title>

            <Row gutter={[20, 20]}>

                {pets.map(pet => (

                    <Col xs={24} sm={12} md={8} lg={6} key={pet.id}>

                        <Card
                            cover={
                                <img
                                    src={
                                        pet.profileImageUrl ||
                                        "https://cdn-icons-png.flaticon.com/512/616/616408.png"
                                    }
                                    style={{ height: 200, objectFit: "cover" }}
                                />
                            }
                            actions={[

                                <Button type="link" onClick={() => openEditPet(pet)}>
                                    Chỉnh sửa
                                </Button>,

                                <Button type="link" onClick={() => openPetHistory(pet)}>
                                    Lịch sử chăm sóc
                                </Button>

                            ]}
                        >

                            <Descriptions column={1} size="small">

                                <Descriptions.Item label="Tên">
                                    {pet.name}
                                </Descriptions.Item>

                                <Descriptions.Item label="Loài">
                                    {pet.species}
                                </Descriptions.Item>

                                <Descriptions.Item label="Giống">
                                    {pet.breed || "-"}
                                </Descriptions.Item>

                                <Descriptions.Item label="Giới tính">
                                    {pet.gender || "-"}
                                </Descriptions.Item>

                                <Descriptions.Item label="Ngày sinh">
                                    {pet.dateOfBirth || "-"}
                                </Descriptions.Item>

                                <Descriptions.Item label="Màu sắc">
                                    {pet.color || "-"}
                                </Descriptions.Item>

                                <Descriptions.Item label="Cân nặng">
                                    {pet.weight ? `${pet.weight} kg` : "-"}
                                </Descriptions.Item>

                                <Descriptions.Item label="Ghi chú">
                                    {pet.notes || "-"}
                                </Descriptions.Item>

                            </Descriptions>

                        </Card>

                    </Col>

                ))}

            </Row>

            <Divider style={{ margin: "40px 0" }} />

            <Title level={3}>Thông tin cá nhân</Title>

            {user && (

                <Card
                    extra={
                        <Button type="primary" onClick={openEditUser}>
                            Chỉnh sửa
                        </Button>
                    }
                >

                    <Descriptions column={1} bordered>

                        <Descriptions.Item label="Họ tên">
                            {user.fullName}
                        </Descriptions.Item>

                        <Descriptions.Item label="Email">
                            {user.email}
                        </Descriptions.Item>

                        <Descriptions.Item label="Số điện thoại">
                            {user.phoneNumber}
                        </Descriptions.Item>

                    </Descriptions>

                </Card>

            )}

            {/* HISTORY MODAL */}

            <Modal
                open={historyOpen}
                title={`Lịch sử chăm sóc của ${historyPetName}`}
                onCancel={() => setHistoryOpen(false)}
                footer={null}
            >

                <List
                    dataSource={petHistory}
                    renderItem={(item) => (

                        <List.Item>

                            <List.Item.Meta
                                title={item.storeName}
                                description={
                                    <>
                                        <div>Dịch vụ: {item.serviceNames.join(", ")}</div>
                                        <div>Ngày: {item.bookingDate}</div>
                                        <div>Giờ: {item.startTime}</div>
                                    </>
                                }
                            />

                        </List.Item>

                    )}
                />

            </Modal>

            {/* EDIT PET MODAL */}

            <Modal
                open={!!editPet}
                title="Chỉnh sửa pet"
                onCancel={() => setEditPet(null)}
                footer={null}
            >

                <Form form={petForm} layout="vertical" onFinish={handleUpdatePet}>

                    <Form.Item name="name" label="Tên" rules={[{ required: true }]}>
                        <Input />
                    </Form.Item>

                    <Form.Item name="species" label="Loài">
                        <Input />
                    </Form.Item>

                    <Form.Item name="breed" label="Giống">
                        <Input />
                    </Form.Item>

                    <Form.Item name="gender" label="Giới tính">
                        <Input />
                    </Form.Item>

                    <Form.Item name="dateOfBirth" label="Ngày sinh">
                        <DatePicker style={{ width: "100%" }} />
                    </Form.Item>

                    <Form.Item name="color" label="Màu sắc">
                        <Input />
                    </Form.Item>

                    <Form.Item name="weight" label="Cân nặng">
                        <Input type="number" />
                    </Form.Item>

                    <Form.Item name="notes" label="Ghi chú">
                        <Input.TextArea />
                    </Form.Item>

                    <Button type="primary" htmlType="submit" block>
                        Lưu thay đổi
                    </Button>

                </Form>

            </Modal>

            {/* EDIT USER MODAL */}

            <Modal
                open={editUserOpen}
                title="Chỉnh sửa thông tin cá nhân"
                onCancel={() => setEditUserOpen(false)}
                footer={null}
            >

                <Form form={userForm} layout="vertical" onFinish={handleUpdateUser}>

                    <Form.Item name="fullName" label="Họ tên">
                        <Input />
                    </Form.Item>

                    <Form.Item name="phoneNumber" label="Số điện thoại">
                        <Input />
                    </Form.Item>

                    <Button type="primary" htmlType="submit" block>
                        Cập nhật
                    </Button>

                </Form>

            </Modal>

        </div>

    );

}