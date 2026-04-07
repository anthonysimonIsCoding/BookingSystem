using BookingSystem.Data;
using BookingSystem.DTOs;
using BookingSystem.Entities;
using BookingSystem.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingSystem.Services;

public class AdminStoreService
{
    private readonly BookingDbContext _context;

    public AdminStoreService(BookingDbContext context)
    {
        _context = context;
    }

    // ==================== DANH SÁCH CỬA HÀNG (CÓ PHÂN TRANG + FILTER) ====================
    public async Task<object> GetAllAsync(string? search, int? status, int page = 1, int pageSize = 20)
    {
        var query = _context.Stores.Include(s => s.Owner).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(s => s.Name.Contains(search) || s.Address.Contains(search));

        if (status.HasValue)
            query = query.Where(s => s.Status == (StoreStatus)status.Value);

        var totalCount = await query.CountAsync();

        var stores = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new
            {
                s.Id,
                s.Name,
                OwnerName = s.Owner != null ? s.Owner.FullName : "Chưa có chủ",
                OwnerEmail = s.Owner != null ? s.Owner.Email : "N/A",
                s.Address,
                s.Status,
                s.AverageRating,
                s.ReviewCount,
                s.TotalCompletedBookings,
                s.CreatedAt
            })
            .ToListAsync();

        return new
        {
            items = stores,
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    // ==================== CHI TIẾT CỬA HÀNG ====================
    public async Task<object?> GetByIdAsync(Guid id)
    {
        var store = await _context.Stores
            .Include(s => s.Owner)
            .Include(s => s.Images)
            .Include(s => s.StoreCategories).ThenInclude(sc => sc.Category)
            .Include(s => s.StoreSpecies).ThenInclude(ss => ss.Species)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (store == null) return null;

        return new
        {
            store.Id,
            store.Name,
            store.Address,
            store.Latitude,
            store.Longitude,
            store.AverageRating,
            store.ReviewCount,
            store.TotalCompletedBookings,
            store.Status,
            store.CreatedAt,
            store.UpdatedAt,
            OwnerName = store.Owner?.FullName,
            OwnerEmail = store.Owner?.Email,
            Images = store.Images.OrderBy(i => i.Order).Select(i => new { i.Id, i.ImageUrl, i.IsThumbnail, i.Order }),
            Categories = store.StoreCategories.Select(sc => new { sc.CategoryId, sc.Category.Name }),
            Species = store.StoreSpecies.Select(ss => new { ss.SpeciesId, ss.Species.Name })
        };
    }

    // ==================== LẤY DANH SÁCH DỊCH VỤ CỦA STORE ====================
    public async Task<List<object>> GetServicesAsync(Guid storeId)
    {
        var services = await _context.Services
            .Where(s => s.StoreId == storeId)
            .Include(s => s.OptionGroups)
                .ThenInclude(g => g.Options)
            .AsNoTracking()
            .ToListAsync();

        return services.Select(s => new
        {
            s.Id,
            s.Name,
            s.Description,
            s.Price,
            s.DurationMinutes,
            s.Type,
            s.IsActive,
            OptionGroups = s.OptionGroups.Select(g => new
            {
                g.Id,
                g.Name,
                g.Type,
                g.IsRequired,
                Options = g.Options.Select(o => new
                {
                    o.Id,
                    o.Name,
                    o.Price,
                    o.DurationMinutes,
                    o.IsActive
                }).ToList()
            }).ToList()
        }).ToList<object>();
    }

    // ==================== BẬT/TẮT DỊCH VỤ ====================
    public async Task ToggleServiceStatusAsync(Guid storeId, Guid serviceId, bool isActive)
    {
        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == serviceId && s.StoreId == storeId);

        if (service == null)
            throw new KeyNotFoundException("Không tìm thấy dịch vụ");

        service.IsActive = isActive;
        await _context.SaveChangesAsync();
    }

    // ==================== CÁC PHẦN KHÁC (TIMESLOT, VOUCHER, BOOKINGS, REVIEWS) ====================

    public async Task<List<TimeSlot>> GetTimeSlotsAsync(Guid storeId)
    {
        return await _context.TimeSlots
            .Where(t => t.StoreId == storeId)
            .OrderBy(t => t.StartTime)
            .ToListAsync();
    }

    public async Task<List<TimeSlotOverride>> GetTimeSlotOverridesAsync(Guid storeId)
    {
        return await _context.TimeSlotOverrides
            .Where(o => o.StoreId == storeId)
            .OrderByDescending(o => o.Date)
            .ToListAsync();
    }

    public async Task<List<StoreVoucher>> GetVouchersAsync(Guid storeId)
    {
        return await _context.StoreVouchers
            .Where(v => v.StoreId == storeId)
            .ToListAsync();
    }

    public async Task<List<object>> GetBookingsAsync(Guid storeId)
    {
        var bookings = await _context.Bookings
            .Where(b => b.StoreId == storeId)
            .Include(b => b.User)
            .Include(b => b.Pet)
            .Include(b => b.TimeSlot)
            .Include(b => b.ServiceItems)
                .ThenInclude(si => si.ServiceOption)
                    .ThenInclude(o => o.OptionGroup!)
                        .ThenInclude(g => g.Service)
            .AsNoTracking()
            .OrderByDescending(b => b.BookingDate)
            .ThenBy(b => b.TimeSlot.StartTime)
            .ToListAsync();

        return bookings.Select(b => new
        {
            b.Id,
            b.BookingDate,
            b.TotalPrice,
            b.Status,
            b.Notes,

            Customer = new
            {
                b.User.Id,
                FullName = b.User.FullName,
                b.User.Email,
                b.User.PhoneNumber
            },

            Pet = new
            {
                b.Pet.Id,
                b.Pet.Name,
                Species = b.Pet.Species?.Name,
                Breed = b.Pet.Breed?.Name,
                b.Pet.Gender,
                b.Pet.Weight
            },

            TimeSlot = new
            {
                b.TimeSlot.StartTime,
                b.TimeSlot.EndTime
            },

            Services = b.ServiceItems.Select(si => new
            {
                ServiceName = si.ServiceOption?.OptionGroup?.Service?.Name,
                OptionName = si.ServiceOption?.Name,
                si.Price,
                si.DurationMinutes
            }).ToList()
        }).ToList<object>();
    }

    public async Task<List<object>> GetReviewsAsync(Guid storeId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.StoreId == storeId)
            .Include(r => r.User)
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return reviews.Select(r => new
        {
            r.Id,
            r.Rating,
            r.Comment,
            r.CreatedAt,
            Customer = new
            {
                r.User.Id,
                FullName = r.User.FullName,
                r.User.Email
            }
        }).ToList<object>();
    }
}