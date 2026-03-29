using BookingSystem.Data;
using BookingSystem.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookingSystem.Controllers.Vendor;

[Authorize(Roles = "ServiceProvider")]
[ApiController]
[Route("api/vendor/store")]
public class VendorStoreController : ControllerBase
{
    private readonly BookingDbContext _context;

    public VendorStoreController(BookingDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetStoreInfo()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var store = await _context.Stores
                .Include(s => s.Images)
                .FirstOrDefaultAsync(s => s.OwnerId == userId);

            if (store == null)
                return NotFound(new { message = "Không tìm thấy cửa hàng" });

            return Ok(new
            {
                id = store.Id,
                name = store.Name,
                address = store.Address,
                averageRating = store.AverageRating,
                reviewCount = store.ReviewCount,
                status = store.Status,
                images = store.Images
                    .OrderBy(i => i.Order)
                    .Select(i => new { i.Id, i.ImageUrl, i.IsThumbnail })
                    .ToList()
            });
        }
        catch (Exception ex)
        {
            // Log lỗi để xem chính xác vấn đề gì
            Console.WriteLine($"Error in GetStoreInfo: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateStoreInfo([FromBody] UpdateStoreRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var store = await _context.Stores.FirstOrDefaultAsync(s => s.OwnerId == userId);
        if (store == null) return NotFound("Không tìm thấy cửa hàng");

        store.Name = request.Name;
        store.Address = request.Address;
        store.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Cập nhật thông tin cửa hàng thành công" });
    }

    public class UpdateStoreRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }

    [HttpGet("timeslots")]
    public async Task<IActionResult> GetTimeSlots()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var store = await _context.Stores.FirstOrDefaultAsync(s => s.OwnerId == userId);
        if (store == null) return NotFound();

        var slots = await _context.TimeSlots
            .Where(t => t.StoreId == store.Id && t.IsActive)
            .OrderBy(t => t.StartTime)
            .ToListAsync();

        return Ok(slots);
    }

    [HttpGet("overrides")]
    public async Task<IActionResult> GetOverrides([FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var store = await _context.Stores.FirstOrDefaultAsync(s => s.OwnerId == userId);
        if (store == null) return NotFound();

        var query = _context.TimeSlotOverrides.Where(o => o.StoreId == store.Id);

        if (fromDate.HasValue) query = query.Where(o => o.Date >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(o => o.Date <= toDate.Value);

        var data = await query
            .OrderByDescending(o => o.Date)
            .ThenBy(o => o.StartTime)
            .ToListAsync();

        return Ok(data);
    }

    [HttpPost("overrides")]
    public async Task<IActionResult> CreateOverride([FromBody] TimeSlotOverrideRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var store = await _context.Stores.FirstOrDefaultAsync(s => s.OwnerId == userId);
        if (store == null) return NotFound("Không tìm thấy cửa hàng");

        var ov = new TimeSlotOverride
        {
            StoreId = store.Id,
            TimeSlotId = request.TimeSlotId,
            Date = request.Date,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Capacity = request.Capacity,
            IsFullDayClosure = request.IsFullDayClosure,
            Reason = request.Reason ?? "Override thủ công",
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.TimeSlotOverrides.Add(ov);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Tạo override thành công", overrideId = ov.Id });
    }

    public class TimeSlotOverrideRequest
    {
        public Guid? TimeSlotId { get; set; }
        public DateOnly Date { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public int? Capacity { get; set; }
        public bool IsFullDayClosure { get; set; } = false;
        public string? Reason { get; set; }
    }
}