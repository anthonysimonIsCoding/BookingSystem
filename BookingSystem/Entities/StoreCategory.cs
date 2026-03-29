using System;
using System.Collections.Generic;

namespace BookingSystem.Entities;

public class StoreCategory
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;
    // Spa, Hotel, Daycare, Dog Walking,...

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<StoreCategoryMapping> StoreCategories { get; set; } = new List<StoreCategoryMapping>();
}