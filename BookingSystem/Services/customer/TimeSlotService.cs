using BookingSystem.Data;
using BookingSystem.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingSystem.Services;

public class TimeSlotService
{
    private readonly BookingDbContext _context;

    public TimeSlotService(BookingDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lấy danh sách time slot theo Store và ngày
    /// </summary>
    public async Task<List<object>> GetByStoreAsync(Guid storeId, DateOnly? date = null)
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
        {
            return new List<object>(); // Trả về mảng rỗng nếu cửa hàng đóng cả ngày
        }

        // 2. Lấy template time slots (sắp xếp theo giờ bắt đầu)
        var templates = await _context.TimeSlots
            .Where(s => s.StoreId == storeId && s.IsActive)
            .OrderBy(s => s.StartTime)
            .ToListAsync();

        // 3. Lấy các override theo ngày
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

            // Đếm số lượng booking đã đặt trong slot này (không tính Cancelled)
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

        return result;
    }
}