using System;
using System.Collections.Generic;
using BookingSystem.Entities.Enums;

namespace BookingSystem.DTOs;

public class OrderOptionDto
{
    public string OptionName { get; set; } = string.Empty;
    public decimal Price { get; set; }
}