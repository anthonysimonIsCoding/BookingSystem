// src/components/admin/SpeciesBreedTab.tsx
import { useState, useEffect } from "react";
import axiosInstance from "../../utils/axiosInstance";

export default function SpeciesBreedTab() {
    const [speciesList, setSpeciesList] = useState<any[]>([]);
    const [selectedSpeciesId, setSelectedSpeciesId] = useState<string>("");
    const [breeds, setBreeds] = useState<any[]>([]);
    const [loading, setLoading] = useState(true);

    // Modal Species
    const [showSpeciesModal, setShowSpeciesModal] = useState(false);
    const [editingSpecies, setEditingSpecies] = useState<any>(null);
    const [speciesForm, setSpeciesForm] = useState({ name: "" });

    // Modal Breed
    const [showBreedModal, setShowBreedModal] = useState(false);
    const [editingBreed, setEditingBreed] = useState<any>(null);
    const [breedForm, setBreedForm] = useState({ name: "", speciesId: "" });

    useEffect(() => {
        loadSpecies();
    }, []);

    useEffect(() => {
        if (selectedSpeciesId) {
            loadBreeds(selectedSpeciesId);
        } else {
            setBreeds([]);
        }
    }, [selectedSpeciesId]);

    const loadSpecies = async () => {
        setLoading(true);
        try {
            const res = await axiosInstance.get("/admin/masterdata/species");
            setSpeciesList(res.data);
        } catch (err) {
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    const loadBreeds = async (speciesId: string) => {
        try {
            const res = await axiosInstance.get("/admin/masterdata/breeds");
            // Lọc giống theo loài được chọn
            const filtered = res.data.filter((b: any) => b.species?.id === speciesId);
            setBreeds(filtered);
        } catch (err) {
            console.error(err);
        }
    };

    // ==================== SPECIES CRUD ====================
    const openSpeciesModal = (item?: any) => {
        if (item) {
            setEditingSpecies(item);
            setSpeciesForm({ name: item.name });
        } else {
            setEditingSpecies(null);
            setSpeciesForm({ name: "" });
        }
        setShowSpeciesModal(true);
    };

    const saveSpecies = async () => {
        if (!speciesForm.name.trim()) return alert("Vui lòng nhập tên loài");
        try {
            if (editingSpecies) {
                await axiosInstance.put(`/admin/masterdata/species/${editingSpecies.id}`, speciesForm);
            } else {
                await axiosInstance.post("/admin/masterdata/species", speciesForm);
            }
            alert("Lưu thành công!");
            setShowSpeciesModal(false);
            loadSpecies();
        } catch (err) {
            alert("Lưu thất bại");
        }
    };

    const deleteSpecies = async (id: string) => {
        if (!window.confirm("Xóa loài này sẽ xóa luôn tất cả giống thuộc loài?")) return;
        try {
            await axiosInstance.delete(`/admin/masterdata/species/${id}`);
            loadSpecies();
            if (selectedSpeciesId === id) setSelectedSpeciesId("");
        } catch (err) {
            alert("Xóa thất bại");
        }
    };

    // ==================== BREED CRUD ====================
    const openBreedModal = (item?: any) => {
        if (item) {
            setEditingBreed(item);
            setBreedForm({ name: item.name, speciesId: item.species?.id || "" });
        } else {
            setEditingBreed(null);
            setBreedForm({ name: "", speciesId: selectedSpeciesId });
        }
        setShowBreedModal(true);
    };

    const saveBreed = async () => {
        if (!breedForm.name.trim() || !breedForm.speciesId) {
            return alert("Vui lòng nhập tên giống và chọn loài");
        }
        try {
            if (editingBreed) {
                await axiosInstance.put(`/admin/masterdata/breeds/${editingBreed.id}`, breedForm);
            } else {
                await axiosInstance.post("/admin/masterdata/breeds", breedForm);
            }
            alert("Lưu thành công!");
            setShowBreedModal(false);
            if (selectedSpeciesId) loadBreeds(selectedSpeciesId);
        } catch (err) {
            alert("Lưu thất bại");
        }
    };

    const deleteBreed = async (id: string) => {
        if (!window.confirm("Xóa giống này?")) return;
        try {
            await axiosInstance.delete(`/admin/masterdata/breeds/${id}`);
            if (selectedSpeciesId) loadBreeds(selectedSpeciesId);
        } catch (err) {
            alert("Xóa thất bại");
        }
    };

    return (
        <div style={{ background: "white", padding: "32px", borderRadius: "20px" }}>
            <h2>Quản lý Loài & Giống thú cưng</h2>

            <div style={{ display: "flex", gap: "30px", marginTop: "30px" }}>
                {/* Cột Loài */}
                <div style={{ flex: "1" }}>
                    <div style={{ display: "flex", justifyContent: "space-between", marginBottom: "16px" }}>
                        <h3>Loài thú cưng</h3>
                        <button onClick={() => openSpeciesModal()} style={{ padding: "8px 16px", background: "#86542B", color: "white", border: "none", borderRadius: "8px" }}>
                            + Thêm loài
                        </button>
                    </div>

                    <div style={{ display: "flex", flexDirection: "column", gap: "8px" }}>
                        {speciesList.map(s => (
                            <div
                                key={s.id}
                                onClick={() => setSelectedSpeciesId(s.id)}
                                style={{
                                    padding: "14px",
                                    border: selectedSpeciesId === s.id ? "2px solid #86542B" : "1px solid #ddd",
                                    borderRadius: "10px",
                                    cursor: "pointer",
                                    background: selectedSpeciesId === s.id ? "#fff4e6" : "white"
                                }}
                            >
                                <strong>{s.name}</strong>
                                <div style={{ marginTop: "8px", display: "flex", gap: "8px" }}>
                                    <button onClick={(e) => { e.stopPropagation(); openSpeciesModal(s); }} style={{ flex: 1, padding: "6px", background: "#3b82f6", color: "white", border: "none", borderRadius: "6px" }}>Sửa</button>
                                    <button onClick={(e) => { e.stopPropagation(); deleteSpecies(s.id); }} style={{ flex: 1, padding: "6px", background: "#ef4444", color: "white", border: "none", borderRadius: "6px" }}>Xóa</button>
                                </div>
                            </div>
                        ))}
                    </div>
                </div>

                {/* Cột Giống */}
                <div style={{ flex: "1" }}>
                    <div style={{ display: "flex", justifyContent: "space-between", marginBottom: "16px" }}>
                        <h3>
                            Giống thuộc loài:{" "}
                            <span style={{ color: "#86542B" }}>
                                {speciesList.find(s => s.id === selectedSpeciesId)?.name || "Chưa chọn loài"}
                            </span>
                        </h3>
                        <button
                            onClick={() => openBreedModal()}
                            disabled={!selectedSpeciesId}
                            style={{
                                padding: "8px 16px",
                                background: selectedSpeciesId ? "#86542B" : "#ccc",
                                color: "white",
                                border: "none",
                                borderRadius: "8px"
                            }}
                        >
                            + Thêm giống
                        </button>
                    </div>

                    <div style={{ display: "flex", flexDirection: "column", gap: "8px" }}>
                        {breeds.map(b => (
                            <div key={b.id} style={{ padding: "14px", border: "1px solid #ddd", borderRadius: "10px", background: "white" }}>
                                <strong>{b.name}</strong>
                                <div style={{ marginTop: "10px", display: "flex", gap: "8px" }}>
                                    <button onClick={() => openBreedModal(b)} style={{ flex: 1, padding: "6px", background: "#3b82f6", color: "white", border: "none", borderRadius: "6px" }}>Sửa</button>
                                    <button onClick={() => deleteBreed(b.id)} style={{ flex: 1, padding: "6px", background: "#ef4444", color: "white", border: "none", borderRadius: "6px" }}>Xóa</button>
                                </div>
                            </div>
                        ))}
                        {selectedSpeciesId && breeds.length === 0 && (
                            <p style={{ color: "#888", textAlign: "center", padding: "40px" }}>Loài này chưa có giống nào.</p>
                        )}
                    </div>
                </div>
            </div>

            {/* Modal Species */}
            {showSpeciesModal && (
                <div style={{ position: "fixed", inset: 0, background: "rgba(0,0,0,0.7)", display: "flex", alignItems: "center", justifyContent: "center", zIndex: 2000 }}>
                    <div style={{ background: "white", width: "450px", borderRadius: "16px", padding: "24px" }}>
                        <h3>{editingSpecies ? "Sửa loài" : "Thêm loài mới"}</h3>
                        <input
                            value={speciesForm.name}
                            onChange={e => setSpeciesForm({ name: e.target.value })}
                            placeholder="Tên loài"
                            style={{ width: "100%", padding: "12px", margin: "12px 0", borderRadius: "8px", border: "1px solid #ddd" }}
                        />
                        <div style={{ marginTop: "20px", display: "flex", gap: "12px" }}>
                            <button onClick={saveSpecies} style={{ flex: 1, padding: "12px", background: "#86542B", color: "white", border: "none", borderRadius: "8px" }}>Lưu</button>
                            <button onClick={() => setShowSpeciesModal(false)} style={{ flex: 1, padding: "12px", background: "#666", color: "white", border: "none", borderRadius: "8px" }}>Hủy</button>
                        </div>
                    </div>
                </div>
            )}

            {/* Modal Breed */}
            {showBreedModal && (
                <div style={{ position: "fixed", inset: 0, background: "rgba(0,0,0,0.7)", display: "flex", alignItems: "center", justifyContent: "center", zIndex: 2000 }}>
                    <div style={{ background: "white", width: "450px", borderRadius: "16px", padding: "24px" }}>
                        <h3>{editingBreed ? "Sửa giống" : "Thêm giống mới"}</h3>
                        <select
                            value={breedForm.speciesId}
                            onChange={e => setBreedForm({ ...breedForm, speciesId: e.target.value })}
                            style={{ width: "100%", padding: "12px", margin: "12px 0", borderRadius: "8px", border: "1px solid #ddd" }}
                        >
                            <option value="">Chọn loài</option>
                            {speciesList.map(s => (
                                <option key={s.id} value={s.id}>{s.name}</option>
                            ))}
                        </select>
                        <input
                            value={breedForm.name}
                            onChange={e => setBreedForm({ ...breedForm, name: e.target.value })}
                            placeholder="Tên giống"
                            style={{ width: "100%", padding: "12px", margin: "12px 0", borderRadius: "8px", border: "1px solid #ddd" }}
                        />
                        <div style={{ marginTop: "20px", display: "flex", gap: "12px" }}>
                            <button onClick={saveBreed} style={{ flex: 1, padding: "12px", background: "#86542B", color: "white", border: "none", borderRadius: "8px" }}>Lưu</button>
                            <button onClick={() => setShowBreedModal(false)} style={{ flex: 1, padding: "12px", background: "#666", color: "white", border: "none", borderRadius: "8px" }}>Hủy</button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}