using System;

namespace BookingSystem.DTOs;

public class VendorRegisterRequest
{
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string StoreName { get; set; } = null!;
    public string Address { get; set; } = null!;
}