using System;
using BookingSystem.Entities.Enums;
namespace BookingSystem.DTOs;

public class VendorOrderDto
{
    public Guid Id { get; set; }
    public string PetName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public DateOnly BookingDate { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public BookingStatus Status { get; set; }
    public decimal TotalPrice { get; set; }
}