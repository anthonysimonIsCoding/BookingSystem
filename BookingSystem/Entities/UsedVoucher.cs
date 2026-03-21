using System;

namespace BookingSystem.Entities;

public class UsedVoucher
{
    public Guid Id { get; set; }

    public Guid? PlatformVoucherId { get; set; }       // null nếu là store voucher
    public Guid? StoreVoucherId { get; set; }          // null nếu là platform voucher

    public Guid UserId { get; set; }                   // Ai dùng
    public Guid BookingId { get; set; }                // Đơn booking nào đã áp dụng

    public decimal DiscountApplied { get; set; }       // Số tiền thực tế được giảm

    public DateTime UsedAt { get; set; }

    // Navigation
    public PlatformVoucher? PlatformVoucher { get; set; }
    public StoreVoucher? StoreVoucher { get; set; }
    public User User { get; set; } = null!;
    public Booking Booking { get; set; } = null!;
}