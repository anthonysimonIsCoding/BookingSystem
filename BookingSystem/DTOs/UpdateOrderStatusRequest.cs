using System;
using BookingSystem.Entities.Enums;
namespace BookingSystem.DTOs;

public class UpdateOrderStatusRequest
{
    public BookingStatus Status { get; set; }
}