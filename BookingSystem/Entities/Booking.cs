using BookingSystem.Entities.Enums;

namespace BookingSystem.Entities;

public class Booking
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid StoreId { get; set; }

    public Guid TimeSlotId { get; set; }

    public DateOnly BookingDate { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Active;

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public string? CancellationReason { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Store Store { get; set; } = null!;
    public TimeSlot TimeSlot { get; set; } = null!;
}