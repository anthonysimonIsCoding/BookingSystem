using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using BookingSystem.Data;
using BookingSystem.Entities.Enums;

namespace BookingSystem.Controllers.Admin;

//[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/stores")]
public class AdminStoreController : ControllerBase
{
    private readonly BookingDbContext _context;

    public AdminStoreController(BookingDbContext context)
    {
        _context = context;
    }

    // ==================== DANH SÁCH CỬA HÀNG ====================
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
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

        return Ok(new { items = stores, totalCount, page, pageSize, totalPages = (int)Math.Ceiling((double)totalCount / pageSize) });
    }

    // ==================== CHI TIẾT CỬA HÀNG ====================
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var store = await _context.Stores
            .Include(s => s.Owner)
            .Include(s => s.Images)
            .Include(s => s.StoreCategories).ThenInclude(sc => sc.Category)
            .Include(s => s.StoreSpecies).ThenInclude(ss => ss.Species)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (store == null) return NotFound(new { message = "Không tìm thấy cửa hàng" });

        return Ok(new
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
        });
    }

    // ==================== DỊCH VỤ (ĐÃ FIX CYCLE) ====================
    [HttpGet("{storeId}/services")]
    public async Task<IActionResult> GetServices(Guid storeId)
    {
        var services = await _context.Services
            .Where(s => s.StoreId == storeId)
            .Include(s => s.OptionGroups)
                .ThenInclude(g => g.Options)
            .AsNoTracking()
            .ToListAsync();

        // Project để tránh cycle
        var result = services.Select(s => new
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
        });

        return Ok(result);
    }

    [HttpPut("{storeId}/services/{serviceId}/status")]
    public async Task<IActionResult> ToggleServiceStatus(Guid storeId, Guid serviceId, [FromBody] ToggleServiceStatusRequest request)
    {
        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == serviceId && s.StoreId == storeId);

        if (service == null)
            return NotFound(new { message = "Không tìm thấy dịch vụ" });

        service.IsActive = request.IsActive;
        await _context.SaveChangesAsync();

        return Ok(new { message = $"Dịch vụ đã được {(request.IsActive ? "mở" : "khóa")}" });
    }

    // ==================== TIMESLOT, VOUCHER, BOOKINGS, REVIEWS ====================
    [HttpGet("{storeId}/timeslots")]
    public async Task<IActionResult> GetTimeSlots(Guid storeId) => Ok(await _context.TimeSlots.Where(t => t.StoreId == storeId).OrderBy(t => t.StartTime).ToListAsync());

    [HttpGet("{storeId}/timeslot-overrides")]
    public async Task<IActionResult> GetOverrides(Guid storeId) => Ok(await _context.TimeSlotOverrides.Where(o => o.StoreId == storeId).OrderByDescending(o => o.Date).ToListAsync());

    [HttpGet("{storeId}/vouchers")]
    public async Task<IActionResult> GetVouchers(Guid storeId) => Ok(await _context.StoreVouchers.Where(v => v.StoreId == storeId).ToListAsync());

    // ==================== ĐƠN HÀNG (ĐÃ FIX CYCLE + DUPLICATE NAME) ====================
    [HttpGet("{storeId}/bookings")]
    public async Task<IActionResult> GetBookings(Guid storeId)
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

        var result = bookings.Select(b => new
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
        });

        return Ok(result);
    }

    // ==================== ĐÁNH GIÁ ====================
    [HttpGet("{storeId}/reviews")]
    public async Task<IActionResult> GetReviews(Guid storeId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.StoreId == storeId)
            .Include(r => r.User)
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var result = reviews.Select(r => new
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
        });

        return Ok(result);
    }
}

// DTOs
public class UpdateStoreStatusRequest
{
    public StoreStatus Status { get; set; }
}

public class ToggleServiceStatusRequest
{
    public bool IsActive { get; set; }
}