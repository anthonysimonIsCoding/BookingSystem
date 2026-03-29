using BookingSystem.Data;
using BookingSystem.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingSystem.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BookingSystem.DTOs;

namespace BookingSystem.Controllers;

[Authorize(Roles = "Customer")]
[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly BookingDbContext _context;

    public BookingsController(BookingDbContext context)
    {
        _context = context;
    }

    // ====================== CREATE BOOKING (FULLY UPDATED) ======================
    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr == null) return Unauthorized();
        var userId = Guid.Parse(userIdStr);

        // 1. Kiểm tra TimeSlot
        var slot = await _context.TimeSlots.FindAsync(request.TimeSlotId);
        if (slot == null) return NotFound("Time slot not found");

        var bookingDateTime = request.BookingDate.ToDateTime(TimeOnly.FromTimeSpan(slot.StartTime));
        if (bookingDateTime < DateTime.UtcNow)
            return BadRequest("Không thể đặt lịch trong quá khứ");

        // 2. Kiểm tra Pet tồn tại
        var pet = await _context.Pets.Include(p => p.Species)
            .FirstOrDefaultAsync(p => p.Id == request.PetId && p.UserId == userId);
        if (pet == null) return BadRequest("Pet không hợp lệ");

        // ====================== KIỂM TRA PET CÓ ĐƠN ĐANG XỬ LÝ KHÔNG ======================
        var activeBooking = await _context.Bookings
            .Where(b => b.PetId == request.PetId
                     && b.Status != BookingStatus.Completed
                     && b.Status != BookingStatus.Cancelled)
            .FirstOrDefaultAsync();

        if (activeBooking != null)
        {
            return BadRequest(
                $"Pet này đang có đơn đang xử lý (trạng thái: {activeBooking.Status}). " +
                "Vui lòng chờ đơn cũ hoàn thành hoặc hủy trước khi đặt lịch mới.");
        }

        // 3. Store có nhận loài thú không
        var storeAcceptsSpecies = await _context.StoreSpecies
            .AnyAsync(ss => ss.StoreId == request.StoreId && ss.SpeciesId == pet.SpeciesId);
        if (!storeAcceptsSpecies)
            return BadRequest($"Cửa hàng không nhận {pet.Species.Name}");

        // 4. Slot còn chỗ không
        var bookedCount = await _context.Bookings
            .CountAsync(b => b.TimeSlotId == request.TimeSlotId
                          && b.BookingDate == request.BookingDate
                          && b.Status != BookingStatus.Cancelled);
        if (bookedCount >= slot.Capacity)
            return BadRequest("Khung giờ đã đầy");

        // 5. Load Options + kiểm tra tất cả thuộc cùng 1 Service
        var selectedOptions = await _context.ServiceOptions
            .Include(o => o.OptionGroup!)
                .ThenInclude(g => g.Service)
            .Where(o => request.ServiceOptionIds.Contains(o.Id) && o.IsActive)
            .ToListAsync();

        if (selectedOptions.Count != request.ServiceOptionIds.Count)
            return BadRequest("Một số option không hợp lệ");

        var distinctServiceIds = selectedOptions.Select(o => o.OptionGroup!.ServiceId).Distinct().ToList();
        if (distinctServiceIds.Count > 1)
            return BadRequest("Chỉ được chọn option thuộc cùng một dịch vụ");

        var service = selectedOptions.First().OptionGroup!.Service;

        // 6. Validate ServiceType và OptionGroupType
        if (service.Type == ServiceType.Single && selectedOptions.Count > 1)
            return BadRequest("Dịch vụ này chỉ cho phép chọn 1 option");

        foreach (var group in service.OptionGroups)
        {
            var selectedInGroup = selectedOptions.Count(o => o.OptionGroupId == group.Id);
            if (group.Type == OptionGroupType.SingleChoice && selectedInGroup > 1)
                return BadRequest($"Group '{group.Name}' chỉ được chọn 1 option");
            if (group.IsRequired && selectedInGroup == 0)
                return BadRequest($"Group '{group.Name}' là bắt buộc");
        }

        // Tính giá gốc
        decimal basePrice = service.Price;
        decimal optionsPrice = selectedOptions.Sum(o => o.Price);
        decimal totalBeforeDiscount = basePrice + optionsPrice;

        // Voucher logic (giữ nguyên như cũ)
        decimal platformDiscount = 0, storeDiscount = 0;
        Guid? platformVoucherId = null, storeVoucherId = null;

        if (!string.IsNullOrWhiteSpace(request.PlatformVoucherCode))
        {
            var pv = await _context.PlatformVouchers.FirstOrDefaultAsync(v =>
                v.Code == request.PlatformVoucherCode && v.IsActive &&
                v.StartDate <= DateTime.UtcNow && (v.EndDate == null || v.EndDate >= DateTime.UtcNow));

            if (pv != null && totalBeforeDiscount >= (pv.MinOrderValue ?? 0))
            {
                platformDiscount = pv.DiscountType == VoucherDiscountType.Percent
                    ? Math.Min(totalBeforeDiscount * pv.DiscountValue / 100, pv.MaxDiscountAmount ?? decimal.MaxValue)
                    : Math.Min(pv.DiscountValue, pv.MaxDiscountAmount ?? decimal.MaxValue);
                platformVoucherId = pv.Id;
            }
            else return BadRequest("Voucher sàn không hợp lệ hoặc không đủ điều kiện");
        }

        if (!string.IsNullOrWhiteSpace(request.StoreVoucherCode))
        {
            var sv = await _context.StoreVouchers.FirstOrDefaultAsync(v =>
                v.Code == request.StoreVoucherCode && v.StoreId == request.StoreId && v.IsActive &&
                v.StartDate <= DateTime.UtcNow && (v.EndDate == null || v.EndDate >= DateTime.UtcNow));

            if (sv != null && totalBeforeDiscount >= (sv.MinOrderValue ?? 0))
            {
                if (sv.ApplicableServiceId.HasValue && sv.ApplicableServiceId != service.Id)
                    return BadRequest("Voucher shop không áp dụng cho dịch vụ này");

                storeDiscount = sv.DiscountType == VoucherDiscountType.Percent
                    ? Math.Min(totalBeforeDiscount * sv.DiscountValue / 100, sv.MaxDiscountAmount ?? decimal.MaxValue)
                    : Math.Min(sv.DiscountValue, sv.MaxDiscountAmount ?? decimal.MaxValue);
                storeVoucherId = sv.Id;
            }
            else return BadRequest("Voucher shop không hợp lệ hoặc không đủ điều kiện");
        }

        decimal finalPrice = totalBeforeDiscount - platformDiscount - storeDiscount;

        // Tạo Booking
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            StoreId = request.StoreId,
            TimeSlotId = request.TimeSlotId,
            PetId = request.PetId,
            BookingDate = request.BookingDate,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            TotalPrice = finalPrice,
            Notes = request.Notes,
            PlatformVoucherId = platformVoucherId,
            StoreVoucherId = storeVoucherId,
            PlatformVoucherDiscount = platformDiscount > 0 ? platformDiscount : null,
            StoreVoucherDiscount = storeDiscount > 0 ? storeDiscount : null
        };

        _context.Bookings.Add(booking);

        foreach (var opt in selectedOptions)
        {
            _context.BookingServiceItems.Add(new BookingServiceItem
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                ServiceOptionId = opt.Id,
                Price = opt.Price,
                DurationMinutes = opt.DurationMinutes
            });
        }

        if (platformVoucherId.HasValue)
        {
            _context.UsedVouchers.Add(new UsedVoucher
            {
                Id = Guid.NewGuid(),
                PlatformVoucherId = platformVoucherId.Value,
                UserId = userId,
                BookingId = booking.Id,
                DiscountApplied = platformDiscount,
                UsedAt = DateTime.UtcNow
            });
        }

        if (storeVoucherId.HasValue)
        {
            _context.UsedVouchers.Add(new UsedVoucher
            {
                Id = Guid.NewGuid(),
                StoreVoucherId = storeVoucherId.Value,
                UserId = userId,
                BookingId = booking.Id,
                DiscountApplied = storeDiscount,
                UsedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();

        return Ok(new { booking.Id, TotalPrice = finalPrice });
    }

    // ====================== HỦY ĐƠN ======================
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelBooking(Guid id, [FromBody] CancelBookingRequest? request = null)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr == null) return Unauthorized();
        var userId = Guid.Parse(userIdStr);

        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

        if (booking == null) return NotFound("Không tìm thấy đơn hàng hoặc bạn không có quyền hủy");

        if (booking.Status != BookingStatus.Pending)
            return BadRequest("Chỉ có thể hủy đơn khi đơn đang ở trạng thái Pending");

        booking.Status = BookingStatus.Cancelled;
        booking.CancelledAt = DateTime.UtcNow;
        booking.CancellationReason = request?.Reason ?? "Khách hàng hủy";

        await _context.SaveChangesAsync();

        return Ok(new { message = "Hủy đơn thành công" });
    }

    // ====================== GET BOOKINGS BY DATE ======================
    [HttpGet]
    public async Task<IActionResult> GetByDate(DateOnly? date)
    {
        var query = _context.Bookings
            .Include(b => b.User)
            .Include(b => b.TimeSlot)
            .AsQueryable();

        if (date.HasValue)
            query = query.Where(b => b.BookingDate == date.Value);

        var result = await query.ToListAsync();
        return Ok(result);
    }

    // ====================== GET PET BOOKING HISTORY ======================
    [HttpGet("pet/{petId}")]
    public async Task<IActionResult> GetPetHistory(Guid petId)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr == null) return Unauthorized();

        var userId = Guid.Parse(userIdStr);

        var history = await _context.Bookings
            .Where(b => b.PetId == petId && b.UserId == userId)
            .Include(b => b.Store)
            .Include(b => b.TimeSlot)
            .Include(b => b.ServiceItems)
                .ThenInclude(i => i.ServiceOption)
            .OrderByDescending(b => b.BookingDate)
            .Select(b => new
            {
                b.Id,
                StoreName = b.Store.Name,
                ServiceNames = b.ServiceItems
                    .Select(i => i.ServiceOption.Name)
                    .ToList(),
                BookingDate = b.BookingDate,
                StartTime = b.TimeSlot.StartTime.ToString(@"hh\:mm"),
                Status = b.Status,
                TotalPrice = b.TotalPrice
            })
            .ToListAsync();

        return Ok(history);
    }

    // ====================== SERVER TIME (VN TIMEZONE) ======================
    [HttpGet("server-time")]
    public IActionResult GetServerTime()
    {
        var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);

        return Ok(new
        {
            utcTime = DateTime.UtcNow,
            serverLocalTime = DateTime.Now,
            vietnamTime = vietnamTime,
            vietnamTimeString = vietnamTime.ToString("yyyy-MM-dd HH:mm:ss")
        });
    }
}