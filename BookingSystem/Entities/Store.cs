namespace BookingSystem.Entities;

public class Store
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public decimal AverageRating { get; set; } = 0.0m; // Trung bình số sao (tính từ Reviews)

    public int ReviewCount { get; set; } = 0; // Số lượng review
    public int TotalCompletedBookings { get; set; } = 0;  // <-- Thêm dòng này
    // Navigation
    public ICollection<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<StoreImage> Images { get; set; } = new List<StoreImage>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Service> Services { get; set; } = new List<Service>();
    public ICollection<StoreSpecies> StoreSpecies { get; set; } = new List<StoreSpecies>();
    public ICollection<StoreCategoryMapping> StoreCategories { get; set; } = new List<StoreCategoryMapping>();
}