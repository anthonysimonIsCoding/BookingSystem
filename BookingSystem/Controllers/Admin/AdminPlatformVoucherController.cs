using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookingSystem.Data;
using BookingSystem.Entities;
using BookingSystem.Entities.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BookingSystem.Controllers.Admin;

//[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/platform-vouchers")]
public class AdminPlatformVoucherController : ControllerBase
{
    private readonly BookingDbContext _context;

    public AdminPlatformVoucherController(BookingDbContext context)
    {
        _context = context;
    }

    // ==================== DANH SÁCH VOUCHER ====================
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var vouchers = await _context.PlatformVouchers
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();

        return Ok(vouchers);
    }

    // ==================== THÊM VOUCHER ====================
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PlatformVoucherCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Code))
            return BadRequest(new { message = "Mã voucher không được để trống" });

        if (dto.DiscountValue <= 0)
            return BadRequest(new { message = "Giá trị giảm phải lớn hơn 0" });

        if (dto.DiscountType == VoucherDiscountType.Percent && dto.DiscountValue > 100)
            return BadRequest(new { message = "Phần trăm giảm không được vượt quá 100%" });

        var voucher = new PlatformVoucher
        {
            Code = dto.Code.ToUpper().Trim(),
            Name = dto.Name?.Trim(),
            Description = dto.Description?.Trim(),
            DiscountType = dto.DiscountType,
            DiscountValue = dto.DiscountValue,
            MinOrderValue = dto.MinOrderValue,
            MaxDiscountAmount = dto.MaxDiscountAmount,
            UsageLimitPerUser = dto.UsageLimitPerUser > 0 ? dto.UsageLimitPerUser : null,
            TotalUsageLimit = dto.TotalUsageLimit > 0 ? dto.TotalUsageLimit : null,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            //CreatedByAdminId = User?.Identity?.IsAuthenticated == true
            //    ? Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString())
            //    : null
        };

        _context.PlatformVouchers.Add(voucher);
        await _context.SaveChangesAsync();

        return Ok(voucher);
    }

    // ==================== SỬA VOUCHER ====================
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] PlatformVoucherCreateDto dto)
    {
        var voucher = await _context.PlatformVouchers.FindAsync(id);
        if (voucher == null) return NotFound(new { message = "Không tìm thấy voucher" });

        voucher.Code = dto.Code.ToUpper().Trim();
        voucher.Name = dto.Name;
        voucher.Description = dto.Description;
        voucher.DiscountType = dto.DiscountType;
        voucher.DiscountValue = dto.DiscountValue;
        voucher.MinOrderValue = dto.MinOrderValue;
        voucher.MaxDiscountAmount = dto.MaxDiscountAmount;
        voucher.UsageLimitPerUser = dto.UsageLimitPerUser;
        voucher.TotalUsageLimit = dto.TotalUsageLimit;
        voucher.StartDate = dto.StartDate;
        voucher.EndDate = dto.EndDate;
        voucher.IsActive = dto.IsActive;
        voucher.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(voucher);
    }

    // ==================== XÓA / TẮT VOUCHER ====================
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var voucher = await _context.PlatformVouchers.FindAsync(id);
        if (voucher == null) return NotFound();

        _context.PlatformVouchers.Remove(voucher);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Đã xóa voucher" });
    }
}

// ==================== DTO ====================
public class PlatformVoucherCreateDto
{
    public string Code { get; set; } = null!;
    public string? Name { get; set; }
    public string? Description { get; set; }

    public VoucherDiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }

    public decimal? MinOrderValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }

    public int? UsageLimitPerUser { get; set; }
    public int? TotalUsageLimit { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;
}