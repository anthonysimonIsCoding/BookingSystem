using System;

namespace BookingSystem.DTOs;

public class StoreCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}