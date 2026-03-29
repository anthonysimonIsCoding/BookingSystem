using BookingSystem.Entities.Enums;
using System;

namespace BookingSystem.Entities;

public class BookingServiceItem
{
    public Guid Id { get; set; }

    public Guid BookingId { get; set; }

    public Guid ServiceOptionId { get; set; }

    public decimal Price { get; set; }   // snapshot giá tại thời điểm đặt

    public int DurationMinutes { get; set; }

    public Booking Booking { get; set; } = null!;

    public ServiceOption ServiceOption { get; set; } = null!;
}