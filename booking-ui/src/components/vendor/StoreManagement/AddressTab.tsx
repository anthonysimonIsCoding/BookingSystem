// src/components/vendor/AddressTab.tsx
import React, { useEffect, useRef, useState } from 'react';
import maplibregl from "maplibre-gl";
import "maplibre-gl/dist/maplibre-gl.css";

const MAP_TILE_KEY = "bQvmssxbjRm6JeS82OOQGtLp3SZ7iDUP1BpAAQfa";
const API_KEY = "qA25nsR6quBvGmC2mykSw4Rs2249ZHd8Z3DOExix";

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
    const [searchTimeout, setSearchTimeout] = useState<NodeJS.Timeout | null>(null);

    const handleAddressChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const value = e.target.value;
        onAddressChange(value);

        if (searchTimeout) clearTimeout(searchTimeout);
        const timeout = setTimeout(() => {
            if (value.length > 2) fetchAutoComplete(value);
            else {
                setSuggestions([]);
                setShowSuggestions(false);
            }
        }, 600);
        setSearchTimeout(timeout);
    };

    const fetchAutoComplete = (query: string) => {
        fetch(`https://rsapi.goong.io/place/autocomplete?api_key=${API_KEY}&input=${encodeURIComponent(query)}`)
            .then(res => res.json())
            .then(data => {
                setSuggestions(data.predictions || []);
                setShowSuggestions(true);
            })
            .catch(() => setSuggestions([]));
    };

    const selectSuggestion = (prediction: any) => {
        onAddressChange(prediction.description);
        setShowSuggestions(false);
        fetchPlaceDetails(prediction.place_id);
    };

    const fetchPlaceDetails = (placeId: string) => {
        fetch(`https://rsapi.goong.io/place/detail?api_key=${API_KEY}&place_id=${placeId}`)
            .then(res => res.json())
            .then(data => {
                if (data.result?.geometry?.location) {
                    const { lat, lng } = data.result.geometry.location;
                    onLocationChange(lat, lng);
                    if (mapInstance.current) mapInstance.current.flyTo({ center: [lng, lat], zoom: 16 });
                }
            })
            .catch(err => console.error(err));
    };

    const reverseGeocode = async (lat: number, lng: number) => {
        try {
            const res = await fetch(`https://rsapi.goong.io/v2/geocode/street?api_key=${API_KEY}&latlng=${lat},${lng}`);
            const data = await res.json();
            if (data.results?.length > 0) {
                onAddressChange(data.results[0].formatted_address || "");
            }
        } catch (err) {
            console.error(err);
        }
    };

    useEffect(() => {
        if (!mapRef.current) return;

        const map = new maplibregl.Map({
            container: mapRef.current,
            style: `https://tiles.goong.io/assets/goong_map_highlight.json?api_key=${MAP_TILE_KEY}`,
            center: [longitude, latitude],
            zoom: 15
        });

        mapInstance.current = map;

        let isUserDragging = false;
        map.on('dragstart', () => { isUserDragging = true; });
        map.on('moveend', () => {
            if (!isUserDragging) return;
            const center = map.getCenter();
            const newLat = parseFloat(center.lat.toFixed(6));
            const newLng = parseFloat(center.lng.toFixed(6));
            onLocationChange(newLat, newLng);
            reverseGeocode(newLat, newLng);
            isUserDragging = false;
        });

        return () => map.remove();
    }, [latitude, longitude]);

    return (
        <div style={{ marginBottom: "32px" }}>
            <label style={{ display: "block", marginBottom: "8px", fontWeight: "600" }}>Địa chỉ</label>
            <input
                type="text"
                value={address}
                onChange={handleAddressChange}
                placeholder="Nhập hoặc tìm kiếm địa chỉ..."
                style={{ width: "100%", padding: "14px", borderRadius: "10px", border: "1px solid #ddd", marginBottom: "12px" }}
            />

            {showSuggestions && suggestions.length > 0 && (
                <div style={{ border: "1px solid #ddd", borderRadius: "8px", maxHeight: "200px", overflowY: "auto", background: "white", marginBottom: "16px" }}>
                    {suggestions.map((s, idx) => (
                        <div key={idx} onClick={() => selectSuggestion(s)} style={{ padding: "12px 16px", cursor: "pointer", borderBottom: "1px solid #eee" }}>
                            {s.description}
                        </div>
                    ))}
                </div>
            )}

            <div style={{ position: "relative", height: "420px", borderRadius: "12px", overflow: "hidden", border: "1px solid #ddd" }}>
                <div ref={mapRef} style={{ width: "100%", height: "100%" }} />
                <div style={{ position: "absolute", top: "50%", left: "50%", transform: "translate(-50%, -50%)", pointerEvents: "none", zIndex: 10 }}>
                    <div style={{ width: "44px", height: "44px", background: "#86542B", borderRadius: "50%", border: "4px solid white", boxShadow: "0 4px 15px rgba(134,84,43,0.5)", display: "flex", alignItems: "center", justifyContent: "center", color: "white", fontSize: "24px" }}>📍</div>
                </div>
            </div>
        </div>
    );
}