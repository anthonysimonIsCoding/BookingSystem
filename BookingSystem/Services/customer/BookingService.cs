using BookingSystem.Data;
using BookingSystem.Entities;
using BookingSystem.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using BookingSystem.DTOs;

namespace BookingSystem.Services;

public class BookingService
{
    private readonly BookingDbContext _context;

    public BookingService(BookingDbContext context)
    {
        _context = context;
    }

    // ====================== TẠO BOOKING ======================
    public async Task<Booking> CreateBookingAsync(CreateBookingRequest request, Guid userId)
    {
        // 1. Kiểm tra TimeSlot tồn tại
        var timeSlot = await _context.TimeSlots.FindAsync(request.TimeSlotId);
        if (timeSlot == null)
            throw new InvalidOperationException("Khung giờ không tồn tại");

        var bookingDateTime = request.BookingDate.ToDateTime(TimeOnly.FromTimeSpan(timeSlot.StartTime));
        if (bookingDateTime < DateTime.UtcNow)
            throw new InvalidOperationException("Không thể đặt lịch trong quá khứ");

        // 2. Kiểm tra Pet thuộc về user
        var pet = await _context.Pets
            .Include(p => p.Species)
            .FirstOrDefaultAsync(p => p.Id == request.PetId && p.UserId == userId);

        if (pet == null)
            throw new InvalidOperationException("Thú cưng không hợp lệ hoặc không thuộc về bạn");

        // 3. Kiểm tra pet không có đơn đang xử lý
        var activeBooking = await _context.Bookings
            .AnyAsync(b => b.PetId == request.PetId &&
                           b.Status != BookingStatus.Completed &&
                           b.Status != BookingStatus.Cancelled);

        if (activeBooking)
            throw new InvalidOperationException("Pet này đang có đơn đang xử lý. Vui lòng chờ hoàn thành hoặc hủy trước.");

        // 4. Kiểm tra cửa hàng có nhận loài thú này không
        var storeAcceptsSpecies = await _context.StoreSpecies
            .AnyAsync(ss => ss.StoreId == request.StoreId && ss.SpeciesId == pet.SpeciesId);

        if (!storeAcceptsSpecies)
            throw new InvalidOperationException($"Cửa hàng không nhận loài {pet.Species.Name}");

        // 5. Kiểm tra slot còn chỗ không
        var bookedCount = await _context.Bookings
            .CountAsync(b => b.TimeSlotId == request.TimeSlotId &&
                             b.BookingDate == request.BookingDate &&
                             b.Status != BookingStatus.Cancelled);

        if (bookedCount >= timeSlot.Capacity)
            throw new InvalidOperationException("Khung giờ đã đầy");

        // 6. Lấy danh sách options được chọn
        var selectedOptions = await _context.ServiceOptions
            .Include(o => o.OptionGroup!)
                .ThenInclude(g => g.Service)
            .Where(o => request.ServiceOptionIds.Contains(o.Id) && o.IsActive)
            .ToListAsync();

        if (selectedOptions.Count != request.ServiceOptionIds.Count)
            throw new InvalidOperationException("Một số tùy chọn dịch vụ không hợp lệ");

        // Kiểm tra tất cả options thuộc cùng 1 dịch vụ
        var distinctServiceIds = selectedOptions.Select(o => o.OptionGroup!.ServiceId).Distinct().ToList();
        if (distinctServiceIds.Count > 1)
            throw new InvalidOperationException("Chỉ được chọn các option thuộc cùng một dịch vụ");

        var service = selectedOptions.First().OptionGroup!.Service;

        // Validate theo loại dịch vụ
        if (service.Type == ServiceType.Single && selectedOptions.Count > 1)
            throw new InvalidOperationException("Dịch vụ này chỉ cho phép chọn 1 option");

        foreach (var group in service.OptionGroups)
        {
            var selectedInGroup = selectedOptions.Count(o => o.OptionGroupId == group.Id);
            if (group.Type == OptionGroupType.SingleChoice && selectedInGroup > 1)
                throw new InvalidOperationException($"Nhóm '{group.Name}' chỉ được chọn 1 option");
            if (group.IsRequired && selectedInGroup == 0)
                throw new InvalidOperationException($"Nhóm '{group.Name}' là bắt buộc");
        }

        // Tính giá gốc
        decimal basePrice = service.Price;
        decimal optionsPrice = selectedOptions.Sum(o => o.Price);
        decimal totalBeforeDiscount = basePrice + optionsPrice;

        // ====================== XỬ LÝ VOUCHER ======================
        decimal platformDiscount = 0, storeDiscount = 0;
        Guid? platformVoucherId = null, storeVoucherId = null;

        if (!string.IsNullOrWhiteSpace(request.PlatformVoucherCode))
        {
            var pv = await _context.PlatformVouchers
                .FirstOrDefaultAsync(v => v.Code == request.PlatformVoucherCode && v.IsActive &&
                    v.StartDate <= DateTime.UtcNow && (v.EndDate == null || v.EndDate >= DateTime.UtcNow));

            if (pv != null && totalBeforeDiscount >= (pv.MinOrderValue ?? 0))
            {
                platformDiscount = pv.DiscountType == VoucherDiscountType.Percent
                    ? Math.Min(totalBeforeDiscount * pv.DiscountValue / 100, pv.MaxDiscountAmount ?? decimal.MaxValue)
                    : Math.Min(pv.DiscountValue, pv.MaxDiscountAmount ?? decimal.MaxValue);

                platformVoucherId = pv.Id;
            }
            else
                throw new InvalidOperationException("Voucher sàn không hợp lệ hoặc không đủ điều kiện");
        }

        if (!string.IsNullOrWhiteSpace(request.StoreVoucherCode))
        {
            var sv = await _context.StoreVouchers
                .FirstOrDefaultAsync(v => v.Code == request.StoreVoucherCode && v.StoreId == request.StoreId && v.IsActive &&
                    v.StartDate <= DateTime.UtcNow && (v.EndDate == null || v.EndDate >= DateTime.UtcNow));

            if (sv != null && totalBeforeDiscount >= (sv.MinOrderValue ?? 0))
            {
                if (sv.ApplicableServiceId.HasValue && sv.ApplicableServiceId != service.Id)
                    throw new InvalidOperationException("Voucher của cửa hàng không áp dụng cho dịch vụ này");

                storeDiscount = sv.DiscountType == VoucherDiscountType.Percent
                    ? Math.Min(totalBeforeDiscount * sv.DiscountValue / 100, sv.MaxDiscountAmount ?? decimal.MaxValue)
                    : Math.Min(sv.DiscountValue, sv.MaxDiscountAmount ?? decimal.MaxValue);

                storeVoucherId = sv.Id;
            }
            else
                throw new InvalidOperationException("Voucher của cửa hàng không hợp lệ hoặc không đủ điều kiện");
        }

        decimal finalPrice = totalBeforeDiscount - platformDiscount - storeDiscount;

        // ====================== TẠO BOOKING ======================
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            StoreId = request.StoreId,
            TimeSlotId = request.TimeSlotId,
            PetId = request.PetId,
            BookingDate = request.BookingDate,
            Status = BookingStatus.Pending,
            TotalPrice = finalPrice,
            Notes = request.Notes,
            PlatformVoucherId = platformVoucherId,
            StoreVoucherId = storeVoucherId,
            PlatformVoucherDiscount = platformDiscount > 0 ? platformDiscount : null,
            StoreVoucherDiscount = storeDiscount > 0 ? storeDiscount : null,
            CreatedAt = DateTime.UtcNow
        };

        _context.Bookings.Add(booking);

        // Thêm BookingServiceItem
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

        // Thêm UsedVoucher
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

        return booking;
    }

    // ====================== HỦY BOOKING ======================
    public async Task CancelBookingAsync(Guid bookingId, Guid userId, string? reason = null)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

        if (booking == null)
            throw new InvalidOperationException("Không tìm thấy đơn hàng hoặc bạn không có quyền hủy");

        if (booking.Status != BookingStatus.Pending)
            throw new InvalidOperationException("Chỉ có thể hủy đơn khi đang ở trạng thái Pending");

        booking.Status = BookingStatus.Cancelled;
        booking.CancelledAt = DateTime.UtcNow;
        booking.CancellationReason = reason ?? "Khách hàng hủy";

        await _context.SaveChangesAsync();
    }

    // ====================== LẤY LỊCH SỬ BOOKING CỦA PET ======================
    public async Task<List<object>> GetPetBookingHistoryAsync(Guid petId, Guid userId)
    {
        return await _context.Bookings
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
                ServiceNames = b.ServiceItems.Select(i => i.ServiceOption.Name).ToList(),
                b.BookingDate,
                StartTime = b.TimeSlot.StartTime.ToString(@"hh\:mm"),
                b.Status,
                b.TotalPrice
            })
            .ToListAsync<object>();
    }

    // ====================== CẬP NHẬT TRẠNG THÁI (DÀNH CHO VENDOR/ADMIN) ======================
    public async Task UpdateBookingStatusAsync(Guid bookingId, BookingStatus newStatus, Guid? performerId = null)
    {
        var booking = await _context.Bookings.FindAsync(bookingId);
        if (booking == null)
            throw new InvalidOperationException("Không tìm thấy đơn hàng");

        if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
            throw new InvalidOperationException("Không thể thay đổi trạng thái của đơn đã hoàn thành hoặc hủy");

        booking.Status = newStatus;
        booking.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    // ====================== LẤY DANH SÁCH BOOKING THEO NGÀY ======================
    public async Task<List<object>> GetBookingsByDateAsync(DateOnly? date)
    {
        var query = _context.Bookings
            .Include(b => b.User)
            .Include(b => b.TimeSlot)
            .AsQueryable();

        if (date.HasValue)
        {
            query = query.Where(b => b.BookingDate == date.Value);
        }

        var result = await query
            .OrderByDescending(b => b.BookingDate)
            .ThenBy(b => b.TimeSlot.StartTime)
            .Select(b => new
            {
                b.Id,
                b.BookingDate,
                b.TotalPrice,
                b.Status,
                b.Notes,
                Customer = new
                {
                    b.User.Id,
                    b.User.FullName,
                    b.User.Email
                },
                Pet = new
                {
                    b.Pet.Id,
                    b.Pet.Name
                },
                TimeSlot = new
                {
                    b.TimeSlot.StartTime,
                    b.TimeSlot.EndTime
                }
            })
            .ToListAsync<object>();

        return result;
    }
}