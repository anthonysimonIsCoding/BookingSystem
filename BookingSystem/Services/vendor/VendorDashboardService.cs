using BookingSystem.Data;
using BookingSystem.DTOs;
using BookingSystem.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookingSystem.Services;

public class VendorDashboardService
{
    private readonly BookingDbContext _context;

    public VendorDashboardService(BookingDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lấy dữ liệu Dashboard cho Vendor (ServiceProvider)
    /// </summary>
    public async Task<VendorDashboardDto> GetDashboardAsync(Guid userId)
    {
        // Lấy Store của Vendor (giả sử 1 vendor = 1 store)
        var store = await _context.Stores
            .FirstOrDefaultAsync(s => s.OwnerId == userId);

        if (store == null)
            throw new InvalidOperationException("Bạn chưa có cửa hàng. Vui lòng liên hệ admin hoặc hoàn tất đăng ký cửa hàng.");

        var storeId = store.Id;

        // Thống kê cơ bản
        var totalBookings = await _context.Bookings.CountAsync(b => b.StoreId == storeId);

        var pendingBookings = await _context.Bookings.CountAsync(b => b.StoreId == storeId
            && b.Status != BookingStatus.Completed
            && b.Status != BookingStatus.Cancelled);

        var totalRevenue = await _context.Bookings
            .Where(b => b.StoreId == storeId && b.Status == BookingStatus.Completed)
            .SumAsync(b => b.TotalPrice);

        // Top 5 Services
        var topServices = await _context.BookingServiceItems
            .Include(bsi => bsi.ServiceOption)
                .ThenInclude(o => o.OptionGroup!)
                    .ThenInclude(g => g.Service)
            .Where(bsi => bsi.Booking.StoreId == storeId)
            .GroupBy(bsi => bsi.ServiceOption!.OptionGroup!.Service.Name)
            .Select(g => new TopServiceDto
            {
                ServiceName = g.Key,
                Price = g.First().ServiceOption!.OptionGroup!.Service.Price,
                BookingCount = g.Count()
            })
            .OrderByDescending(x => x.BookingCount)
            .Take(5)
            .ToListAsync();

        // Gán Rank
        for (int i = 0; i < topServices.Count; i++)
        {
            topServices[i].Rank = i + 1;
        }

        // Recent Reviews (10 đánh giá mới nhất)
        var recentReviews = await _context.Reviews
            .Include(r => r.User)
            .Where(r => r.StoreId == storeId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(10)
            .Select(r => new RecentReviewDto
            {
                Rating = r.Rating,
                Comment = r.Comment,
                CustomerName = r.User.FullName,
                PetName = null,                    // Có thể mở rộng sau nếu cần
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        // Trả về DTO tổng hợp
        return new VendorDashboardDto
        {
            TotalBookings = totalBookings,
            PendingBookings = pendingBookings,
            TotalRevenue = totalRevenue,
            AverageRating = store.AverageRating,
            TopServices = topServices,
            RecentReviews = recentReviews
        };
    }
}