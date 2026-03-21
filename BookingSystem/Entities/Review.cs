using System;
using System.Collections.Generic;

namespace BookingSystem.Entities;


public class Review
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; }
    public Guid UserId { get; set; }
    public decimal Rating { get; set; } // Số sao (1.0 đến 5.0)
    public string? Comment { get; set; } // Comment đánh giá
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Store Store { get; set; } = null!;
    public User User { get; set; } = null!;
}