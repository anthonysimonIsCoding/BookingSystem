using BookingSystem.Entities.Enums;

namespace BookingSystem.Entities;

public class User
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string PasswordHash { get; set; } = null!;

    public UserRole Role { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}