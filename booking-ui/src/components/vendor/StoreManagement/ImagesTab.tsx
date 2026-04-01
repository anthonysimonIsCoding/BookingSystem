// src/components/vendor/ImagesTab.tsx
import React from 'react';

interface ImageItem {
    id?: string;
    imageUrl: string;
    isThumbnail: boolean;
    order: number;
    isNew?: boolean;
    file?: File;
}

interface ImagesTabProps {
    images: ImageItem[];
    onImagesChange: (images: ImageItem[]) => void;
}

export default function ImagesTab({ images, onImagesChange }: ImagesTabProps) {

    const handleAddImages = (e: React.ChangeEvent<HTMLInputElement>) => {
        const files = Array.from(e.target.files || []);
        if (images.length + files.length > 8) {
            alert("Cửa hàng chỉ được tối đa 8 ảnh!");
            return;
        }

        const newItems: ImageItem[] = files.map((file, i) => ({
            imageUrl: URL.createObjectURL(file),
            isThumbnail: false,
            order: images.length + i,
            isNew: true,
            file
        }));

        onImagesChange([...images, ...newItems]);
    };

    const removeImage = (index: number) => {
        onImagesChange(images.filter((_, i) => i !== index));
    };

    const toggleThumbnail = (index: number) => {
        const updated = images.map((img, i) => ({
            ...img,
            isThumbnail: i === index
        }));
        onImagesChange(updated);
    };

    const onDragStart = (e: React.DragEvent, index: number) => {
        e.dataTransfer.setData("text/plain", index.toString());
    };

    const onDrop = (e: React.DragEvent, toIndex: number) => {
        e.preventDefault();
        const fromIndex = parseInt(e.dataTransfer.getData("text/plain"));
        if (fromIndex === toIndex) return;

        const updated = [...images];
        const [moved] = updated.splice(fromIndex, 1);
        updated.splice(toIndex, 0, moved);

        const reordered = updated.map((img, idx) => ({ ...img, order: idx }));
        onImagesChange(reordered);
    };

    return (
        <div style={{ marginBottom: "40px" }}>
            <label style={{ display: "block", marginBottom: "12px", fontWeight: "600" }}>
                Hình ảnh cửa hàng ({images.length}/8)
            </label>
            <div style={{ display: "flex", flexWrap: "wrap", gap: "12px" }}>
                {images.map((img, idx) => (
                    <div
                        key={idx}
                        draggable
                        onDragStart={(e) => onDragStart(e, idx)}
                        onDrop={(e) => onDrop(e, idx)}
                        onDragOver={(e) => e.preventDefault()}
                        style={{ position: "relative", width: "160px", cursor: "grab" }}
                    >
                        <img src={img.imageUrl} style={{ width: "100%", height: "110px", objectFit: "cover", borderRadius: "10px" }} alt="store" />
                        {img.isThumbnail && <div style={{ position: "absolute", top: 6, left: 6, background: "#10b981", color: "white", padding: "2px 8px", borderRadius: "4px", fontSize: "11px" }}>Ảnh chính</div>}
                        <button onClick={() => toggleThumbnail(idx)} style={{ position: "absolute", top: 6, right: 40, background: "#3b82f6", color: "white", border: "none", borderRadius: "4px", padding: "2px 6px", fontSize: "12px" }}>Chính</button>
                        <button onClick={() => removeImage(idx)} style={{ position: "absolute", top: 6, right: 6, background: "#ef4444", color: "white", border: "none", borderRadius: "4px", padding: "2px 6px", fontSize: "12px" }}>Xóa</button>
                    </div>
                ))}

                {images.length < 8 && (
                    <label style={{ width: "160px", height: "110px", border: "2px dashed #86542B", borderRadius: "10px", display: "flex", alignItems: "center", justifyContent: "center", cursor: "pointer", color: "#86542B" }}>
                        <input type="file" multiple accept="image/*" onChange={handleAddImages} style={{ display: "none" }} />
                        <div style={{ textAlign: "center" }}>
                            <div style={{ fontSize: "32px" }}>+</div>
                            <div style={{ fontSize: "13px" }}>Thêm ảnh</div>
                        </div>
                    </label>
                )}
            </div>
        </div>
    );
}