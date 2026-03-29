using BookingSystem.Entities.Enums;
using System;
using System.Collections.Generic;

namespace BookingSystem.Entities;

public class Service
{
    public Guid Id { get; set; }

    public Guid StoreId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int DurationMinutes { get; set; }

    public ServiceType Type { get; set; }

    public bool IsActive { get; set; } = true;

    public Store Store { get; set; } = null!;

    public ICollection<ServiceOptionGroup> OptionGroups { get; set; } = new List<ServiceOptionGroup>();

}