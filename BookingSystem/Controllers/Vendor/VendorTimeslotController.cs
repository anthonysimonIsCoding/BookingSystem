// Controllers/Vendor/VendorTimeslotController.cs
using BookingSystem.Data;
using BookingSystem.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookingSystem.Controllers.Vendor;

[Authorize(Roles = "ServiceProvider")]
[ApiController]
[Route("api/vendor/timeslot")]
public class VendorTimeslotController : ControllerBase
{
    private readonly BookingDbContext _context;

    public VendorTimeslotController(BookingDbContext context)
    {
        _context = context;
    }

    // ====================== LẤY TIMESLOT CỐ ĐỊNH ======================
    [HttpGet]
    public async Task<IActionResult> GetTimeSlots()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var store = await _context.Stores.FirstOrDefaultAsync(s => s.OwnerId == userId);
        if (store == null) return NotFound("Không tìm thấy cửa hàng");

        var slots = await _context.TimeSlots
            .Where(t => t.StoreId == store.Id)
            .Select(t => new
            {
                t.Id,
                t.StartTime,
                t.EndTime,
                t.Capacity,
                t.IsActive
            })
            .OrderBy(t => t.StartTime)
            .ToListAsync();

        return Ok(slots);
    }

    // ====================== LƯU / THÊM / SỬA TIMESLOT ======================
    [HttpPost]
    public async Task<IActionResult> SaveTimeSlots([FromBody] List<TimeSlotRequest> requests)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var store = await _context.Stores.FirstOrDefaultAsync(s => s.OwnerId == userId);
        if (store == null) return NotFound("Không tìm thấy cửa hàng");

        foreach (var req in requests)
        {
            if (req.StartTime >= req.EndTime)
                return BadRequest($"Khung giờ {req.StartTime} - {req.EndTime} không hợp lệ.");

            if (!string.IsNullOrEmpty(req.Id) && Guid.TryParse(req.Id, out Guid reqId))
            {
                // Edit
                var existing = await _context.TimeSlots.FirstOrDefaultAsync(t => t.Id == reqId && t.StoreId == store.Id);
                if (existing != null)
                {
                    existing.StartTime = req.StartTime;
                    existing.EndTime = req.EndTime;
                    existing.Capacity = req.Capacity;
                    existing.IsActive = req.IsActive;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
            }
            else
            {
                // Thêm mới
                var isDuplicate = await _context.TimeSlots.AnyAsync(t =>
                    t.StoreId == store.Id &&
                    t.StartTime == req.StartTime &&
                    t.EndTime == req.EndTime);

                if (isDuplicate)
                    return BadRequest($"Khung giờ {req.StartTime} - {req.EndTime} đã tồn tại");

                var newSlot = new TimeSlot
                {
                    Id = Guid.NewGuid(),
                    StoreId = store.Id,
                    StartTime = req.StartTime,
                    EndTime = req.EndTime,
                    Capacity = req.Capacity,
                    IsActive = req.IsActive,
                    CreatedAt = DateTime.UtcNow
                };
                _context.TimeSlots.Add(newSlot);
            }
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "Lưu timeslot thành công" });
    }

    public class TimeSlotRequest
    {
        public string? Id { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int Capacity { get; set; }
        public bool IsActive { get; set; }
    }

    // ====================== OVERRIDE TIMESLOT ======================
    [HttpGet("overrides")]
    public async Task<IActionResult> GetOverrides([FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var store = await _context.Stores.FirstOrDefaultAsync(s => s.OwnerId == userId);
        if (store == null) return NotFound("Không tìm thấy cửa hàng");

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
    public async Task<IActionResult> CreateOverride([FromBody] TimeSlotOverrideRequest req)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var store = await _context.Stores.FirstOrDefaultAsync(s => s.OwnerId == userId);
        if (store == null) return NotFound("Không tìm thấy cửa hàng");

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var maxDate = today.AddMonths(6);

        if (req.Date < today) return BadRequest("Không thể tạo override cho ngày trong quá khứ");
        if (req.Date > maxDate) return BadRequest("Chỉ được tạo override tối đa 6 tháng tới");

        if (!req.IsFullDayClosure && req.StartTime.HasValue && req.EndTime.HasValue)
        {
            if (req.StartTime >= req.EndTime)
                return BadRequest("Giờ kết thúc phải lớn hơn giờ bắt đầu");
        }

        var ov = new TimeSlotOverride
        {
            StoreId = store.Id,
            TimeSlotId = req.TimeSlotId,
            Date = req.Date,
            StartTime = req.IsFullDayClosure ? null : req.StartTime,
            EndTime = req.IsFullDayClosure ? null : req.EndTime,
            Capacity = req.IsFullDayClosure ? null : req.Capacity,
            IsFullDayClosure = req.IsFullDayClosure,
            Reason = req.Reason ?? "Override thủ công",
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.TimeSlotOverrides.Add(ov);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Tạo override thành công" });
    }

    [HttpPut("overrides/{id}")]
    public async Task<IActionResult> UpdateOverride(Guid id, [FromBody] TimeSlotOverrideRequest req)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var ov = await _context.TimeSlotOverrides
            .FirstOrDefaultAsync(o => o.Id == id && o.StoreId == _context.Stores.First(s => s.OwnerId == userId).Id);

        if (ov == null) return NotFound("Không tìm thấy override");

        // Kiểm tra logic ngày tháng (giữ nguyên như cũ)
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var maxDate = today.AddMonths(6);

        if (req.Date < today) return BadRequest("Không thể đặt override cho ngày trong quá khứ");
        if (req.Date > maxDate) return BadRequest("Chỉ được tạo override tối đa 6 tháng tới");

        if (!req.IsFullDayClosure && req.StartTime.HasValue && req.EndTime.HasValue)
        {
            if (req.StartTime >= req.EndTime)
                return BadRequest("Giờ kết thúc phải lớn hơn giờ bắt đầu");
        }

        ov.Date = req.Date;
        ov.TimeSlotId = req.TimeSlotId;
        ov.StartTime = req.IsFullDayClosure ? null : req.StartTime;
        ov.EndTime = req.IsFullDayClosure ? null : req.EndTime;
        ov.Capacity = req.IsFullDayClosure ? null : req.Capacity;
        ov.IsFullDayClosure = req.IsFullDayClosure;
        ov.Reason = req.Reason;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Cập nhật override thành công" });
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