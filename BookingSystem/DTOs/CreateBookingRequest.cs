using System;
namespace BookingSystem.DTOs;

public class CreateBookingRequest
{
    public Guid StoreId { get; set; }

    public Guid TimeSlotId { get; set; }

    public Guid PetId { get; set; }

    public DateOnly BookingDate { get; set; }

    public List<Guid> ServiceIds { get; set; } = new();
}