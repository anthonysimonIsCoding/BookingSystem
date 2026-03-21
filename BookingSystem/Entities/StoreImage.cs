using System;

namespace BookingSystem.Entities;

public class StoreImage
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; }
    public string ImageUrl { get; set; } = null!; // URL của ảnh (ví dụ: "https://storage.example.com/store1/thumbnail.jpg")
    public bool IsThumbnail { get; set; } = false; // True nếu là thumbnail chính
    public int Order { get; set; } = 0; // Thứ tự hiển thị (0 là cao nhất)
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Store Store { get; set; } = null!;
}