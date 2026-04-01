// src/pages/StoreDetailPage.tsx
import { useParams } from "react-router-dom";
import { useEffect, useState } from "react";
import api from '../utils/axiosInstance';
import {
    Modal, Button, Form, Select, Input, message, DatePicker, Spin,
    Checkbox, Divider, Radio, Alert, Image
} from "antd";
import dayjs from "dayjs";
import Navbar from '../components/Navbar';
import TimeSlotCard from "../components/TimeSlotCard";

interface TimeSlot {
    id: string;
    startTime: string;
    endTime: string;
    capacity: number;
    remainingCapacity: number;
    isAvailable: boolean;
    isDisabledByOverride?: boolean;
}

interface StoreDetail {
    id: string;
    name: string;
    address: string;
    averageRating: number;
    reviewCount: number;
    totalCompletedBookings: number;
    images: { imageUrl: string; isThumbnail: boolean; order: number }[];
}

interface Pet {
    id: string;
    name: string;
    species: string;
}

interface ServiceOption {
    id: string;
    name: string;
    price: number;
    durationMinutes: number;
}

interface OptionGroup {
    id: string;
    name: string;
    type: number;
    isRequired: boolean;
    options: ServiceOption[];
}

interface Service {
    id: string;
    name: string;
    price: number;
    durationMinutes: number;
    optionGroups: OptionGroup[];
}

export default function StoreDetailPage() {
    const { id } = useParams<{ id: string }>();

    const [store, setStore] = useState<StoreDetail | null>(null);
    const [slots, setSlots] = useState<TimeSlot[]>([]);
    const [pets, setPets] = useState<Pet[]>([]);
    const [services, setServices] = useState<Service[]>([]);
    const [serverTime, setServerTime] = useState<dayjs.Dayjs | null>(null);
    const [platformVouchers, setPlatformVouchers] = useState<any[]>([]);
    const [storeVouchers, setStoreVouchers] = useState<any[]>([]);

    const [loading, setLoading] = useState(true);
    const [loadingSlots, setLoadingSlots] = useState(false);
    const [selectedDate, setSelectedDate] = useState<string>(dayjs().format("YYYY-MM-DD"));
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [selectedSlotId, setSelectedSlotId] = useState<string | null>(null);
    const [selectedServiceId, setSelectedServiceId] = useState<string>("");
    const [selectedOptionIds, setSelectedOptionIds] = useState<string[]>([]);
    const [platformVoucherCode, setPlatformVoucherCode] = useState("");
    const [storeVoucherCode, setStoreVoucherCode] = useState("");
    const [subtotal, setSubtotal] = useState(0);
    const [selectedPetHasActiveBooking, setSelectedPetHasActiveBooking] = useState(false);
    const [activeStoreName, setActiveStoreName] = useState<string>("");

    // Modal xem tất cả ảnh
    const [isImageModalOpen, setIsImageModalOpen] = useState(false);
    const [currentImageIndex, setCurrentImageIndex] = useState(0);

    const [form] = Form.useForm();

    // ==================== LOAD DATA ====================
    useEffect(() => {
        if (!id) return;
        const loadAll = async () => {
            setLoading(true);
            const token = localStorage.getItem("token");
            try {
                const [storeRes, petsRes, servicesRes, timeRes] = await Promise.all([
                    api.get(`http://localhost:5263/api/stores/${id}`, { headers: { Authorization: `Bearer ${token}` } }),
                    api.get("http://localhost:5263/api/pets", { headers: { Authorization: `Bearer ${token}` } }),
                    api.get(`http://localhost:5263/api/services/store/${id}`, { headers: { Authorization: `Bearer ${token}` } }),
                    api.get("http://localhost:5263/api/bookings/server-time", { headers: { Authorization: `Bearer ${token}` } })
                ]);

                setStore(storeRes.data);
                setPets(petsRes.data);
                setServices(servicesRes.data || []);
                setServerTime(dayjs(timeRes.data.vietnamTime));
            } catch (err) {
                console.error(err);
                message.error("Lỗi tải dữ liệu cửa hàng");
            } finally {
                setLoading(false);
            }
        };
        loadAll();
    }, [id]);

    const loadSlots = async (date: string) => {
        if (!id) return;
        setLoadingSlots(true);
        try {
            const token = localStorage.getItem("token");
            const res = await api.get(`http://localhost:5263/api/timeslots/store/${id}?date=${date}`, {
                headers: { Authorization: `Bearer ${token}` }
            });
            setSlots(res.data || []);
        } catch {
            message.error("Không tải được khung giờ");
        } finally {
            setLoadingSlots(false);
        }
    };

    useEffect(() => {
        loadSlots(selectedDate);
    }, [selectedDate]);

    // Load voucher
    useEffect(() => {
        if (!isModalOpen || !id) return;
        const loadVouchers = async () => {
            const token = localStorage.getItem("token");
            try {
                const [pRes, sRes] = await Promise.all([
                    api.get("http://localhost:5263/api/vouchers/platform/available", { headers: { Authorization: `Bearer ${token}` } }),
                    api.get(`http://localhost:5263/api/vouchers/store/${id}/available`, { headers: { Authorization: `Bearer ${token}` } })
                ]);
                setPlatformVouchers(pRes.data);
                setStoreVouchers(sRes.data);
            } catch { }
        };
        loadVouchers();
    }, [isModalOpen, id]);

    const isSlotPast = (slot: TimeSlot) => {
        if (!serverTime) return false;
        const slotDateTime = dayjs(`${selectedDate} ${slot.startTime}`);
        return slotDateTime.isBefore(serverTime);
    };

    const handleBook = (slot: TimeSlot) => {
        if (isSlotPast(slot) || !slot.isAvailable) {
            message.warning("Khung giờ này đã qua hoặc không khả dụng");
            return;
        }
        setSelectedSlotId(slot.id);
        setIsModalOpen(true);
        form.resetFields();
        setSelectedServiceId("");
        setSelectedOptionIds([]);
        setPlatformVoucherCode("");
        setStoreVoucherCode("");
        setSubtotal(0);
        setSelectedPetHasActiveBooking(false);
        setActiveStoreName("");
    };

    useEffect(() => {
        setSelectedOptionIds([]);
    }, [selectedServiceId]);

    // Tính subtotal
    useEffect(() => {
        const service = services.find(s => s.id === selectedServiceId);
        if (!service) return setSubtotal(0);

        const base = service.price;
        const optionsPrice = selectedOptionIds.reduce((sum, id) => {
            const opt = service.optionGroups.flatMap(g => g.options).find(o => o.id === id);
            return sum + (opt?.price || 0);
        }, 0);
        setSubtotal(base + optionsPrice);
    }, [selectedServiceId, selectedOptionIds, services]);

    const checkPetStatus = async (petId: string) => {
        try {
            const token = localStorage.getItem("token");
            const res = await api.get(`http://localhost:5263/api/bookings/pet/${petId}`, {
                headers: { Authorization: `Bearer ${token}` }
            });
            const latest = res.data?.[0];
            const status = Number(latest?.status ?? latest?.Status);
            const hasActive = latest && status !== 4 && status !== 5;

            setSelectedPetHasActiveBooking(hasActive);
            if (hasActive && latest.storeName) {
                setActiveStoreName(latest.storeName);
            } else {
                setActiveStoreName("");
            }
        } catch {
            setSelectedPetHasActiveBooking(false);
            setActiveStoreName("");
        }
    };

    const handleSubmit = async (values: any) => {
        if (selectedPetHasActiveBooking) {
            return message.error(`Pet này đang được chăm sóc tại ${activeStoreName || "một cửa hàng khác"}. Không thể đặt lịch mới.`);
        }
        if (!selectedSlotId || !selectedServiceId) {
            return message.error("Vui lòng chọn dịch vụ");
        }

        try {
            const token = localStorage.getItem("token");
            const res = await api.post("http://localhost:5263/api/bookings", {
                storeId: id,
                petId: values.petId,
                timeSlotId: selectedSlotId,
                bookingDate: selectedDate,
                serviceOptionIds: selectedOptionIds,
                platformVoucherCode: platformVoucherCode.trim() || undefined,
                storeVoucherCode: storeVoucherCode.trim() || undefined,
                notes: values.notes
            }, { headers: { Authorization: `Bearer ${token}` } });

            message.success(`Đặt lịch thành công! Tổng tiền: ${res.data.totalPrice.toLocaleString()}đ`);
            setIsModalOpen(false);
            loadSlots(selectedDate);
        } catch (err: any) {
            message.error(err.response?.data?.title || err.response?.data || "Đặt lịch thất bại");
        }
    };

    if (loading) return <div style={{ textAlign: "center", marginTop: 80 }}><Spin size="large" /></div>;
    if (!store) return <div>Không tìm thấy cửa hàng</div>;

    const allImages = store.images || [];
    const thumbnail = allImages.find(i => i.isThumbnail)?.imageUrl || allImages[0]?.imageUrl || "https://picsum.photos/900/500";
    const otherImages = allImages.filter(i => !i.isThumbnail).sort((a, b) => a.order - b.order);

    // Lấy service đang chọn để render tùy chọn
    const selectedService = services.find(s => s.id === selectedServiceId);

    return (
        <div className="min-h-screen bg-gray-100">
            <Navbar />
            <div style={{ padding: "40px 20px", background: "#f6f7fb", minHeight: "100vh" }}>
                <div style={{ maxWidth: 1200, margin: "0 auto" }}>

                    {/* ==================== PHẦN ẢNH CỬA HÀNG ==================== */}
                    <div style={{ marginBottom: 40, position: 'relative' }}>
                        <div style={{
                            display: "grid",
                            gridTemplateColumns: "2fr 1fr",
                            gap: 16,
                            height: "480px"
                        }}>

                            {/* Ảnh chính lớn */}
                            <div style={{
                                borderRadius: 16,
                                overflow: "hidden",
                                position: "relative",
                                height: "480px",
                                boxShadow: "0 4px 20px rgba(0,0,0,0.1)"
                            }}>
                                <img
                                    src={thumbnail}
                                    alt={store.name}
                                    style={{
                                        width: "100%",
                                        height: "100%",
                                        objectFit: "cover"
                                    }}
                                />

                                {/* DẤU NHỎ Ở GÓC PHẢI DƯỚI - Nhấn để xem tất cả ảnh */}
                                {/*{allImages.length > 1 && (*/}
                                {/*    <div*/}
                                {/*        onClick={() => setIsImageModalOpen(true)}*/}
                                {/*        style={{*/}
                                {/*            position: "absolute",*/}
                                {/*            bottom: "12px",*/}
                                {/*            right: "12px",*/}
                                {/*            background: "rgba(0, 0, 0, 0.75)",*/}
                                {/*            color: "white",*/}
                                {/*            padding: "6px 12px",*/}
                                {/*            borderRadius: "20px",*/}
                                {/*            fontSize: "13px",*/}
                                {/*            fontWeight: "600",*/}
                                {/*            display: "flex",*/}
                                {/*            alignItems: "center",*/}
                                {/*            gap: "6px",*/}
                                {/*            cursor: "pointer",*/}
                                {/*            boxShadow: "0 2px 8px rgba(0,0,0,0.3)",*/}
                                {/*            zIndex: 10*/}
                                {/*        }}*/}
                                {/*    >*/}
                                {/*        <span>📸</span>*/}
                                {/*        <span>{allImages.length}</span>*/}
                                {/*    </div>*/}
                                {/*)}*/}
                            </div>

                            {/* 2 ảnh nhỏ bên phải */}
                            <div style={{
                                display: "grid",
                                gridTemplateRows: "1fr 1fr",
                                gap: 12,
                                height: "479px"
                            }}>
                                {otherImages.slice(0, 2).map((img, idx) => (
                                    <div
                                        key={idx}
                                        style={{
                                            borderRadius: 12,
                                            overflow: "hidden",
                                            height: "100%",
                                            boxShadow: "0 4px 15px rgba(0,0,0,0.08)"
                                        }}
                                    >
                                        <img
                                            src={img.imageUrl}
                                            alt={`Ảnh ${idx + 1}`}
                                            style={{
                                                width: "100%",
                                                height: "100%",
                                                objectFit: "cover"
                                            }}
                                        />
                                        {allImages.length > 1 && (
                                            <div
                                                onClick={() => setIsImageModalOpen(true)}
                                                style={{
                                                    position: "absolute",
                                                    bottom: "12px",
                                                    right: "12px",
                                                    background: "rgba(0, 0, 0, 0.75)",
                                                    color: "white",
                                                    padding: "6px 12px",
                                                    borderRadius: "20px",
                                                    fontSize: "13px",
                                                    fontWeight: "600",
                                                    display: "flex",
                                                    alignItems: "center",
                                                    gap: "6px",
                                                    cursor: "pointer",
                                                    boxShadow: "0 2px 8px rgba(0,0,0,0.3)",
                                                    zIndex: 10
                                                }}
                                            >
                                                <span>📸</span>
                                                <span>{allImages.length}</span>
                                            </div>
                                        )}
                                    </div>
                                ))}

                                {otherImages.length < 2 && (
                                    <div style={{
                                        background: "#f0f0f0",
                                        borderRadius: 12,
                                        display: "flex",
                                        alignItems: "center",
                                        justifyContent: "center",
                                        color: "#999",
                                        height: "100%"
                                    }}>
                                        Ảnh {otherImages.length + 1}
                                    </div>
                                )}
                            </div>
                        </div>
                    </div>

                    {/* Thông tin cửa hàng */}
                    <h1 style={{ fontSize: 32, fontWeight: 700, marginBottom: 12 }}>{store.name}</h1>
                    <p style={{ color: "#555", fontSize: 17, marginBottom: 24 }}>
                        📍 {store.address} • ⭐ {store.averageRating.toFixed(1)} ({store.reviewCount} đánh giá)
                    </p>

                    {/* Phần chọn ngày và khung giờ giữ nguyên */}
                    <h2 style={{ margin: "32px 0 16px" }}>Chọn ngày</h2>
                    <DatePicker
                        value={dayjs(selectedDate)}
                        format="DD/MM/YYYY"
                        disabledDate={(current) => current && (current < dayjs().startOf("day") || current > dayjs().add(14, "day"))}
                        onChange={(d) => d && setSelectedDate(d.format("YYYY-MM-DD"))}
                        allowClear={false}
                        style={{ marginBottom: 24, width: 220 }}
                    />

                    <h2>Khung giờ</h2>
                    {loadingSlots ? <Spin /> : (
                        <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(220px, 1fr))", gap: 16 }}>
                            {slots.map(slot => {
                                const past = isSlotPast(slot);
                                const disabled = past || !slot.isAvailable || !!slot.isDisabledByOverride;
                                return <TimeSlotCard key={slot.id} slot={slot} selectedDate={selectedDate} onBook={() => handleBook(slot)} disabled={disabled} />;
                            })}
                        </div>
                    )}
                </div>
            </div>

            {/* ====================== MODAL ĐẶT LỊCH ====================== */}
            <Modal title="Đặt lịch" open={isModalOpen} onCancel={() => setIsModalOpen(false)} footer={null} width={950}>
                <Form form={form} layout="vertical" onFinish={handleSubmit}>
                    <Form.Item
                        name="petId"
                        label="Thú cưng"
                        rules={[{ required: true, message: "Vui lòng chọn pet" }]}
                    >
                        <Select
                            placeholder="Chọn thú cưng"
                            onChange={(value) => checkPetStatus(value as string)}
                        >
                            {pets.map(p => (
                                <Select.Option key={p.id} value={p.id}>
                                    {p.name}
                                </Select.Option>
                            ))}
                        </Select>
                    </Form.Item>

                    {selectedPetHasActiveBooking && (
                        <Alert
                            message="Không thể đặt lịch"
                            description={`Pet này đang được chăm sóc tại ${activeStoreName || "một cửa hàng khác"}. Vui lòng chờ đơn cũ hoàn thành hoặc hủy trước khi đặt lịch mới.`}
                            type="error"
                            showIcon
                            style={{ marginBottom: 16 }}
                        />
                    )}

                    <Form.Item label="Dịch vụ" required>
                        <Select
                            placeholder="Chọn dịch vụ"
                            value={selectedServiceId}
                            onChange={setSelectedServiceId}
                        >
                            {services.map(s => (
                                <Select.Option key={s.id} value={s.id}>
                                    {s.name} - {s.price.toLocaleString()}đ
                                </Select.Option>
                            ))}
                        </Select>
                    </Form.Item>

                    {selectedService && (
                        <div>
                            <Divider orientation="left">Tùy chọn dịch vụ</Divider>
                            {selectedService.optionGroups.map(group => {
                                const selectedInGroup = selectedOptionIds.filter(id => group.options.some(opt => opt.id === id));
                                return (
                                    <div key={group.id} style={{ marginBottom: 24 }}>
                                        <div style={{ fontWeight: 600, marginBottom: 8 }}>
                                            {group.name}
                                            {group.isRequired && <span style={{ color: "red" }}> *</span>}
                                        </div>
                                        {group.type === 0 ? (
                                            <Radio.Group
                                                value={selectedInGroup[0] || undefined}
                                                onChange={(e) => {
                                                    const newIds = selectedOptionIds.filter(id => !group.options.some(opt => opt.id === id));
                                                    if (e.target.value) newIds.push(e.target.value);
                                                    setSelectedOptionIds(newIds);
                                                }}
                                            >
                                                {group.options.map(opt => (
                                                    <Radio key={opt.id} value={opt.id} style={{ display: "block", marginBottom: 8 }}>
                                                        {opt.name} (+{opt.price.toLocaleString()}đ)
                                                    </Radio>
                                                ))}
                                            </Radio.Group>
                                        ) : (
                                            <Checkbox.Group
                                                value={selectedInGroup}
                                                onChange={(checkedValues) => {
                                                    const newIds = selectedOptionIds.filter(id => !group.options.some(opt => opt.id === id));
                                                    newIds.push(...(checkedValues as string[]));
                                                    setSelectedOptionIds(newIds);
                                                }}
                                            >
                                                {group.options.map(opt => (
                                                    <Checkbox key={opt.id} value={opt.id} style={{ display: "block", marginBottom: 8 }}>
                                                        {opt.name} (+{opt.price.toLocaleString()}đ)
                                                    </Checkbox>
                                                ))}
                                            </Checkbox.Group>
                                        )}
                                    </div>
                                );
                            })}
                        </div>
                    )}

                    <Divider orientation="left">Chọn Voucher</Divider>
                    {/* Phần voucher giữ nguyên như code cũ của bạn */}
                    {platformVouchers.length > 0 && (
                        <div style={{ marginBottom: 24 }}>
                            <h4>Voucher sàn</h4>
                            <div style={{ display: "flex", gap: 12, flexWrap: "wrap" }}>
                                {platformVouchers.map(v => {
                                    const discountText = v.discountType === 1 ? `${v.discountValue}%` : `${v.discountValue.toLocaleString()}đ`;
                                    const minText = v.minOrderValue ? `cho đơn từ ${v.minOrderValue.toLocaleString()}đ` : "";
                                    return (
                                        <div key={v.id} onClick={() => setPlatformVoucherCode(v.code)}
                                            style={{ padding: "14px 16px", border: "2px solid #3b82f6", borderRadius: 8, cursor: "pointer", minWidth: 240, background: platformVoucherCode === v.code ? "#eff6ff" : "white" }}>
                                            <strong>{v.code}</strong><br />
                                            {v.name}<br />
                                            <span style={{ color: "#3b82f6", fontWeight: 600 }}>Giảm {discountText} {minText}</span>
                                        </div>
                                    );
                                })}
                            </div>
                        </div>
                    )}

                    {storeVouchers.length > 0 && (
                        <div style={{ marginBottom: 24 }}>
                            <h4>Voucher của cửa hàng</h4>
                            <div style={{ display: "flex", gap: 12, flexWrap: "wrap" }}>
                                {storeVouchers.map(v => {
                                    const discountText = v.discountType === 1 ? `${v.discountValue}%` : `${v.discountValue.toLocaleString()}đ`;
                                    const minText = v.minOrderValue ? `cho đơn từ ${v.minOrderValue.toLocaleString()}đ` : "";
                                    return (
                                        <div key={v.id} onClick={() => setStoreVoucherCode(v.code)}
                                            style={{ padding: "14px 16px", border: "2px solid #10b981", borderRadius: 8, cursor: "pointer", minWidth: 240, background: storeVoucherCode === v.code ? "#f0fdf4" : "white" }}>
                                            <strong>{v.code}</strong><br />
                                            {v.name}<br />
                                            <span style={{ color: "#10b981", fontWeight: 600 }}>Giảm {discountText} {minText}</span>
                                        </div>
                                    );
                                })}
                            </div>
                        </div>
                    )}

                    <Form.Item label="Hoặc nhập mã voucher sàn">
                        <Input value={platformVoucherCode} onChange={e => setPlatformVoucherCode(e.target.value)} placeholder="WELCOME2025" />
                    </Form.Item>
                    <Form.Item label="Hoặc nhập mã voucher shop">
                        <Input value={storeVoucherCode} onChange={e => setStoreVoucherCode(e.target.value)} placeholder="PETCARE10" />
                    </Form.Item>

                    <div style={{ fontSize: 20, fontWeight: 700, textAlign: "right", margin: "24px 0", color: "#3b82f6" }}>
                        Tổng tiền: {subtotal.toLocaleString()}đ
                    </div>

                    <Form.Item>
                        <Button
                            type="primary"
                            htmlType="submit"
                            block
                            size="large"
                            disabled={selectedPetHasActiveBooking}
                        >
                            Xác nhận đặt lịch
                        </Button>
                    </Form.Item>
                </Form>
            </Modal>

            {/* ====================== MODAL XEM TẤT CẢ ẢNH ====================== */}
            <Modal
                title={`Hình ảnh cửa hàng - ${store?.name}`}
                open={isImageModalOpen}
                onCancel={() => setIsImageModalOpen(false)}
                footer={null}
                width={1100}
                centered
            >
                <div style={{ textAlign: "center", padding: "20px 0" }}>
                    <Image
                        src={allImages[currentImageIndex]?.imageUrl}
                        alt="store"
                        style={{ maxHeight: "520px", objectFit: "contain", borderRadius: 12 }}
                        preview={false}
                    />

                    <div style={{ marginTop: 24, display: "flex", justifyContent: "center", gap: 10, flexWrap: "wrap" }}>
                        {allImages.map((img, idx) => (
                            <div
                                key={idx}
                                onClick={() => setCurrentImageIndex(idx)}
                                style={{
                                    width: 90,
                                    height: 70,
                                    border: currentImageIndex === idx ? "3px solid #86542B" : "2px solid #ddd",
                                    borderRadius: 8,
                                    overflow: "hidden",
                                    cursor: "pointer",
                                    boxShadow: currentImageIndex === idx ? "0 0 0 3px rgba(134, 84, 43, 0.3)" : "none"
                                }}
                            >
                                <img
                                    src={img.imageUrl}
                                    alt=""
                                    style={{ width: "100%", height: "100%", objectFit: "cover" }}
                                />
                            </div>
                        ))}
                    </div>
                </div>
            </Modal>
        </div>
    );
}