// src/components/vendor/AddressTab.tsx
import React, { useEffect, useRef, useState } from 'react';
import maplibregl from "maplibre-gl";
import "maplibre-gl/dist/maplibre-gl.css";
import axiosInstance from '../../../utils/axiosInstance';

interface AddressTabProps {
    address: string;
    latitude: number;
    longitude: number;
    onAddressChange: (address: string) => void;
    onLocationChange: (lat: number, lng: number) => void;
}

export default function AddressTab({
    address,
    latitude,
    longitude,
    onAddressChange,
    onLocationChange
}: AddressTabProps) {

    const mapRef = useRef<HTMLDivElement>(null);
    const mapInstance = useRef<any>(null);
    const [suggestions, setSuggestions] = useState<any[]>([]);
    const [showSuggestions, setShowSuggestions] = useState(false);

    // ==================== GỌI QUA BACKEND (không còn key nào ở frontend) ====================

    const fetchAutoComplete = async (query: string) => {
        try {
            const res = await axiosInstance.get(`/vendor/profile/map/autocomplete?input=${encodeURIComponent(query)}`);
            setSuggestions(res.data.predictions || []);
            setShowSuggestions(true);
        } catch (err) {
            console.error("Lỗi autocomplete:", err);
        }
    };

    const fetchPlaceDetails = async (placeId: string) => {
        try {
            const res = await axiosInstance.get(`/vendor/profile/map/detail?place_id=${placeId}`);
            const data = res.data;

            if (data.result?.geometry?.location) {
                const { lat, lng } = data.result.geometry.location;
                onLocationChange(lat, lng);
                if (mapInstance.current) {
                    mapInstance.current.flyTo({ center: [lng, lat], zoom: 16 });
                }
            }
        } catch (err) {
            console.error("Lỗi lấy chi tiết place:", err);
        }
    };

    const reverseGeocode = async (lat: number, lng: number) => {
        try {
            const res = await axiosInstance.get(`/vendor/profile/map/reverse?lat=${lat}&lng=${lng}`);
            const data = res.data;
            if (data.results && data.results.length > 0) {
                onAddressChange(data.results[0].formatted_address || "");
            }
        } catch (err) {
            console.error("Lỗi reverse geocode:", err);
        }
    };

    // Lấy Map Style từ backend (để lấy Tile Key)
    const loadMapStyle = async (): Promise<string | null> => {
        try {
            const res = await axiosInstance.get('/vendor/profile/map/style');
            return res.data.styleUrl;
        } catch (err) {
            console.error("Không lấy được map style từ backend:", err);
            return null;
        }
    };

    // ==================== KHỞI TẠO BẢN ĐỒ ====================
    useEffect(() => {
        if (!mapRef.current) return;

        const initMap = async () => {
            const styleUrl = await loadMapStyle();
            if (!styleUrl) {
                console.error("Không thể load map style");
                return;
            }

            const map = new maplibregl.Map({
                container: mapRef.current!,
                style: styleUrl,           // Style đã gắn TileKey từ backend
                center: [longitude, latitude],
                zoom: 15
            });

            mapInstance.current = map;

            let isUserDragging = false;

            map.on('dragstart', () => {
                isUserDragging = true;
            });

            map.on('moveend', () => {
                if (!isUserDragging) return;

                const center = map.getCenter();
                const newLat = parseFloat(center.lat.toFixed(6));
                const newLng = parseFloat(center.lng.toFixed(6));

                onLocationChange(newLat, newLng);
                reverseGeocode(newLat, newLng);

                isUserDragging = false;
            });
        };

        initMap();

        // Cleanup khi component unmount
        return () => {
            if (mapInstance.current) {
                mapInstance.current.remove();
            }
        };
    }, [latitude, longitude]);   // Re-init nếu lat/lng thay đổi

    // ==================== XỬ LÝ ĐỊA CHỈ ====================
    const handleAddressChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const value = e.target.value;
        onAddressChange(value);

        if (value.length > 2) {
            fetchAutoComplete(value);
        } else {
            setSuggestions([]);
            setShowSuggestions(false);
        }
    };

    const selectSuggestion = (prediction: any) => {
        onAddressChange(prediction.description);
        setShowSuggestions(false);
        fetchPlaceDetails(prediction.place_id);
    };

    return (
        <div style={{ marginBottom: "32px" }}>
            <label style={{ display: "block", marginBottom: "8px", fontWeight: "600" }}>Địa chỉ</label>

            <input
                type="text"
                value={address}
                onChange={handleAddressChange}
                placeholder="Nhập hoặc tìm kiếm địa chỉ..."
                style={{
                    width: "100%",
                    padding: "14px",
                    borderRadius: "10px",
                    border: "1px solid #ddd",
                    marginBottom: "12px",
                    fontSize: "16px"
                }}
            />

            {/* Gợi ý địa chỉ */}
            {showSuggestions && suggestions.length > 0 && (
                <div style={{
                    border: "1px solid #ddd",
                    borderRadius: "8px",
                    maxHeight: "200px",
                    overflowY: "auto",
                    background: "white",
                    marginBottom: "16px"
                }}>
                    {suggestions.map((s, idx) => (
                        <div
                            key={idx}
                            onClick={() => selectSuggestion(s)}
                            style={{
                                padding: "12px 16px",
                                cursor: "pointer",
                                borderBottom: "1px solid #eee"
                            }}
                        >
                            {s.description}
                        </div>
                    ))}
                </div>
            )}

            {/* Bản đồ */}
            <div style={{
                position: "relative",
                height: "420px",
                borderRadius: "12px",
                overflow: "hidden",
                border: "1px solid #ddd"
            }}>
                <div ref={mapRef} style={{ width: "100%", height: "100%" }} />

                {/* Icon vị trí giữa bản đồ */}
                <div style={{
                    position: "absolute",
                    top: "50%",
                    left: "50%",
                    transform: "translate(-50%, -50%)",
                    pointerEvents: "none",
                    zIndex: 10
                }}>
                    <div style={{
                        width: "10px",
                        height: "10px",
                        background: "#86542B",
                        borderRadius: "50%",
                        border: "4px solid white",
                        boxShadow: "0 4px 15px rgba(134,84,43,0.5)",
                        display: "flex",
                        alignItems: "center",
                        justifyContent: "center",
                        color: "white",
                        fontSize: "24px"
                    }}>
                        🐶
                    </div>
                </div>
            </div>
        </div>
    );
}