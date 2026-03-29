using BookingSystem.Entities.Enums;

namespace BookingSystem.Entities;

public class Booking
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid StoreId { get; set; }

    public Guid TimeSlotId { get; set; }

    public Guid PetId { get; set; }      
    
   // <-- Thêm trường này

    public DateOnly BookingDate { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public string? CancellationReason { get; set; }

    public Guid? PlatformVoucherId { get; set; }
    public Guid? StoreVoucherId { get; set; }

    public decimal? PlatformVoucherDiscount { get; set; }  // Số tiền giảm từ voucher toàn sàn
    public decimal? StoreVoucherDiscount { get; set; }

    public decimal TotalPrice { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Store Store { get; set; } = null!;
    public TimeSlot TimeSlot { get; set; } = null!;
    public Pet Pet { get; set; } = null!;
    public PlatformVoucher? PlatformVoucher { get; set; }
    public StoreVoucher? StoreVoucher { get; set; }
    public ICollection<BookingServiceItem> ServiceItems { get; set; } = new List<BookingServiceItem>();

}