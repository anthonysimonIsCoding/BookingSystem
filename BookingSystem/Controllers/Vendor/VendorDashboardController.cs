using BookingSystem.Data;
using BookingSystem.DTOs;   // namespace của bạn
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using BookingSystem.Entities.Enums;


namespace BookingSystem.Controllers.Vendor;

[Authorize(Roles = "ServiceProvider")]
[ApiController]
[Route("api/vendor")]
public class VendorDashboardController : ControllerBase
{
    private readonly BookingDbContext _context;

    public VendorDashboardController(BookingDbContext context)
    {
        _context = context;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr == null) return Unauthorized();
        var userId = Guid.Parse(userIdStr);

        // Lấy Store của Vendor (giả sử 1 vendor = 1 store)
        var store = await _context.Stores
            .FirstOrDefaultAsync(s => s.OwnerId == userId);

        if (store == null)
            return BadRequest("Bạn chưa có cửa hàng. Vui lòng liên hệ admin hoặc hoàn tất đăng ký cửa hàng.");

        var storeId = store.Id;

        // Thống kê cơ bản
        var totalBookings = await _context.Bookings.CountAsync(b => b.StoreId == storeId);
        var pendingBookings = await _context.Bookings.CountAsync(b => b.StoreId == storeId
            && b.Status != BookingStatus.Completed && b.Status != BookingStatus.Cancelled);

        var totalRevenue = await _context.Bookings
            .Where(b => b.StoreId == storeId && b.Status == BookingStatus.Completed)
            .SumAsync(b => b.TotalPrice);

        // Top Services (top 5)
        var topServices = await _context.BookingServiceItems
            .Include(bsi => bsi.ServiceOption)
                .ThenInclude(o => o.OptionGroup!)
                    .ThenInclude(g => g.Service)
            .Where(bsi => bsi.Booking.StoreId == storeId)
            .GroupBy(bsi => bsi.ServiceOption!.OptionGroup!.Service.Name)
            .Select(g => new TopServiceDto
            {
                ServiceName = g.Key,
                Price = g.First().ServiceOption!.OptionGroup!.Service.Price, // hoặc tính trung bình nếu cần
                BookingCount = g.Count()
            })
            .OrderByDescending(x => x.BookingCount)
            .Take(5)
            .ToListAsync();

        // Gán Rank
        for (int i = 0; i < topServices.Count; i++)
            topServices[i].Rank = i + 1;

        // Recent Reviews
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
                PetName = null, // Nếu muốn join Pet thì phức tạp hơn, tạm để null hoặc join qua Booking
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        var dashboard = new VendorDashboardDto
        {
            TotalBookings = totalBookings,
            PendingBookings = pendingBookings,
            TotalRevenue = totalRevenue,
            AverageRating = store.AverageRating,
            TopServices = topServices,
            RecentReviews = recentReviews
        };

        return Ok(dashboard);
    }
}