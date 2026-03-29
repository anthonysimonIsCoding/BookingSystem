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

namespace BookingSystem.Controllers;

[Authorize]   // ← Bắt buộc phải login
[ApiController]
[Route("api/vouchers")]
public class VoucherController : ControllerBase
{
    private readonly BookingDbContext _context;

    public VoucherController(BookingDbContext context)
    {
        _context = context;
    }

    // ====================== VOUCHER SÀN - CHỈ NHỮNG CÁI USER CÒN DÙNG ĐƯỢC ======================
    [HttpGet("platform/available")]
    public async Task<IActionResult> GetAvailablePlatformVouchers()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

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

        // Lọc những voucher user chưa dùng quá giới hạn
        var result = new List<dynamic>();
        foreach (var v in vouchers)
        {
            var usedCount = await _context.UsedVouchers
                .CountAsync(u => u.PlatformVoucherId == v.Id && u.UserId == userId);

            if (!v.UsageLimitPerUser.HasValue || usedCount < v.UsageLimitPerUser.Value)
            {
                result.Add(v);
            }
        }

        return Ok(result);
    }

    // ====================== VOUCHER SHOP - CHỈ NHỮNG CÁI USER CÒN DÙNG ĐƯỢC ======================
    [HttpGet("store/{storeId}/available")]
    public async Task<IActionResult> GetAvailableStoreVouchers(Guid storeId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

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

        var result = new List<dynamic>();
        foreach (var v in vouchers)
        {
            var usedCount = await _context.UsedVouchers
                .CountAsync(u => u.StoreVoucherId == v.Id && u.UserId == userId);

            if (!v.UsageLimitPerUser.HasValue || usedCount < v.UsageLimitPerUser.Value)
            {
                result.Add(v);
            }
        }

        return Ok(result);
    }
}