// src/components/vendor/BasicInfo.tsx
import React from 'react';

interface BasicInfoProps {
    name: string;
    selectedCategories: string[];
    selectedSpecies: string[];
    availableCategories: { id: string; name: string }[];
    availableSpecies: { id: string; name: string }[];
    categorySearch: string;
    speciesSearch: string;
    onNameChange: (name: string) => void;
    onCategorySearchChange: (value: string) => void;
    onSpeciesSearchChange: (value: string) => void;
    onAddCategory: (catId: string) => void;
    onRemoveCategory: (catId: string) => void;
    onAddSpecies: (spId: string) => void;
    onRemoveSpecies: (spId: string) => void;
    showCategoryDropdown: boolean;
    showSpeciesDropdown: boolean;
    onToggleCategoryDropdown: () => void;
    onToggleSpeciesDropdown: () => void;
}

export default function BasicInfo({
    name,
    selectedCategories,
    selectedSpecies,
    availableCategories,
    availableSpecies,
    categorySearch,
    speciesSearch,
    onNameChange,
    onCategorySearchChange,
    onSpeciesSearchChange,
    onAddCategory,
    onRemoveCategory,
    onAddSpecies,
    onRemoveSpecies,
    showCategoryDropdown,
    showSpeciesDropdown,
    onToggleCategoryDropdown,
    onToggleSpeciesDropdown,
}: BasicInfoProps) {

    const filteredCategories = categorySearch.trim() === ""
        ? availableCategories
        : availableCategories.filter(cat => cat.name.toLowerCase().includes(categorySearch.toLowerCase()));

    const filteredSpecies = speciesSearch.trim() === ""
        ? availableSpecies
        : availableSpecies.filter(sp => sp.name.toLowerCase().includes(speciesSearch.toLowerCase()));

    return (
        <div>
            {/* Tên cửa hàng */}
            <div style={{ marginBottom: "24px" }}>
                <label style={{ display: "block", marginBottom: "8px", fontWeight: "600" }}>Tên cửa hàng</label>
                <input
                    type="text"
                    value={name}
                    onChange={(e) => onNameChange(e.target.value)}
                    style={{ width: "100%", padding: "14px", borderRadius: "10px", border: "1px solid #ddd" }}
                    placeholder="Nhập tên cửa hàng"
                />
            </div>

            {/* Danh mục cửa hàng */}
            <div style={{ marginBottom: "30px" }}>
                <label style={{ display: "block", marginBottom: "10px", fontWeight: "600" }}>Danh mục cửa hàng</label>
                <div
                    onClick={onToggleCategoryDropdown}
                    style={{ border: "1px solid #ddd", borderRadius: "10px", padding: "10px", minHeight: "52px", display: "flex", flexWrap: "wrap", gap: "8px", cursor: "pointer" }}
                >
                    {selectedCategories.map(catId => {
                        const cat = availableCategories.find(c => c.id === catId);
                        return (
                            <div key={catId} style={{ background: "#f3e8d9", padding: "6px 12px", borderRadius: "20px", display: "flex", alignItems: "center", gap: "6px" }}>
                                <span>{cat?.name}</span>
                                <button onClick={(e) => { e.stopPropagation(); onRemoveCategory(catId); }} style={{ background: "none", border: "none", color: "#e74c3c", fontSize: "18px" }}>×</button>
                            </div>
                        );
                    })}
                    <input
                        type="text"
                        value={categorySearch}
                        onChange={(e) => onCategorySearchChange(e.target.value)}
                        onFocus={onToggleCategoryDropdown}
                        placeholder="Nhấn vào đây để chọn danh mục..."
                        style={{ border: "none", outline: "none", flex: 1, minWidth: "200px" }}
                    />
                </div>

                {showCategoryDropdown && (
                    <div style={{ border: "1px solid #ddd", borderRadius: "8px", marginTop: "4px", maxHeight: "280px", overflowY: "auto", background: "white" }}>
                        {filteredCategories.map(cat => (
                            <div
                                key={cat.id}
                                onClick={() => onAddCategory(cat.id)}
                                style={{
                                    padding: "12px 16px",
                                    cursor: "pointer",
                                    borderBottom: "1px solid #eee",
                                    background: selectedCategories.includes(cat.id) ? "#e6f0e6" : "transparent"
                                }}
                            >
                                {cat.name} {selectedCategories.includes(cat.id) && "✓"}
                            </div>
                        ))}
                    </div>
                )}
            </div>

            {/* Chủng loài */}
            <div style={{ marginBottom: "40px" }}>
                <label style={{ display: "block", marginBottom: "10px", fontWeight: "600" }}>Chủng loài nhận chăm sóc</label>
                <div
                    onClick={onToggleSpeciesDropdown}
                    style={{ border: "1px solid #ddd", borderRadius: "10px", padding: "10px", minHeight: "52px", display: "flex", flexWrap: "wrap", gap: "8px", cursor: "pointer" }}
                >
                    {selectedSpecies.map(spId => {
                        const sp = availableSpecies.find(s => s.id === spId);
                        return (
                            <div key={spId} style={{ background: "#f3e8d9", padding: "6px 12px", borderRadius: "20px", display: "flex", alignItems: "center", gap: "6px" }}>
                                <span>{sp?.name}</span>
                                <button onClick={(e) => { e.stopPropagation(); onRemoveSpecies(spId); }} style={{ background: "none", border: "none", color: "#e74c3c", fontSize: "18px" }}>×</button>
                            </div>
                        );
                    })}
                    <input
                        type="text"
                        value={speciesSearch}
                        onChange={(e) => onSpeciesSearchChange(e.target.value)}
                        onFocus={onToggleSpeciesDropdown}
                        placeholder="Nhấn vào đây để chọn chủng loài..."
                        style={{ border: "none", outline: "none", flex: 1, minWidth: "200px" }}
                    />
                </div>

                {showSpeciesDropdown && (
                    <div style={{ border: "1px solid #ddd", borderRadius: "8px", marginTop: "4px", maxHeight: "280px", overflowY: "auto", background: "white" }}>
                        {filteredSpecies.map(sp => (
                            <div
                                key={sp.id}
                                onClick={() => onAddSpecies(sp.id)}
                                style={{
                                    padding: "12px 16px",
                                    cursor: "pointer",
                                    borderBottom: "1px solid #eee",
                                    background: selectedSpecies.includes(sp.id) ? "#e6f0e6" : "transparent"
                                }}
                            >
                                {sp.name} {selectedSpecies.includes(sp.id) && "✓"}
                            </div>
                        ))}
                    </div>
                )}
            </div>
        </div>
    );
}