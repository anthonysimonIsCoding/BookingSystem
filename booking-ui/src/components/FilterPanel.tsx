import { useEffect, useState } from "react";
import axios from "axios";

interface FilterPanelProps {
    onFilterChange: (filters: any) => void;
    initialFilters?: {
        speciesId?: string;
        categoryIds?: string[];
        minRating?: number;
        minPrice?: number;
        maxPrice?: number;
    };
}

const MAX_PRICE = 10000000;

export default function FilterPanel({ onFilterChange, initialFilters = {} }: FilterPanelProps) {
    // ================= STATE (giữ nguyên) =================
    const [selectedSpecies, setSelectedSpecies] = useState<string>(initialFilters.speciesId ?? "");
    const [selectedCategories, setSelectedCategories] = useState<string[]>(initialFilters.categoryIds ?? []);
    const [minRating, setMinRating] = useState<string>(initialFilters.minRating ? initialFilters.minRating.toString() : "");

    const [priceRange, setPriceRange] = useState<[number, number]>([
        initialFilters.minPrice ?? 20000,
        initialFilters.maxPrice ?? MAX_PRICE
    ]);
    const [minInput, setMinInput] = useState((initialFilters.minPrice ?? 20000).toString());
    const [maxInput, setMaxInput] = useState((initialFilters.maxPrice ?? MAX_PRICE).toString());

    // ================= LOAD DATA (giữ nguyên) =================
    const [species, setSpecies] = useState<any[]>([]);
    const [categories, setCategories] = useState<any[]>([]);

    useEffect(() => {
        const load = async () => {
            const res = await axios.get("http://localhost:5263/api/stores/filters");
            setSpecies(res.data.species || []);
            setCategories(res.data.categories || []);
        };
        load();
    }, []);

    // ================= SAFE SYNC TỪ URL (quan trọng - đã fix warning) =================
    useEffect(() => {
        // Chỉ setState khi giá trị THẬT SỰ khác → tránh cascading renders
        if (initialFilters.speciesId !== selectedSpecies) {
            setSelectedSpecies(initialFilters.speciesId ?? "");
        }
        if (JSON.stringify(initialFilters.categoryIds || []) !== JSON.stringify(selectedCategories)) {
            setSelectedCategories(initialFilters.categoryIds ?? []);
        }
        if ((initialFilters.minRating?.toString() || "") !== minRating) {
            setMinRating(initialFilters.minRating ? initialFilters.minRating.toString() : "");
        }
        if (initialFilters.minPrice !== priceRange[0] || initialFilters.maxPrice !== priceRange[1]) {
            const newRange: [number, number] = [
                initialFilters.minPrice ?? 20000,
                initialFilters.maxPrice ?? MAX_PRICE
            ];
            setPriceRange(newRange);
            setMinInput(newRange[0].toString());
            setMaxInput(newRange[1].toString());
        }
    }, [initialFilters]);   // Chỉ phụ thuộc vào initialFilters

    // ================= DEBOUNCE → GỬI LÊN CHA (giữ nguyên) =================
    useEffect(() => {
        const t = setTimeout(() => {
            onFilterChange({
                speciesId: selectedSpecies || undefined,
                categoryIds: selectedCategories.length ? selectedCategories : undefined,
                minRating: minRating ? Number(minRating) : undefined,
                minPrice: priceRange[0],
                maxPrice: priceRange[1],
            });
        }, 400);

        return () => clearTimeout(t);
    }, [selectedSpecies, selectedCategories, minRating, priceRange, onFilterChange]);

    // ================= Các hàm còn lại GIỮ NGUYÊN (toggleCategory, handleBlur, slider, reset) =================
    const toggleCategory = (id: string) => {
        setSelectedCategories((prev) =>
            prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id]
        );
    };

    const handleMinBlur = () => {
        let min = Number(minInput.replace(/\D/g, ""));
        let max = priceRange[1];
        if (isNaN(min)) min = 0;
        if (min < 0) min = 0;
        if (min > MAX_PRICE) min = MAX_PRICE;
        if (min > max) min = max;
        setPriceRange([min, max]);
        setMinInput(min.toString());
    };

    const handleMaxBlur = () => {
        let max = Number(maxInput.replace(/\D/g, ""));
        let min = priceRange[0];
        if (isNaN(max)) max = min;
        if (max > MAX_PRICE) max = MAX_PRICE;
        if (max < min) max = min;
        setPriceRange([min, max]);
        setMaxInput(max.toString());
    };

    const handleMinSlider = (val: number) => {
        const newMin = Math.min(val, priceRange[1]);
        setPriceRange([newMin, priceRange[1]]);
        setMinInput(newMin.toString());
    };

    const handleMaxSlider = (val: number) => {
        const newMax = Math.max(val, priceRange[0]);
        setPriceRange([priceRange[0], newMax]);
        setMaxInput(newMax.toString());
    };

    const resetFilters = () => {
        setSelectedSpecies("");
        setSelectedCategories([]);
        setMinRating("");
        setPriceRange([20000, MAX_PRICE]);
        setMinInput("20000");
        setMaxInput(MAX_PRICE.toString());
    };

    // ================= JSX (giữ nguyên hệt) =================
    return (
        <div style={container}>
            <h2 style={title}>Bộ lọc</h2>

            {/* LOÀI */}
            <div style={section}>
                <label style={label}>Loài</label>
                <select value={selectedSpecies} onChange={(e) => setSelectedSpecies(e.target.value)} style={input}>
                    <option value="">Tất cả loài</option>
                    {species.map((s) => (
                        <option key={s.id} value={s.id}>{s.name}</option>
                    ))}
                </select>
            </div>

            {/* CATEGORY */}
            <div style={section}>
                <label style={label}>Loại cửa hàng</label>
                {categories.map((c) => (
                    <label key={c.id} style={checkboxRow}>
                        <input
                            type="checkbox"
                            checked={selectedCategories.includes(c.id)}
                            onChange={() => toggleCategory(c.id)}
                        />
                        {c.name}
                    </label>
                ))}
            </div>

            {/* RATING */}
            <div style={section}>
                <label style={label}>Đánh giá tối thiểu</label>
                <select value={minRating} onChange={(e) => setMinRating(e.target.value)} style={input}>
                    <option value="">Tất cả</option>
                    <option value="4">≥ 4 sao</option>
                    <option value="4.5">≥ 4.5 sao</option>
                    <option value="3.5">≥ 3.5 sao</option>
                </select>
            </div>

            {/* PRICE */}
            <div style={{ marginBottom: 32 }}>
                <label style={label}>Khoảng giá (VNĐ)</label>
                <div style={{ position: "relative", height: 50 }}>
                    <div style={track} />
                    <div style={{ ...range, left: `${(priceRange[0] / MAX_PRICE) * 100}%`, right: `${100 - (priceRange[1] / MAX_PRICE) * 100}%` }} />
                    <input type="range" min={0} max={MAX_PRICE} step={50000} value={priceRange[0]} onChange={(e) => handleMinSlider(Number(e.target.value))} style={{ ...rangeInput, zIndex: 3 }} />
                    <input type="range" min={0} max={MAX_PRICE} step={50000} value={priceRange[1]} onChange={(e) => handleMaxSlider(Number(e.target.value))} style={{ ...rangeInput, zIndex: 4 }} />
                </div>
                <div style={priceWrapper}>
                    <input value={minInput} onChange={(e) => setMinInput(e.target.value)} onBlur={handleMinBlur} style={priceInput} />
                    <div style={{ alignSelf: "center", color: "#9ca3af" }}>–</div>
                    <input value={maxInput} onChange={(e) => setMaxInput(e.target.value)} onBlur={handleMaxBlur} style={priceInput} />
                </div>
            </div>

            <button onClick={resetFilters} style={resetBtn}>Xóa bộ lọc</button>
            <style>{sliderCSS}</style>
        </div>
    );
}

// ================= STYLE (giữ nguyên) =================
const container = { background: "white", padding: 20, borderRadius: 12, border: "1px solid #e5e7eb", position: "sticky" as const, top: 80 };
const title = { marginBottom: 24, fontSize: 20 };
const section = { marginBottom: 24 };
const label = { marginBottom: 8, display: "block", fontWeight: 500 };
const input = { width: "100%", padding: "10px", borderRadius: 6, border: "1px solid #d1d5db" };
const checkboxRow = { display: "flex", gap: 8, marginBottom: 8, alignItems: "center" };
const track = { position: "absolute" as const, top: 20, left: 0, right: 0, height: 8, background: "#e5e7eb", borderRadius: 999 };
const range = { position: "absolute" as const, top: 20, height: 8, background: "#f97316", borderRadius: 999 };
const rangeInput = { position: "absolute" as const, width: "100%", height: 50, background: "transparent" };
const priceWrapper = { display: "flex", gap: 12, marginTop: 20 };
const priceInput = { flex: 1, minWidth: 0, padding: "10px", borderRadius: 8, border: "1px solid #e5e7eb", fontWeight: 600, textAlign: "center" as const };
const resetBtn = { width: "100%", padding: "12px", background: "#ef4444", color: "white", border: "none", borderRadius: 8, cursor: "pointer" };
const sliderCSS = `input[type="range"] { -webkit-appearance: none; pointer-events: none; position: absolute; } input[type="range"]::-webkit-slider-thumb { pointer-events: all; -webkit-appearance: none; height: 18px; width: 18px; border-radius: 50%; background: #f97316; border: 3px solid white; box-shadow: 0 0 0 1px #f97316; cursor: pointer; }`;