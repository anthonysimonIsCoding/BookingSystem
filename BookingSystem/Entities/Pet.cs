using System;
using System.Collections.Generic;

namespace BookingSystem.Entities;

public class Pet
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Name { get; set; } = null!;

    public Guid SpeciesId { get; set; }

    public Guid? BreedId { get; set; }

    public string? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Color { get; set; }

    public double? Weight { get; set; }

    public string? Notes { get; set; }

    public string? ProfileImageUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public User User { get; set; } = null!;
    public Species Species { get; set; } = null!;
    public Breed? Breed { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}