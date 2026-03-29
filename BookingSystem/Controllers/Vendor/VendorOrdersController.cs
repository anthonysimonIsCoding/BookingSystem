// Controllers/Vendor/VendorOrdersController.cs
using BookingSystem.Data;
using BookingSystem.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using BookingSystem.DTOs;

namespace BookingSystem.Controllers.Vendor;

[Authorize(Roles = "ServiceProvider")]
[ApiController]
[Route("api/vendor/orders")]
public class VendorOrdersController : ControllerBase
{
    private readonly BookingDbContext _context;

    public VendorOrdersController(BookingDbContext context)
    {
        _context = context;
    }

    public class VendorOrderDto
    {
        public Guid Id { get; set; }
        public string PetName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public DateOnly BookingDate { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;        // ← Thêm
        public BookingStatus Status { get; set; }
        public decimal TotalPrice { get; set; }
    }

    // ====================== LỊCH TUẦN (Hỗ trợ Override) ======================
    [HttpGet("calendar")]
    public async Task<IActionResult> GetCalendarOrders([FromQuery] DateOnly weekStart)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var store = await _context.Stores.FirstOrDefaultAsync(s => s.OwnerId == userId);
        if (store == null) return BadRequest("Không tìm thấy cửa hàng");

        var endDate = weekStart.AddDays(6);

        // 1. Lấy tất cả booking trong tuần
        var bookings = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Pet)
            .Include(b => b.TimeSlot)
            .Include(b => b.ServiceItems)
                .ThenInclude(si => si.ServiceOption)
                    .ThenInclude(o => o.OptionGroup!.Service)
            .Where(b => b.StoreId == store.Id
                     && b.BookingDate >= weekStart
                     && b.BookingDate <= endDate)
            .OrderBy(b => b.BookingDate)
            .ThenBy(b => b.TimeSlot.StartTime)
            .ToListAsync();

        // 2. Lấy tất cả override trong tuần
        var overrides = await _context.TimeSlotOverrides
            .Where(o => o.StoreId == store.Id
                     && o.Date >= weekStart
                     && o.Date <= endDate)
            .ToListAsync();

        var result = bookings.Select(b =>
        {
            // Tìm override chính xác nhất
            var ov = overrides.FirstOrDefault(o =>
                o.Date == b.BookingDate &&
                o.TimeSlotId == b.TimeSlotId);   // Ưu tiên override theo TimeSlotId cụ thể

            // Nếu không có override theo slot thì kiểm tra full day
            if (ov == null)
            {
                ov = overrides.FirstOrDefault(o =>
                    o.Date == b.BookingDate && o.IsFullDayClosure);
            }

            string effectiveStart = b.TimeSlot.StartTime.ToString(@"hh\:mm");
            string effectiveEnd = b.TimeSlot.EndTime.ToString(@"hh\:mm");

            if (ov != null)
            {
                if (ov.IsFullDayClosure)
                {
                    effectiveStart = "Đóng cửa";
                    effectiveEnd = "";
                }
                else
                {
                    if (ov.StartTime.HasValue) effectiveStart = ov.StartTime.Value.ToString(@"hh\:mm");
                    if (ov.EndTime.HasValue) effectiveEnd = ov.EndTime.Value.ToString(@"hh\:mm");
                }
            }

            var serviceItem = b.ServiceItems.FirstOrDefault();
            string serviceName = serviceItem?.ServiceOption?.OptionGroup?.Service?.Name ?? "Không xác định";

            return new VendorOrderDto
            {
                Id = b.Id,
                PetName = b.Pet?.Name ?? "Không có pet",
                CustomerName = b.User?.FullName ?? "Không rõ",
                ServiceName = serviceName,
                BookingDate = b.BookingDate,
                StartTime = effectiveStart,
                EndTime = effectiveEnd,
                Status = b.Status,
                TotalPrice = b.TotalPrice
            };
        }).ToList();

        return Ok(result);
    }
    // ====================== CHI TIẾT ĐƠN HÀNG ======================
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderDetail(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var store = await _context.Stores.FirstOrDefaultAsync(s => s.OwnerId == userId);
        if (store == null) return BadRequest("Không tìm thấy cửa hàng");

        var booking = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Pet)
                .ThenInclude(p => p.Species)
            .Include(b => b.Pet)
                .ThenInclude(p => p.Breed)
            .Include(b => b.TimeSlot)
            .Include(b => b.ServiceItems)
                .ThenInclude(si => si.ServiceOption)
                    .ThenInclude(o => o.OptionGroup!)
                        .ThenInclude(g => g.Service)
            .Include(b => b.PlatformVoucher)
            .Include(b => b.StoreVoucher)
            .FirstOrDefaultAsync(b => b.Id == id && b.StoreId == store.Id);

        if (booking == null) return NotFound("Không tìm thấy đơn hàng");

        var mainService = booking.ServiceItems.FirstOrDefault()?.ServiceOption?.OptionGroup?.Service?.Name ?? "Không xác định";

        var options = booking.ServiceItems.Select(si => new OrderOptionDto
        {
            OptionName = si.ServiceOption.Name,
            Price = si.Price
        }).ToList();

        var dto = new VendorOrderDetailDto
        {
            Id = booking.Id,
            BookingDate = booking.BookingDate,
            StartTime = booking.TimeSlot.StartTime.ToString(@"hh\:mm"),
            EndTime = booking.TimeSlot.EndTime.ToString(@"hh\:mm"),
            Status = booking.Status,
            TotalPrice = booking.TotalPrice,
            Notes = booking.Notes,

            PetName = booking.Pet.Name,
            PetSpecies = booking.Pet.Species.Name,
            PetBreed = booking.Pet.Breed?.Name,
            PetGender = booking.Pet.Gender,
            PetDateOfBirth = booking.Pet.DateOfBirth?.ToString("yyyy-MM-dd"),
            PetColor = booking.Pet.Color,
            PetWeight = booking.Pet.Weight,
            PetNotes = booking.Pet.Notes,
            PetProfileImageUrl = booking.Pet.ProfileImageUrl,

            CustomerName = booking.User.FullName,

            MainServiceName = mainService,
            Options = options,

            PlatformVoucherCode = booking.PlatformVoucher?.Code,
            StoreVoucherCode = booking.StoreVoucher?.Code,
            PlatformDiscount = booking.PlatformVoucherDiscount,
            StoreDiscount = booking.StoreVoucherDiscount
        };

        return Ok(dto);
    }
    // ====================== CẬP NHẬT TRẠNG THÁI ======================
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var store = await _context.Stores.FirstOrDefaultAsync(s => s.OwnerId == userId);
        if (store == null) return BadRequest("Không tìm thấy cửa hàng");

        var booking = await _context.Bookings
            .Include(b => b.TimeSlot)
            .FirstOrDefaultAsync(b => b.Id == id && b.StoreId == store.Id);

        if (booking == null) return NotFound("Không tìm thấy đơn hàng");

        // Không cho thay đổi trạng thái của đơn đã hoàn thành hoặc đã hủy
        if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
            return BadRequest("Không thể thay đổi trạng thái của đơn đã hoàn thành hoặc đã hủy");

        // Kiểm tra thời gian (chỉ cho phép update khi đã đến giờ, trừ khi hủy)
        var slotTime = booking.BookingDate.ToDateTime(TimeOnly.FromTimeSpan(booking.TimeSlot.StartTime));
        if (slotTime > DateTime.UtcNow && request.Status != BookingStatus.Cancelled)
            return BadRequest("Chưa đến thời gian thực hiện đơn hàng");

        booking.Status = request.Status;
        booking.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Cập nhật trạng thái thành công", newStatus = booking.Status });
    }
    // ====================== CÁC TAB KHÁC ======================
    // ====================== CÁC TAB KHÁC ======================
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingOrders() => await GetOrdersByStatus(new[] { BookingStatus.Pending, BookingStatus.Received, BookingStatus.Caring, BookingStatus.WaitingPickup });

    [HttpGet("completed")]
    public async Task<IActionResult> GetCompletedOrders() => await GetOrdersByStatus(new[] { BookingStatus.Completed });

    [HttpGet("cancelled")]
    public async Task<IActionResult> GetCancelledOrders() => await GetOrdersByStatus(new[] { BookingStatus.Cancelled });

    // ====================== HÀM CHUNG (ĐÃ SỬA) ======================
    private async Task<IActionResult> GetOrdersByStatus(BookingStatus[] statuses)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var store = await _context.Stores.FirstOrDefaultAsync(s => s.OwnerId == userId);
        if (store == null) return BadRequest("Không tìm thấy cửa hàng");

        var orders = await _context.Bookings
            .Include(b => b.User)           // ← Quan trọng: lấy CustomerName
            .Include(b => b.Pet)
            .Include(b => b.TimeSlot)
            .Include(b => b.ServiceItems)
                .ThenInclude(si => si.ServiceOption)
                    .ThenInclude(o => o.OptionGroup!.Service)
            .Where(b => b.StoreId == store.Id && statuses.Contains(b.Status))
            .OrderByDescending(b => b.BookingDate)
            .ThenBy(b => b.TimeSlot.StartTime)
            .ToListAsync();   // Load trước để xử lý override

        // Lấy override cho các ngày có đơn
        var dates = orders.Select(b => b.BookingDate).Distinct().ToList();
        var overrides = await _context.TimeSlotOverrides
            .Where(o => o.StoreId == store.Id && dates.Contains(o.Date))
            .ToListAsync();

        var result = orders.Select(b =>
        {
            // Xử lý Override
            var ov = overrides.FirstOrDefault(o =>
                o.Date == b.BookingDate &&
                (o.TimeSlotId == b.TimeSlotId || o.TimeSlotId == null));

            string effectiveStart = b.TimeSlot.StartTime.ToString(@"hh\:mm");
            string effectiveEnd = b.TimeSlot.EndTime.ToString(@"hh\:mm");

            if (ov != null)
            {
                if (ov.IsFullDayClosure)
                {
                    effectiveStart = "Đóng cửa";
                    effectiveEnd = "";
                }
                else
                {
                    if (ov.StartTime.HasValue) effectiveStart = ov.StartTime.Value.ToString(@"hh\:mm");
                    if (ov.EndTime.HasValue) effectiveEnd = ov.EndTime.Value.ToString(@"hh\:mm");
                }
            }

            var serviceItem = b.ServiceItems.FirstOrDefault();
            string serviceName = serviceItem?.ServiceOption?.OptionGroup?.Service?.Name ?? "Không xác định";

            return new VendorOrderDto
            {
                Id = b.Id,
                PetName = b.Pet?.Name ?? "Không có pet",
                CustomerName = b.User?.FullName ?? "Không rõ",
                ServiceName = serviceName,
                BookingDate = b.BookingDate,
                StartTime = effectiveStart,
                EndTime = effectiveEnd,
                Status = b.Status,
                TotalPrice = b.TotalPrice
            };
        }).ToList();

        return Ok(result);
    }


}