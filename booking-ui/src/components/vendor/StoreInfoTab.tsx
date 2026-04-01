// src/components/vendor/StoreInfoTab.tsx
import { useState, useEffect, useRef } from "react";
import axiosInstance from '../../utils/axiosInstance';
import BasicInfo from './StoreManagement/BasicInfo';
import AddressTab from './StoreManagement/AddressTab';
import ImagesTab from './StoreManagement/ImagesTab';

const MAP_TILE_KEY = "bQvmssxbjRm6JeS82OOQGtLp3SZ7iDUP1BpAAQfa";
const API_KEY = "qA25nsR6quBvGmC2mykSw4Rs2249ZHd8Z3DOExix";

interface ImageItem {
    id?: string;
    imageUrl: string;
    isThumbnail: boolean;
    order: number;
    isNew?: boolean;
    file?: File;
}

interface AvailableCategory {
    id: string;
    name: string;
    description?: string;
}

interface AvailableSpecies {
    id: string;
    name: string;
}

interface StoreData {
    id: string;
    name: string;
    address: string;
    latitude?: number;
    longitude?: number;
    averageRating: number;
    reviewCount: number;
    images: any[];
    categories: { categoryId: string; name: string }[];
    species: { speciesId: string; name: string }[];
}

export default function StoreInfoTab() {
    const [storeInfo, setStoreInfo] = useState<StoreData | null>(null);
    const [editMode, setEditMode] = useState(false);
    const [saving, setSaving] = useState(false);
    const [loading, setLoading] = useState(true);

    const [formData, setFormData] = useState({
        name: "",
        address: "",
        latitude: 10.8231,
        longitude: 106.6297
    });

    const [images, setImages] = useState<ImageItem[]>([]);

    const [selectedCategories, setSelectedCategories] = useState<string[]>([]);
    const [selectedSpecies, setSelectedSpecies] = useState<string[]>([]);
    const [availableCategories, setAvailableCategories] = useState<AvailableCategory[]>([]);
    const [availableSpecies, setAvailableSpecies] = useState<AvailableSpecies[]>([]);

    const [categorySearch, setCategorySearch] = useState("");
    const [speciesSearch, setSpeciesSearch] = useState("");

    const [showCategoryDropdown, setShowCategoryDropdown] = useState(false);
    const [showSpeciesDropdown, setShowSpeciesDropdown] = useState(false);

    useEffect(() => {
        loadAllData();
    }, []);

    const loadAllData = async () => {
        setLoading(true);
        try {
            const [storeRes, availableRes] = await Promise.all([
                axiosInstance.get('/vendor/profile'),
                axiosInstance.get('/vendor/profile/available-categories')
            ]);

            const data: StoreData = storeRes.data;
            setStoreInfo(data);

            setFormData({
                name: data.name || "",
                address: data.address || "",
                latitude: data.latitude || 10.8231,
                longitude: data.longitude || 106.6297
            });

            const sortedImages = (data.images || []).sort((a: any, b: any) => a.order - b.order);
            setImages(sortedImages);

            setSelectedCategories(data.categories?.map((c: any) => c.categoryId.toString()) || []);
            setSelectedSpecies(data.species?.map((s: any) => s.speciesId.toString()) || []);

            setAvailableCategories(availableRes.data.categories || []);
            setAvailableSpecies(availableRes.data.species || []);
        } catch (err: any) {
            console.error(err);
            alert("Không thể tải thông tin cửa hàng.");
        } finally {
            setLoading(false);
        }
    };

    const handleSave = async () => {
        if (!formData.name.trim() || !formData.address.trim()) {
            alert("Vui lòng nhập tên và địa chỉ cửa hàng");
            return;
        }

        setSaving(true);
        try {
            await axiosInstance.put('/vendor/profile', {
                name: formData.name,
                address: formData.address,
                latitude: formData.latitude,
                longitude: formData.longitude
            });

            let uploadedUrls: string[] = [];
            const filesToUpload = images.filter(img => img.isNew && img.file);
            if (filesToUpload.length > 0) {
                const formDataImg = new FormData();
                filesToUpload.forEach(item => formDataImg.append("files", item.file!));
                const uploadRes = await axiosInstance.post('/vendor/profile/images', formDataImg, {
                    headers: { "Content-Type": "multipart/form-data" }
                });
                uploadedUrls = uploadRes.data.urls || [];
            }

            let finalImages: any[] = [];
            let urlIndex = 0;
            images.forEach(img => {
                finalImages.push({
                    imageUrl: img.isNew && img.file ? uploadedUrls[urlIndex++] : img.imageUrl,
                    isThumbnail: img.isThumbnail,
                    order: finalImages.length
                });
            });

            if (finalImages.length > 0 && !finalImages.some(i => i.isThumbnail)) {
                finalImages[0].isThumbnail = true;
            }

            await axiosInstance.post('/vendor/profile/images/save', { images: finalImages });
            await axiosInstance.put('/vendor/profile/categories', { categoryIds: selectedCategories });
            await axiosInstance.put('/vendor/profile/species', { speciesIds: selectedSpecies });

            alert("✅ Cập nhật thông tin cửa hàng thành công!");
            setEditMode(false);
            loadAllData();
        } catch (err: any) {
            console.error(err);
            alert(err.response?.data?.message || "Cập nhật thất bại");
        } finally {
            setSaving(false);
        }
    };

    // Helper functions cho BasicInfo
    const addCategory = (catId: string) => {
        if (!selectedCategories.includes(catId)) {
            setSelectedCategories([...selectedCategories, catId]);
        }
    };

    const removeCategory = (catId: string) => {
        setSelectedCategories(prev => prev.filter(id => id !== catId));
    };

    const addSpecies = (spId: string) => {
        if (!selectedSpecies.includes(spId)) {
            setSelectedSpecies([...selectedSpecies, spId]);
        }
    };

    const removeSpecies = (spId: string) => {
        setSelectedSpecies(prev => prev.filter(id => id !== spId));
    };

    const renderViewMode = () => {
        if (!storeInfo) return <div>Không tìm thấy thông tin cửa hàng</div>;
        const thumbnail = storeInfo.images?.find((img: any) => img.isThumbnail) || storeInfo.images?.[0];

        return (
            <div style={{ background: "white", borderRadius: "20px", overflow: "hidden", boxShadow: "0 8px 30px rgba(0,0,0,0.08)" }}>
                {thumbnail && (
                    <div style={{ height: "280px", position: "relative" }}>
                        <img src={thumbnail.imageUrl} alt={storeInfo.name} style={{ width: "100%", height: "100%", objectFit: "cover" }} />
                        <div style={{ position: "absolute", inset: 0, background: "linear-gradient(to top, rgba(0,0,0,0.65), rgba(0,0,0,0.2))" }} />
                        <div style={{ position: "absolute", bottom: "24px", left: "32px", color: "white" }}>
                            <h1 style={{ fontSize: "32px", fontWeight: "700", margin: "0 0 8px 0" }}>{storeInfo.name}</h1>
                            <div style={{ display: "flex", alignItems: "center", gap: "8px", fontSize: "16px" }}>
                                📍 {storeInfo.address}
                            </div>
                        </div>
                        <div style={{ position: "absolute", top: "20px", right: "20px", background: "#10b981", color: "white", padding: "6px 14px", borderRadius: "20px", fontSize: "13px", fontWeight: "600" }}>
                            Ảnh chính
                        </div>
                    </div>
                )}

                <div style={{ padding: "32px" }}>
                    <div style={{ display: "flex", alignItems: "center", gap: "16px", marginBottom: "28px" }}>
                        <div style={{ fontSize: "42px", fontWeight: "700", color: "#86542B" }}>
                            {storeInfo.averageRating.toFixed(1)}
                        </div>
                        <div>
                            <div style={{ fontSize: "26px", color: "#facc15" }}>
                                {"★".repeat(Math.floor(storeInfo.averageRating))}
                            </div>
                            <div style={{ color: "#666", fontSize: "15px" }}>
                                Dựa trên {storeInfo.reviewCount} đánh giá
                            </div>
                        </div>
                    </div>

                    {storeInfo.categories?.length > 0 && (
                        <div style={{ marginBottom: "28px" }}>
                            <div style={{ fontWeight: "600", color: "#444", marginBottom: "12px" }}>🏷️ Danh mục cửa hàng</div>
                            <div style={{ display: "flex", flexWrap: "wrap", gap: "10px" }}>
                                {storeInfo.categories.map((cat: any, idx: number) => (
                                    <span key={idx} style={{ background: "#fff4e6", color: "#b45309", padding: "10px 18px", borderRadius: "30px", fontSize: "15px", fontWeight: "500" }}>
                                        {cat.name}
                                    </span>
                                ))}
                            </div>
                        </div>
                    )}

                    {storeInfo.species?.length > 0 && (
                        <div style={{ marginBottom: "32px" }}>
                            <div style={{ fontWeight: "600", color: "#444", marginBottom: "12px" }}>🐾 Chủng loài nhận chăm sóc</div>
                            <div style={{ display: "flex", flexWrap: "wrap", gap: "10px" }}>
                                {storeInfo.species.map((sp: any, idx: number) => (
                                    <span key={idx} style={{ background: "#e6f4ea", color: "#166534", padding: "10px 18px", borderRadius: "30px", fontSize: "15px", fontWeight: "500" }}>
                                        {sp.name}
                                    </span>
                                ))}
                            </div>
                        </div>
                    )}

                    {storeInfo.images && storeInfo.images.length > 0 && (
                        <div>
                            <div style={{ fontWeight: "600", color: "#444", marginBottom: "16px" }}>
                                Hình ảnh cửa hàng ({storeInfo.images.length})
                            </div>
                            <div style={{ display: "flex", gap: "12px", flexWrap: "wrap" }}>
                                {storeInfo.images.sort((a: any, b: any) => a.order - b.order).map((img: any, idx: number) => (
                                    <div key={idx} style={{ position: "relative", width: "165px" }}>
                                        <img src={img.imageUrl} style={{ width: "100%", height: "120px", objectFit: "cover", borderRadius: "12px" }} />
                                        {img.isThumbnail && (
                                            <div style={{ position: "absolute", top: "8px", left: "8px", background: "#10b981", color: "white", padding: "4px 10px", borderRadius: "6px", fontSize: "12px", fontWeight: "600" }}>
                                                Ảnh chính
                                            </div>
                                        )}
                                    </div>
                                ))}
                            </div>
                        </div>
                    )}
                </div>
            </div>
        );
    };

    if (loading) {
        return <div style={{ padding: "100px", textAlign: "center", fontSize: "18px" }}>Đang tải thông tin cửa hàng...</div>;
    }

    return (
        <div>
            {!editMode ? (
                renderViewMode()
            ) : (
                <div style={{ background: "white", padding: "32px", borderRadius: "20px", boxShadow: "0 4px 20px rgba(0,0,0,0.06)" }}>
                    <BasicInfo
                        name={formData.name}
                        selectedCategories={selectedCategories}
                        selectedSpecies={selectedSpecies}
                        availableCategories={availableCategories}
                        availableSpecies={availableSpecies}
                        categorySearch={categorySearch}
                        speciesSearch={speciesSearch}
                        onNameChange={(name) => setFormData(p => ({ ...p, name }))}
                        onCategorySearchChange={setCategorySearch}
                        onSpeciesSearchChange={setSpeciesSearch}
                        onAddCategory={addCategory}
                        onRemoveCategory={removeCategory}
                        onAddSpecies={addSpecies}
                        onRemoveSpecies={removeSpecies}
                        showCategoryDropdown={showCategoryDropdown}
                        showSpeciesDropdown={showSpeciesDropdown}
                        onToggleCategoryDropdown={() => setShowCategoryDropdown(!showCategoryDropdown)}
                        onToggleSpeciesDropdown={() => setShowSpeciesDropdown(!showSpeciesDropdown)}
                    />

                    <AddressTab
                        address={formData.address}
                        latitude={formData.latitude}
                        longitude={formData.longitude}
                        onAddressChange={(addr) => setFormData(p => ({ ...p, address: addr }))}
                        onLocationChange={(lat, lng) => setFormData(p => ({ ...p, latitude: lat, longitude: lng }))}
                    />

                    <ImagesTab images={images} onImagesChange={setImages} />

                    <div style={{ display: "flex", gap: "12px", marginTop: "40px" }}>
                        <button onClick={handleSave} disabled={saving} style={{ flex: 1, padding: "16px", background: "#86542B", color: "white", border: "none", borderRadius: "12px", fontWeight: "600" }}>
                            {saving ? "Đang lưu..." : "Cập nhật tất cả"}
                        </button>
                        <button onClick={() => setEditMode(false)} style={{ flex: 1, padding: "16px", background: "#666", color: "white", border: "none", borderRadius: "12px" }}>
                            Hủy
                        </button>
                    </div>
                </div>
            )}

            {!editMode && (
                <div style={{ textAlign: "center", marginTop: "30px" }}>
                    <button
                        onClick={() => setEditMode(true)}
                        style={{ padding: "14px 36px", background: "#86542B", color: "white", border: "none", borderRadius: "12px", cursor: "pointer", fontSize: "16px", fontWeight: "600" }}
                    >
                        ✏️ Chỉnh sửa thông tin cửa hàng
                    </button>
                </div>
            )}
        </div>
    );
}