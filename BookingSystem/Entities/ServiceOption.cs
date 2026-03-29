using System;
using System.Collections.Generic;

namespace BookingSystem.Entities;

public class ServiceOption
{
    public Guid Id { get; set; }

    public Guid OptionGroupId { get; set; }

    public string Name { get; set; } = null!; // "Chó nhỏ", "60 phút"

    public decimal Price { get; set; }

    public int DurationMinutes { get; set; }

    public bool IsActive { get; set; } = true;

    public ServiceOptionGroup OptionGroup { get; set; } = null!;

    public ICollection<BookingServiceItem> BookingItems { get; set; } = new List<BookingServiceItem>();
}