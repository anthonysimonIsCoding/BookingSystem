using BookingSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using BookingSystem.Entities.Enums;

namespace BookingSystem.Controllers;

[Authorize(Roles = "Customer")]
[ApiController]
[Route("api/[controller]")]
public class TimeSlotsController : ControllerBase
{
    private readonly BookingDbContext _context;

    public TimeSlotsController(BookingDbContext context)
    {
        _context = context;
    }

    [HttpGet("store/{storeId}")]
    public async Task<IActionResult> GetByStore(Guid storeId, [FromQuery] DateOnly? date = null)
    {
        var targetDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);

        // 1. Kiểm tra full day closure
        var fullDayClosure = await _context.TimeSlotOverrides
            .FirstOrDefaultAsync(o =>
                o.StoreId == storeId &&
                o.Date == targetDate &&
                o.TimeSlotId == null &&
                o.IsFullDayClosure);

        if (fullDayClosure != null)
            return Ok(new List<object>());

        // 2. Lấy template slots + SẮP XẾP THEO GIỜ (quan trọng nhất)
        var templates = await _context.TimeSlots
            .Where(s => s.StoreId == storeId && s.IsActive)
            .OrderBy(s => s.StartTime)          // ← SẮP XẾP TỪ SỚM → MUỘN
            .ToListAsync();

        // 3. Lấy override theo ngày
        var overrides = await _context.TimeSlotOverrides
            .Where(o => o.StoreId == storeId && o.Date == targetDate)
            .ToListAsync();

        var result = new List<object>();

        foreach (var slot in templates)
        {
            var ov = overrides.FirstOrDefault(o => o.TimeSlotId == slot.Id);

            bool isActive = ov?.IsActive ?? slot.IsActive;
            var effectiveCapacity = ov?.Capacity ?? slot.Capacity;
            var effectiveStart = ov?.StartTime ?? slot.StartTime;
            var effectiveEnd = ov?.EndTime ?? slot.EndTime;

            var booked = await _context.Bookings
                .CountAsync(b => b.TimeSlotId == slot.Id
                              && b.BookingDate == targetDate
                              && b.Status != BookingStatus.Cancelled);

            var remaining = effectiveCapacity - booked;

            result.Add(new
            {
                id = slot.Id,
                startTime = effectiveStart.ToString(@"hh\:mm"),
                endTime = effectiveEnd.ToString(@"hh\:mm"),
                capacity = effectiveCapacity,
                remainingCapacity = remaining,
                isAvailable = isActive && remaining > 0,
                isOverridden = ov != null,
                overrideReason = ov?.Reason ?? (isActive ? null : "Slot bị tạm ngưng"),
                isDisabledByOverride = !isActive
            });
        }

        return Ok(result);
    }
}