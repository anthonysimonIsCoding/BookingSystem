import { useParams } from "react-router-dom";
import { useEffect, useState } from "react";
import axios from "axios";
import TimeSlotCard from "../components/TimeSlotCard";
import { Modal, Button, Form, Select, Input, message, DatePicker, Spin, Checkbox } from "antd";
import type { FormInstance } from "antd";
import dayjs from "dayjs";
import Navbar from '../components/Navbar';

interface TimeSlot {
    id: string;
    startTime: string;
    endTime: string;
    capacity: number;
    remainingCapacity: number;
    isAvailable: boolean;
}

interface StoreDetail {
    id: string;
    name: string;
    address: string;
    averageRating: number;
    reviewCount: number;
    totalCompletedBookings: number;
    images: { imageUrl: string; isThumbnail: boolean }[];
}

interface Pet {
    id: string;
    name: string;
}

interface Service {
    id: string;
    name: string;
    price: number;
    durationMinutes: number;
}

export default function StoreDetailPage() {

    const { id } = useParams<{ id: string }>();

    const [store, setStore] = useState<StoreDetail | null>(null);
    const [slots, setSlots] = useState<TimeSlot[]>([]);
    const [pets, setPets] = useState<Pet[]>([]);
    const [services, setServices] = useState<Service[]>([]);

    const [loading, setLoading] = useState(true);
    const [loadingSlots, setLoadingSlots] = useState(false);

    const [selectedDate, setSelectedDate] = useState<string>(() =>
        dayjs().format("YYYY-MM-DD")
    );

    const [isModalOpen, setIsModalOpen] = useState(false);
    const [selectedSlotId, setSelectedSlotId] = useState<string | null>(null);

    const [form] = Form.useForm<FormInstance>();


    const disableDate = (current: dayjs.Dayjs) => {
        const today = dayjs().startOf("day");
        const maxDate = today.add(14, "day");
        return current && (current < today || current > maxDate);
    };


    const loadSlots = async (date: string) => {

        if (!id) return;

        setLoadingSlots(true);

        try {

            const token = localStorage.getItem("token");

            const res = await axios.get<TimeSlot[]>(
                `http://localhost:5263/api/timeslots/store/${id}?date=${date}`,
                { headers: { Authorization: `Bearer ${token}` } }
            );

            setSlots(res.data);

        } catch {

            message.error("Không tải được khung giờ");

        } finally {

            setLoadingSlots(false);

        }

    };


    useEffect(() => {

        if (!id) return;

        const loadAll = async () => {

            setLoading(true);

            const token = localStorage.getItem("token");

            try {

                const storeRes = await axios.get(
                    `http://localhost:5263/api/stores/${id}`,
                    { headers: { Authorization: `Bearer ${token}` } }
                );

                setStore(storeRes.data);


                const petsRes = await axios.get(
                    `http://localhost:5263/api/pets`,
                    { headers: { Authorization: `Bearer ${token}` } }
                );

                setPets(petsRes.data);


                const serviceRes = await axios.get(
                    `http://localhost:5263/api/services/store/${id}`,
                    { headers: { Authorization: `Bearer ${token}` } }
                );

                setServices(serviceRes.data);

            }
            catch {

                message.error("Lỗi tải dữ liệu");

            }
            finally {

                setLoading(false);

            }

        };

        loadAll();

    }, [id]);


    useEffect(() => {

        loadSlots(selectedDate);

    }, [selectedDate]);


    const handleBook = (slotId: string) => {

        setSelectedSlotId(slotId);

        setIsModalOpen(true);

        form.resetFields();

    };


    const handleSubmit = async (values: any) => {

        if (!selectedSlotId || !id) return;

        try {

            const token = localStorage.getItem("token");

            let petId = values.petId;

            if (values.petId === "new") {

                const petRes = await axios.post(
                    "http://localhost:5263/api/pets",
                    {
                        name: values.newPetName,
                        species: values.newPetSpecies,
                        breed: values.newPetBreed
                    },
                    { headers: { Authorization: `Bearer ${token}` } }
                );

                petId = petRes.data.id;

            }


            await axios.post(
                "http://localhost:5263/api/bookings",
                {
                    storeId: id,
                    timeSlotId: selectedSlotId,
                    bookingDate: selectedDate,
                    petId: petId,
                    serviceIds: values.serviceIds
                },
                { headers: { Authorization: `Bearer ${token}` } }
            );


            message.success("Đặt lịch thành công 🎉");

            setIsModalOpen(false);

            loadSlots(selectedDate);

        }
        catch (err: any) {

            message.error(err.response?.data || "Lỗi đặt lịch");

        }

    };


    if (loading)
        return <div style={{ textAlign: "center", marginTop: 80 }}>
            <Spin size="large" />
        </div>;


    if (!store)
        return <div>Không tìm thấy cửa hàng</div>;


    const thumbnail =
        store.images?.find(i => i.isThumbnail)?.imageUrl ||
        store.images?.[0]?.imageUrl ||
        "https://picsum.photos/900/500";


    return (
        <div className="min-h-screen bg-gray-100">
            <Navbar />    
        <div style={{ padding: 40, background: "#f6f7fb", minHeight: "100vh" }}>
             
            <div style={{ maxWidth: 1100, margin: "0 auto" }}>

                {/* Gallery */}

                <div
                    style={{
                        display: "grid",
                        gridTemplateColumns: "2fr 1fr",
                        gap: 12,
                        marginBottom: 32,
                        height: 420
                    }}
                >

                    <img
                        src={thumbnail}
                        alt={store.name}
                        style={{
                            width: "100%",
                            height: "100%",
                            objectFit: "cover",
                            borderRadius: 16
                        }}
                    />

                    <div style={{ display: "grid", gap: 12 }}>

                        {store.images
                            ?.filter(i => !i.isThumbnail)
                            .slice(0, 2)
                            .map((img, idx) => (

                                <img
                                    key={idx}
                                    src={img.imageUrl}
                                    alt=""
                                    style={{
                                        width: "100%",
                                        height: "100%",
                                        objectFit: "cover",
                                        borderRadius: 12
                                    }}
                                />

                            ))}

                    </div>

                </div>


                <h1 style={{ fontSize: 28, fontWeight: 700 }}>
                    {store.name}
                </h1>

                <p style={{ color: "#666", marginBottom: 24 }}>
                    📍 {store.address} • ⭐ {store.averageRating.toFixed(1)} ({store.reviewCount}+ đánh giá)
                </p>


                <h2>Chọn ngày</h2>

                <DatePicker
                    value={dayjs(selectedDate)}
                    format="DD/MM/YYYY"
                    disabledDate={disableDate}
                    onChange={(d) => d && setSelectedDate(d.format("YYYY-MM-DD"))}
                    allowClear={false}
                    style={{ marginBottom: 24 }}
                />


                <h2>Khung giờ</h2>


                {loadingSlots ? (

                    <Spin />

                ) : (

                    <div
                        style={{
                            display: "grid",
                            gridTemplateColumns: "repeat(auto-fit, minmax(220px,1fr))",
                            gap: 16
                        }}
                    >

                        {slots.map(slot => (

                            <TimeSlotCard
                                key={slot.id}
                                slot={slot}
                                selectedDate={selectedDate}
                                onBook={() => handleBook(slot.id)}
                            />

                        ))}

                    </div>

                )}

            </div>


            {/* Modal */}

            <Modal
                title="Thông tin đặt lịch"
                open={isModalOpen}
                footer={null}
                onCancel={() => setIsModalOpen(false)}
            >

                <Form form={form} layout="vertical" onFinish={handleSubmit}>

                    <Form.Item
                        name="petId"
                        label="Chọn thú cưng"
                        rules={[{ required: true }]}
                    >

                        <Select placeholder="Chọn thú cưng">

                            {pets.map(p => (

                                <Select.Option key={p.id} value={p.id}>
                                    {p.name}
                                </Select.Option>

                            ))}

                            <Select.Option value="new">
                                + Thêm thú cưng
                            </Select.Option>

                        </Select>

                    </Form.Item>


                    <Form.Item
                        name="serviceIds"
                        label="Chọn dịch vụ"
                        rules={[{ required: true }]}
                    >

                        <Checkbox.Group>

                            {services.map(s => (

                                <Checkbox key={s.id} value={s.id}>
                                    {s.name} - {s.price.toLocaleString()}đ
                                </Checkbox>

                            ))}

                        </Checkbox.Group>

                    </Form.Item>


                    <Button
                        type="primary"
                        htmlType="submit"
                        block
                        size="large"
                    >

                        Xác nhận đặt lịch

                    </Button>

                </Form>

            </Modal>

        </div>
        </div>
    );

}