using BookingSystem.Entities.Enums;
using System;

namespace BookingSystem.Entities;

public class BookingService
{
    public Guid BookingId { get; set; }
    public Guid ServiceId { get; set; }

    public decimal Price { get; set; } // lưu giá tại thời điểm đặt

    // Navigation
    public Booking Booking { get; set; } = null!;
    public Service Service { get; set; } = null!;
}