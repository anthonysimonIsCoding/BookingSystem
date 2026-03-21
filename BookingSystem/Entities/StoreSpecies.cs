using System;

namespace BookingSystem.Entities;

public class StoreSpecies
{
    public Guid StoreId { get; set; }

    public Guid SpeciesId { get; set; }

    // Navigation
    public Store Store { get; set; } = null!;
    public Species Species { get; set; } = null!;
}