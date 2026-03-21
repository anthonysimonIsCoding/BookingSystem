using System;
using System.Collections.Generic;

namespace BookingSystem.Entities;

public class Species
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;
    // Dog, Cat, Rabbit,...

    public DateTime CreatedAt { get; set; }

    // Navigation
    public ICollection<Breed> Breeds { get; set; } = new List<Breed>();
}