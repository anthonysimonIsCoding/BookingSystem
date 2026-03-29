using System;
namespace BookingSystem.DTOs;

public class OrderServiceItemDto
{
    public string OptionName { get; set; } = string.Empty;
    public decimal Price { get; set; }
}