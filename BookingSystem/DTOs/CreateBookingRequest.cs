using System;
namespace BookingSystem.DTOs;

public class CreateBookingRequest
{
    public Guid StoreId { get; set; }
    public Guid PetId { get; set; }
    public Guid TimeSlotId { get; set; }
    public DateOnly BookingDate { get; set; }

    public List<Guid> ServiceOptionIds { get; set; } = new();

    public string? Notes { get; set; }
    public string? PlatformVoucherCode { get; set; }
    public string? StoreVoucherCode { get; set; }
}