import { useEffect, useRef, useState } from "react"
import maplibregl from "maplibre-gl"
import axios from "axios"
import { createRoot } from "react-dom/client"
import { useNavigate } from "react-router-dom"
import StoreCard from "../components/StoreCard"

import "maplibre-gl/dist/maplibre-gl.css"

interface Store {
    id: string
    name: string
    address: string
    latitude: number
    longitude: number
    thumbnail: string
}

const mapKey = "bQvmssxbjRm6JeS82OOQGtLp3SZ7iDUP1BpAAQfa"

function getDistanceKm(
    lat1: number,
    lon1: number,
    lat2: number,
    lon2: number
) {
    const R = 6371

    const dLat = (lat2 - lat1) * Math.PI / 180
    const dLon = (lon2 - lon1) * Math.PI / 180

    const a =
        Math.sin(dLat / 2) * Math.sin(dLat / 2) +
        Math.cos(lat1 * Math.PI / 180) *
        Math.cos(lat2 * Math.PI / 180) *
        Math.sin(dLon / 2) *
        Math.sin(dLon / 2)

    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a))

    return R * c
}

export default function StoreMap() {

    const mapRef = useRef<HTMLDivElement | null>(null)
    const mapInstance = useRef<maplibregl.Map | null>(null)

    const userMarkerRef = useRef<maplibregl.Marker | null>(null)
    const storeMarkersRef = useRef<maplibregl.Marker[]>([])

    const [radius, setRadius] = useState(5)
    const [stores, setStores] = useState<Store[]>([])
    const [userLocation, setUserLocation] =
        useState<{ lat: number; lng: number } | null>(null)

    const navigate = useNavigate()

    // INIT MAP
    useEffect(() => {

        if (!mapRef.current) return
        if (mapInstance.current) return   // tránh init map 2 lần

        navigator.geolocation.getCurrentPosition(async (pos) => {

            const lat = pos.coords.latitude
            const lng = pos.coords.longitude

            setUserLocation({ lat, lng })

            const map = new maplibregl.Map({
                container: mapRef.current!,
                style: `https://tiles.goong.io/assets/goong_map_highlight.json?api_key=${mapKey}`,
                center: [lng, lat],
                zoom: 14
            })

            mapInstance.current = map

            // USER MARKER
            const userEl = document.createElement("div")

            userEl.style.width = "18px"
            userEl.style.height = "18px"
            userEl.style.borderRadius = "50%"
            userEl.style.background = "#1E90FF"
            userEl.style.border = "4px solid white"
            userEl.style.boxShadow = "0 0 8px rgba(30,144,255,0.8)"
            userEl.style.zIndex = "999"

            if (userMarkerRef.current) {
                userMarkerRef.current.remove()
            }

            userMarkerRef.current = new maplibregl.Marker({
                element: userEl
            })
                .setLngLat([lng, lat])
                .addTo(map)

            // LOAD STORES
            const res = await axios.get<Store[]>(
                "http://localhost:5263/api/stores"
            )

            setStores(res.data)

        })

        return () => {
            mapInstance.current?.remove()
            mapInstance.current = null
        }

    }, [])

    // RENDER STORES
    useEffect(() => {

        if (!mapInstance.current || !userLocation) return

        const map = mapInstance.current

        // REMOVE OLD MARKERS
        storeMarkersRef.current.forEach(marker => marker.remove())
        storeMarkersRef.current = []

        stores.forEach(store => {

            const distance = getDistanceKm(
                userLocation.lat,
                userLocation.lng,
                store.latitude,
                store.longitude
            )

            if (distance > radius) return

            // MARKER CONTAINER
            const el = document.createElement("div")
            el.style.position = "relative"
            el.style.width = "40px"
            el.style.height = "40px"
            el.style.cursor = "pointer"

            // ICON (đúng vị trí store)
            const img = document.createElement("div")
            img.style.width = "40px"
            img.style.height = "40px"
            img.style.borderRadius = "50%"
            img.style.backgroundImage = `url(${store.thumbnail})`
            img.style.backgroundSize = "cover"
            img.style.backgroundPosition = "center"
            img.style.border = "3px solid white"
            img.style.boxShadow = "0 2px 6px rgba(0,0,0,0.3)"

            // LABEL (nằm bên trái icon)
            const label = document.createElement("div")
            label.innerText = store.name

            label.style.position = "absolute"
            label.style.right = "45px"
            label.style.top = "50%"
            label.style.transform = "translateY(-50%)"

            label.style.background = "white"
            label.style.padding = "4px 10px"
            label.style.borderRadius = "999px"
            label.style.fontSize = "13px"
            label.style.fontWeight = "600"
            label.style.whiteSpace = "nowrap"
            label.style.boxShadow = "0 2px 6px rgba(0,0,0,0.2)"

            label.style.maxWidth = "140px"
            label.style.overflow = "hidden"
            label.style.textOverflow = "ellipsis"

            el.appendChild(img)
            el.appendChild(label)


            // popup container
            const popupDiv = document.createElement("div")
            popupDiv.style.width = "220px"



            const root = createRoot(popupDiv)

            root.render(
                <StoreCard
                    store={{
                        id: store.id,
                        name: store.name,
                        address: store.address
                    }}
                    onClick={() => navigate(`/store/${store.id}`)}
                />
            )

            // tạo popup
            const popup = new maplibregl.Popup({
                closeButton: false,
                closeOnClick: false,
                offset: 25
            }).setDOMContent(popupDiv)

            const marker = new maplibregl.Marker({
                element: el,
               
            })
                .setLngLat([store.longitude, store.latitude])
                .addTo(map)

            // hover marker
            el.addEventListener("mouseenter", () => {
                popup.setLngLat([store.longitude, store.latitude]).addTo(map)
            })

            el.addEventListener("mouseleave", () => {
                setTimeout(() => {
                    if (!popupDiv.matches(":hover")) {
                        popup.remove()
                    }
                }, 200)
            })

            // hover card
            popupDiv.addEventListener("mouseleave", () => {
                popup.remove()
            })

            storeMarkersRef.current.push(marker)

        })

    }, [radius, stores, userLocation])

    return (

        <div>

            <div style={{ marginBottom: "10px" }}>
                <label>Bán kính tìm kiếm: {radius} km</label>

                <input
                    type="range"
                    min="1"
                    max="20"
                    value={radius}
                    onChange={(e) => setRadius(Number(e.target.value))}
                />
            </div>

            <div
                ref={mapRef}
                style={{
                    width: "100%",
                    height: "500px",
                    borderRadius: "16px",
                    overflow: "visible"
                }}
            />

        </div>

    )
}