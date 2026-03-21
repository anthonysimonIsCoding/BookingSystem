using System;
namespace BookingSystem.DTOs;

public class UpdateUserRequest
{
    public string FullName { get; set; } = null!;
    public string? PhoneNumber { get; set; }
}