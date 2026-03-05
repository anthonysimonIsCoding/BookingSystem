namespace BookingSystem.Entities;

public class TimeSlot
{
    public Guid Id { get; set; }

    public Guid StoreId { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public int Capacity { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Store Store { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}