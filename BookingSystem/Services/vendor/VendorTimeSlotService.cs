using BookingSystem.Data;
using BookingSystem.DTOs;
using BookingSystem.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingSystem.Services;

public class VendorTimeSlotService
{
    private readonly BookingDbContext _context;

    public VendorTimeSlotService(BookingDbContext context)
    {
        _context = context;
    }

    private async Task<Store> GetCurrentStoreAsync(Guid userId)
    {
        var store = await _context.Stores
            .FirstOrDefaultAsync(s => s.OwnerId == userId);

        if (store == null)
            throw new UnauthorizedAccessException("Không tìm thấy cửa hàng của bạn");

        return store;
    }

    // ====================== LẤY DANH SÁCH TIMESLOT CỐ ĐỊNH ======================
    public async Task<List<object>> GetTimeSlotsAsync(Guid userId)
    {
        var store = await GetCurrentStoreAsync(userId);

        return await _context.TimeSlots
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
            .ToListAsync<object>();
    }

    // ====================== LƯU / THÊM / SỬA TIMESLOT ======================
    public async Task SaveTimeSlotsAsync(Guid userId, List<TimeSlotRequest> requests)
    {
        var store = await GetCurrentStoreAsync(userId);

        foreach (var req in requests)
        {
            if (req.StartTime >= req.EndTime)
                throw new InvalidOperationException($"Khung giờ {req.StartTime} - {req.EndTime} không hợp lệ.");

            if (!string.IsNullOrEmpty(req.Id) && Guid.TryParse(req.Id, out Guid reqId))
            {
                // Cập nhật
                var existing = await _context.TimeSlots
                    .FirstOrDefaultAsync(t => t.Id == reqId && t.StoreId == store.Id);

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
                    throw new InvalidOperationException($"Khung giờ {req.StartTime} - {req.EndTime} đã tồn tại");

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
    }

    // ====================== LẤY OVERRIDE ======================
    public async Task<List<TimeSlotOverride>> GetOverridesAsync(Guid userId, DateOnly? fromDate, DateOnly? toDate)
    {
        var store = await GetCurrentStoreAsync(userId);

        var query = _context.TimeSlotOverrides.Where(o => o.StoreId == store.Id);

        if (fromDate.HasValue) query = query.Where(o => o.Date >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(o => o.Date <= toDate.Value);

        return await query
            .OrderByDescending(o => o.Date)
            .ThenBy(o => o.StartTime)
            .ToListAsync();
    }

    // ====================== TẠO OVERRIDE ======================
    public async Task CreateOverrideAsync(Guid userId, TimeSlotOverrideRequest req)
    {
        var store = await GetCurrentStoreAsync(userId);

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var maxDate = today.AddMonths(6);

        if (req.Date < today)
            throw new InvalidOperationException("Không thể tạo override cho ngày trong quá khứ");

        if (req.Date > maxDate)
            throw new InvalidOperationException("Chỉ được tạo override tối đa 6 tháng tới");

        if (!req.IsFullDayClosure && req.StartTime.HasValue && req.EndTime.HasValue)
        {
            if (req.StartTime >= req.EndTime)
                throw new InvalidOperationException("Giờ kết thúc phải lớn hơn giờ bắt đầu");
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
    }

    // ====================== CẬP NHẬT OVERRIDE ======================
    public async Task UpdateOverrideAsync(Guid userId, Guid overrideId, TimeSlotOverrideRequest req)
    {
        var store = await GetCurrentStoreAsync(userId);

        var ov = await _context.TimeSlotOverrides
            .FirstOrDefaultAsync(o => o.Id == overrideId && o.StoreId == store.Id);

        if (ov == null)
            throw new KeyNotFoundException("Không tìm thấy override");

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var maxDate = today.AddMonths(6);

        if (req.Date < today)
            throw new InvalidOperationException("Không thể đặt override cho ngày trong quá khứ");

        if (req.Date > maxDate)
            throw new InvalidOperationException("Chỉ được tạo override tối đa 6 tháng tới");

        if (!req.IsFullDayClosure && req.StartTime.HasValue && req.EndTime.HasValue)
        {
            if (req.StartTime >= req.EndTime)
                throw new InvalidOperationException("Giờ kết thúc phải lớn hơn giờ bắt đầu");
        }

        ov.Date = req.Date;
        ov.TimeSlotId = req.TimeSlotId;
        ov.StartTime = req.IsFullDayClosure ? null : req.StartTime;
        ov.EndTime = req.IsFullDayClosure ? null : req.EndTime;
        ov.Capacity = req.IsFullDayClosure ? null : req.Capacity;
        ov.IsFullDayClosure = req.IsFullDayClosure;
        ov.Reason = req.Reason;

        await _context.SaveChangesAsync();
    }
}