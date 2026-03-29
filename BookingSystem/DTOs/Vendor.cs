using System;
using System.Collections.Generic;
namespace BookingSystem.DTOs;

public class VendorDashboardDto
{
    public int TotalBookings { get; set; }
    public int PendingBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageRating { get; set; }

    public List<TopServiceDto> TopServices { get; set; } = new();
    public List<RecentReviewDto> RecentReviews { get; set; } = new();
}

public class TopServiceDto
{
    public int Rank { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int BookingCount { get; set; }
}

public class RecentReviewDto
{
    public decimal Rating { get; set; }
    public string? Comment { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? PetName { get; set; }
    public DateTime CreatedAt { get; set; }
}