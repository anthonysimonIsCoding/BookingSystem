using System;

public class CreateBookingRequest
{
    public Guid StoreId { get; set; }
    public Guid TimeSlotId { get; set; }
    public DateOnly BookingDate { get; set; }
}