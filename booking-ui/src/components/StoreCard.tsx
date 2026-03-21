export default function StoreCard({ store }: any) {

    return (
        <div style={{
            background: "#fff",
            borderRadius: 12,
            overflow: "hidden",
            border: "1px solid #eee"
        }}>
            <img
                src={store.thumbnail}
                style={{ width: "100%", height: 160, objectFit: "cover" }}
            />

            <div style={{ padding: 12 }}>
                <h4>{store.name}</h4>
                <p>{store.address}</p>

                <p>⭐ {store.averageRating}</p>
                <p>💰 từ {store.minPrice}</p>

                {store.distance && (
                    <p>📍 {store.distance.toFixed(1)} km</p>
                )}
            </div>
        </div>
    );
}