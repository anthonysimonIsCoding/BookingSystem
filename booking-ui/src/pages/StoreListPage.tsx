import { useEffect, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import axios from "axios";
import FilterPanel from "../components/FilterPanel";
import StoreCard from "../components/StoreCard";
import Navbar from '../components/Navbar';

export default function StoreListPage() {
    const [searchParams, setSearchParams] = useSearchParams();

    const DEFAULT_MIN_PRICE = 20000;
    const DEFAULT_MAX_PRICE = 10000000;

    // ================= ĐỌC TỪ URL (dùng cho UI + FilterPanel) =================
    const urlSort = (searchParams.get("sort") as any) || "recommended";
    const urlSearch = searchParams.get("search") || "";
    const urlSpeciesId = searchParams.get("speciesId") || undefined;
    const urlCategoryIds = searchParams.get("categoryIds")
        ? searchParams.get("categoryIds")!.split(",").map(i => i.trim()).filter(Boolean)
        : (searchParams.get("category") ? [searchParams.get("category")!] : undefined);
    const urlMinRating = searchParams.get("minRating") ? Number(searchParams.get("minRating")) : undefined;
    const urlMinPrice = searchParams.get("minPrice") ? Number(searchParams.get("minPrice")) : DEFAULT_MIN_PRICE;
    const urlMaxPrice = searchParams.get("maxPrice") ? Number(searchParams.get("maxPrice")) : DEFAULT_MAX_PRICE;

    const [stores, setStores] = useState<any[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const [sort, setSort] = useState(urlSort);
    const [searchTerm, setSearchTerm] = useState(urlSearch);
    const [tempSearchTerm, setTempSearchTerm] = useState(urlSearch);

    const [userLocation, setUserLocation] = useState<{ lat: number; lng: number } | null>(null);

    const [filters, setFilters] = useState({
        speciesId: urlSpeciesId,
        categoryIds: urlCategoryIds,
        minRating: urlMinRating,
        minPrice: urlMinPrice,
        maxPrice: urlMaxPrice,
    });

    // ================= SYNC URL → STATE (cho UI FilterPanel) =================
    useEffect(() => {
        setSort(urlSort);
        setSearchTerm(urlSearch);
        setTempSearchTerm(urlSearch);
        setFilters({
            speciesId: urlSpeciesId,
            categoryIds: urlCategoryIds,
            minRating: urlMinRating,
            minPrice: urlMinPrice,
            maxPrice: urlMaxPrice,
        });
    }, [searchParams]);

    // ================= LẤY VỊ TRÍ =================
    useEffect(() => {
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(
                (pos) => setUserLocation({ lat: pos.coords.latitude, lng: pos.coords.longitude }),
                () => { }
            );
        }
    }, []);

    // ================= FETCH API – CHỈ GỌI 1 LẦN (từ URL trực tiếp) =================
    const fetchStores = async () => {
        setLoading(true);
        setError(null);

        try {
            const params: Record<string, any> = {
                sort: searchParams.get("sort") || "recommended",
                radius: 50,
            };

            if (userLocation) {
                params.lat = userLocation.lat;
                params.lng = userLocation.lng;
            }

            // Lấy trực tiếp từ URL (không qua state)
            if (searchParams.get("speciesId")) params.speciesId = searchParams.get("speciesId");
            if (searchParams.get("categoryIds")) {
                params.categoryIds = searchParams.get("categoryIds");
            } else if (searchParams.get("category")) {
                params.categoryIds = searchParams.get("category");
            }
            if (searchParams.get("minRating")) params.minRating = Number(searchParams.get("minRating"));
            if (searchParams.get("minPrice")) params.minPrice = Number(searchParams.get("minPrice"));
            if (searchParams.get("maxPrice")) params.maxPrice = Number(searchParams.get("maxPrice"));
            if (searchParams.get("search")) params.search = searchParams.get("search");

            const res = await axios.get("http://localhost:5263/api/stores", { params });
            setStores(res.data);
        } catch (err: any) {
            console.error(err);
            setError("Không tải được danh sách cửa hàng.");
        } finally {
            setLoading(false);
        }
    };

    // Chỉ trigger khi URL hoặc vị trí thay đổi → không còn gọi 2 lần
    useEffect(() => {
        fetchStores();
    }, [searchParams, userLocation]);

    // ================= STATE → URL (khi user thay đổi filter bên trong) =================
    useEffect(() => {
        const params = new URLSearchParams();

        if (sort !== "recommended") params.set("sort", sort);
        if (searchTerm.trim()) params.set("search", searchTerm.trim());
        if (filters.speciesId) params.set("speciesId", filters.speciesId);
        if (filters.categoryIds?.length) params.set("categoryIds", filters.categoryIds.join(","));
        if (filters.minRating !== undefined) params.set("minRating", filters.minRating.toString());
        if (filters.minPrice !== DEFAULT_MIN_PRICE) params.set("minPrice", filters.minPrice.toString());
        if (filters.maxPrice !== DEFAULT_MAX_PRICE) params.set("maxPrice", filters.maxPrice.toString());

        setSearchParams(params, { replace: true });
    }, [sort, searchTerm, filters, setSearchParams]);

    return (
        <div className="min-h-screen bg-gray-100">
            <Navbar />
            <div style={{ display: "flex", gap: 24, padding: "24px", maxWidth: "1400px", margin: "0 auto" }}>
                <div style={{ width: 280, flexShrink: 0 }}>
                    <FilterPanel onFilterChange={setFilters} initialFilters={filters} />
                </div>

                <div style={{ flex: 1 }}>
                    <h1 style={{ marginBottom: 24, fontSize: 28 }}>Cửa hàng thú cưng</h1>

                    {/* Thanh tìm kiếm + sắp xếp (giữ nguyên code cũ của bạn) */}
                    <div style={{ display: "flex", gap: 16, marginBottom: 32, flexWrap: "wrap", alignItems: "center" }}>
                        <input
                            type="text"
                            placeholder="Tìm tên cửa hàng..."
                            value={tempSearchTerm}
                            onChange={(e) => setTempSearchTerm(e.target.value)}
                            onKeyDown={(e) => { if (e.key === "Enter") setSearchTerm(tempSearchTerm.trim()); }}
                            style={{ flex: "1 1 320px", padding: "12px 16px", borderRadius: 8, border: "1px solid #d1d5db", fontSize: 16 }}
                        />
                        <button onClick={() => setSearchTerm(tempSearchTerm.trim())} style={{ padding: "12px 24px", background: "#3b82f6", color: "white", border: "none", borderRadius: 8, cursor: "pointer", fontSize: 16, minWidth: 120 }}>
                            Tìm kiếm
                        </button>
                        <select value={sort} onChange={(e) => setSort(e.target.value as any)} style={{ padding: "12px 16px", borderRadius: 8, border: "1px solid #d1d5db", background: "white", minWidth: 220, fontSize: 16 }}>
                            <option value="recommended">Khuyến nghị</option>
                            <option value="distance">Gần nhất</option>
                            <option value="price">Giá thấp → cao</option>
                            <option value="rating">Đánh giá cao nhất</option>
                            <option value="booking">Đặt nhiều nhất</option>
                            <option value="category">Phù hợp loại dịch vụ</option>
                        </select>
                    </div>

                    {loading && <div style={{ textAlign: "center", padding: "60px 0", color: "#6b7280" }}>Đang tải cửa hàng...</div>}
                    {error && <div style={{ textAlign: "center", padding: "40px 0", color: "#dc2626" }}>{error}</div>}
                    {!loading && !error && stores.length === 0 && <div style={{ textAlign: "center", padding: "60px 0", color: "#6b7280" }}>Không tìm thấy cửa hàng nào phù hợp</div>}

                    {!loading && !error && stores.length > 0 && (
                        <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(280px, 1fr))", gap: 24 }}>
                            {stores.map((store) => (
                                <Link key={store.id} to={`/store/${store.id}`} style={{ textDecoration: "none", color: "inherit" }}>
                                    <StoreCard store={store} />
                                </Link>
                            ))}
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}