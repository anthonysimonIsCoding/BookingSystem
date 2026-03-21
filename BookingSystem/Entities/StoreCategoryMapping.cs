using System;

namespace BookingSystem.Entities;

public class StoreCategoryMapping
{
    public Guid StoreId { get; set; }

    public Guid CategoryId { get; set; }

    // Navigation
    public Store Store { get; set; } = null!;
    public StoreCategory Category { get; set; } = null!;
}