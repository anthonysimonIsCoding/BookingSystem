using System;
using System.Collections.Generic;

namespace BookingSystem.Entities;

public class Breed
{
    public Guid Id { get; set; }

    public Guid SpeciesId { get; set; }

    public string Name { get; set; } = null!;
    // Poodle, Husky, Golden Retriever,...

    public DateTime CreatedAt { get; set; }

    // Navigation
    public Species Species { get; set; } = null!;
    public ICollection<Pet> Pets { get; set; } = new List<Pet>();
}