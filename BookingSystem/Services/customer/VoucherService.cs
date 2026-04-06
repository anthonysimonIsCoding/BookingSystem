using BookingSystem.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookingSystem.Services;

public class VoucherService
{
    private readonly BookingDbContext _context;

    public VoucherService(BookingDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lấy danh sách Voucher Sàn (Platform Vouchers) mà User còn sử dụng được
    /// </summary>
    public async Task<List<object>> GetAvailablePlatformVouchersAsync(Guid userId)
    {
        var vouchers = await _context.PlatformVouchers
            .Where(v => v.IsActive
                     && v.StartDate <= DateTime.UtcNow
                     && (v.EndDate == null || v.EndDate >= DateTime.UtcNow))
            .Select(v => new
            {
                v.Id,
                v.Code,
                v.Name,
                v.Description,
                v.DiscountType,
                v.DiscountValue,
                v.MinOrderValue,
                v.MaxDiscountAmount,
                v.UsageLimitPerUser
            })
            .ToListAsync();

        var result = new List<object>();

        foreach (var v in vouchers)
        {
            var usedCount = await _context.UsedVouchers
                .CountAsync(u => u.PlatformVoucherId == v.Id && u.UserId == userId);

            // Nếu không giới hạn hoặc chưa dùng hết lượt thì thêm vào
            if (!v.UsageLimitPerUser.HasValue || usedCount < v.UsageLimitPerUser.Value)
            {
                result.Add(v);
            }
        }

        return result;
    }

    /// <summary>
    /// Lấy danh sách Voucher của một Store mà User còn sử dụng được
    /// </summary>
    public async Task<List<object>> GetAvailableStoreVouchersAsync(Guid storeId, Guid userId)
    {
        var vouchers = await _context.StoreVouchers
            .Where(v => v.StoreId == storeId
                     && v.IsActive
                     && v.StartDate <= DateTime.UtcNow
                     && (v.EndDate == null || v.EndDate >= DateTime.UtcNow))
            .Select(v => new
            {
                v.Id,
                v.Code,
                v.Name,
                v.Description,
                v.DiscountType,
                v.DiscountValue,
                v.MinOrderValue,
                v.MaxDiscountAmount,
                v.UsageLimitPerUser
            })
            .ToListAsync();

        var result = new List<object>();

        foreach (var v in vouchers)
        {
            var usedCount = await _context.UsedVouchers
                .CountAsync(u => u.StoreVoucherId == v.Id && u.UserId == userId);

            if (!v.UsageLimitPerUser.HasValue || usedCount < v.UsageLimitPerUser.Value)
            {
                result.Add(v);
            }
        }

        return result;
    }
}