using BookingSystem.Data;
using BookingSystem.Entities;
using BookingSystem.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BookingSystem.DTOs;
namespace BookingSystem.Services;

public class VendorOrderService
{
    private readonly BookingDbContext _context;

    public VendorOrderService(BookingDbContext context)
    {
        _context = context;
    }

    private async Task<Store> GetCurrentStoreAsync(Guid userId)
    {
        var store = await _context.Stores.FirstOrDefaultAsync(s => s.OwnerId == userId);
        if (store == null)
            throw new UnauthorizedAccessException("Không tìm thấy cửa hàng của bạn");
        return store;
    }

    // ====================== LỊCH TUẦN (Calendar) ======================
    public async Task<List<VendorOrderDto>> GetCalendarOrdersAsync(Guid userId, DateOnly weekStart)
    {
        var store = await GetCurrentStoreAsync(userId);
        var endDate = weekStart.AddDays(6);

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

        var overrides = await _context.TimeSlotOverrides
            .Where(o => o.StoreId == store.Id
                     && o.Date >= weekStart
                     && o.Date <= endDate)
            .ToListAsync();

        return bookings.Select(b => MapToVendorOrderDto(b, overrides)).ToList();
    }

    // ====================== LẤY ĐƠN THEO TRẠNG THÁI (Pending, Completed, Cancelled) ======================
    public async Task<List<VendorOrderDto>> GetOrdersByStatusAsync(Guid userId, BookingStatus[] statuses)
    {
        var store = await GetCurrentStoreAsync(userId);

        var orders = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Pet)
            .Include(b => b.TimeSlot)
            .Include(b => b.ServiceItems)
                .ThenInclude(si => si.ServiceOption)
                    .ThenInclude(o => o.OptionGroup!.Service)
            .Where(b => b.StoreId == store.Id && statuses.Contains(b.Status))
            .OrderByDescending(b => b.BookingDate)
            .ThenBy(b => b.TimeSlot.StartTime)
            .ToListAsync();

        var dates = orders.Select(b => b.BookingDate).Distinct().ToList();
        var overrides = await _context.TimeSlotOverrides
            .Where(o => o.StoreId == store.Id && dates.Contains(o.Date))
            .ToListAsync();

        return orders.Select(b => MapToVendorOrderDto(b, overrides)).ToList();
    }

    // ====================== CHI TIẾT ĐƠN HÀNG ======================
    public async Task<VendorOrderDetailDto> GetOrderDetailAsync(Guid userId, Guid bookingId)
    {
        var store = await GetCurrentStoreAsync(userId);

        var booking = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Pet).ThenInclude(p => p.Species)
            .Include(b => b.Pet).ThenInclude(p => p.Breed)
            .Include(b => b.TimeSlot)
            .Include(b => b.ServiceItems)
                .ThenInclude(si => si.ServiceOption)
                    .ThenInclude(o => o.OptionGroup!.Service)
            .Include(b => b.PlatformVoucher)
            .Include(b => b.StoreVoucher)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.StoreId == store.Id);

        if (booking == null)
            throw new KeyNotFoundException("Không tìm thấy đơn hàng");

        return MapToVendorOrderDetailDto(booking);
    }

    // ====================== CẬP NHẬT TRẠNG THÁI ======================
    public async Task UpdateOrderStatusAsync(Guid userId, Guid bookingId, BookingStatus newStatus)
    {
        var store = await GetCurrentStoreAsync(userId);

        var booking = await _context.Bookings
            .Include(b => b.TimeSlot)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.StoreId == store.Id);

        if (booking == null)
            throw new KeyNotFoundException("Không tìm thấy đơn hàng");

        if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
            throw new InvalidOperationException("Không thể thay đổi trạng thái của đơn đã hoàn thành hoặc đã hủy");

        var slotTime = booking.BookingDate.ToDateTime(TimeOnly.FromTimeSpan(booking.TimeSlot.StartTime));
        if (slotTime > DateTime.UtcNow && newStatus != BookingStatus.Cancelled)
            throw new InvalidOperationException("Chưa đến thời gian thực hiện đơn hàng");

        booking.Status = newStatus;
        booking.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    // ====================== HELPER METHODS ======================
    private VendorOrderDto MapToVendorOrderDto(Booking b, List<TimeSlotOverride> overrides)
    {
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
    }

    private VendorOrderDetailDto MapToVendorOrderDetailDto(Booking booking)
    {
        var mainService = booking.ServiceItems.FirstOrDefault()?.ServiceOption?.OptionGroup?.Service?.Name
                          ?? "Không xác định";

        var options = booking.ServiceItems.Select(si => new OrderOptionDto
        {
            OptionName = si.ServiceOption.Name,
            Price = si.Price
        }).ToList();

        return new VendorOrderDetailDto
        {
            Id = booking.Id,
            BookingDate = booking.BookingDate,
            StartTime = booking.TimeSlot.StartTime.ToString(@"hh\:mm"),
            EndTime = booking.TimeSlot.EndTime.ToString(@"hh\:mm"),
            Status = booking.Status,
            TotalPrice = booking.TotalPrice,
            Notes = booking.Notes,

            PetName = booking.Pet.Name,
            PetSpecies = booking.Pet.Species?.Name,
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
    }
}