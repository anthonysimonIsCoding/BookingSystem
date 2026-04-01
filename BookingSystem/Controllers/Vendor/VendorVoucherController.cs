using BookingSystem.Data;
using BookingSystem.Entities;
using BookingSystem.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookingSystem.Controllers.Vendor;

[ApiController]
[Route("api/vendor/store/vouchers")]
[Authorize(Roles = "ServiceProvider")]
public class VendorVoucherController : ControllerBase
{
    private readonly BookingDbContext _context;

    public VendorVoucherController(BookingDbContext context)
    {
        _context = context;
    }

    private async Task<Store> GetCurrentStoreAsync()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var store = await _context.Stores.FirstOrDefaultAsync(s => s.OwnerId == userId);
        if (store == null)
            throw new UnauthorizedAccessException("Không tìm thấy cửa hàng của bạn");
        return store;
    }

    // ====================== LẤY DANH SÁCH VOUCHER + THỐNG KÊ SỬ DỤNG ======================
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var store = await GetCurrentStoreAsync();

            var vouchers = await _context.StoreVouchers
                .Where(v => v.StoreId == store.Id)
                .Include(v => v.ApplicableService)
                .Include(v => v.ApplicableSpecies)
                .AsNoTracking()
                .ToListAsync();

            // Lấy tất cả UsedVoucher của store này trong khoảng thời gian của từng voucher
            var result = new List<object>();

            foreach (var v in vouchers)
            {
                // Query UsedVoucher theo UsedAt nằm trong khoảng StartDate - EndDate của voucher
                var usedQuery = _context.UsedVouchers
                    .Where(uv => uv.StoreVoucherId == v.Id);

                // Nếu có EndDate thì lọc theo khoảng thời gian
                if (v.EndDate.HasValue)
                {
                    usedQuery = usedQuery.Where(uv =>
                        uv.UsedAt >= v.StartDate &&
                        uv.UsedAt <= v.EndDate.Value);
                }
                else
                {
                    // Nếu không có EndDate thì chỉ lấy từ StartDate trở đi
                    usedQuery = usedQuery.Where(uv => uv.UsedAt >= v.StartDate);
                }

                var usedCount = await usedQuery.CountAsync();
                var totalDiscountApplied = await usedQuery.SumAsync(uv => uv.DiscountApplied);

                result.Add(new
                {
                    id = v.Id.ToString(),
                    code = v.Code,
                    name = v.Name,
                    description = v.Description,
                    discountType = (int)v.DiscountType,
                    discountValue = v.DiscountValue,
                    minOrderValue = v.MinOrderValue,
                    maxDiscountAmount = v.MaxDiscountAmount,
                    usageLimitPerUser = v.UsageLimitPerUser,
                    totalUsageLimit = v.TotalUsageLimit,
                    startDate = v.StartDate,
                    endDate = v.EndDate,
                    isActive = v.IsActive,
                    applicableServiceId = v.ApplicableServiceId?.ToString(),
                    applicableServiceName = v.ApplicableService?.Name,
                    applicableSpeciesId = v.ApplicableSpeciesId?.ToString(),
                    applicableSpeciesName = v.ApplicableSpecies?.Name,

                    // Thống kê đã dùng
                    usedCount = usedCount,
                    totalDiscountApplied = totalDiscountApplied
                });
            }

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
        }
    }

    // ====================== TẠO VOUCHER ======================
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] StoreVoucherCreateDto dto)
    {
        try
        {
            var store = await GetCurrentStoreAsync();

            var voucher = new StoreVoucher
            {
                StoreId = store.Id,
                Code = dto.Code.ToUpper().Trim(),
                Name = dto.Name,
                Description = dto.Description,
                DiscountType = (VoucherDiscountType)dto.DiscountType,
                DiscountValue = dto.DiscountValue,
                MinOrderValue = dto.MinOrderValue,
                MaxDiscountAmount = dto.MaxDiscountAmount,
                UsageLimitPerUser = dto.UsageLimitPerUser,
                TotalUsageLimit = dto.TotalUsageLimit,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsActive = dto.IsActive,
                ApplicableServiceId = dto.ApplicableServiceId,
                ApplicableSpeciesId = dto.ApplicableSpeciesId,
                CreatedByStoreOwnerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
                CreatedAt = DateTime.UtcNow
            };

            if (await _context.StoreVouchers.AnyAsync(v => v.StoreId == store.Id && v.Code == voucher.Code))
                return BadRequest("Mã voucher này đã tồn tại trong cửa hàng.");

            _context.StoreVouchers.Add(voucher);
            await _context.SaveChangesAsync();

            return Ok(new { id = voucher.Id.ToString(), message = "Tạo voucher thành công" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi tạo voucher", error = ex.Message });
        }
    }

    // ====================== CẬP NHẬT VOUCHER ======================
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] StoreVoucherCreateDto dto)
    {
        try
        {
            var store = await GetCurrentStoreAsync();

            var voucher = await _context.StoreVouchers
                .FirstOrDefaultAsync(v => v.Id == id && v.StoreId == store.Id);

            if (voucher == null) return NotFound("Không tìm thấy voucher");

            voucher.Code = dto.Code.ToUpper().Trim();
            voucher.Name = dto.Name;
            voucher.Description = dto.Description;
            voucher.DiscountType = (VoucherDiscountType)dto.DiscountType;
            voucher.DiscountValue = dto.DiscountValue;
            voucher.MinOrderValue = dto.MinOrderValue;
            voucher.MaxDiscountAmount = dto.MaxDiscountAmount;
            voucher.UsageLimitPerUser = dto.UsageLimitPerUser;
            voucher.TotalUsageLimit = dto.TotalUsageLimit;
            voucher.StartDate = dto.StartDate;
            voucher.EndDate = dto.EndDate;
            voucher.IsActive = dto.IsActive;
            voucher.ApplicableServiceId = dto.ApplicableServiceId;
            voucher.ApplicableSpeciesId = dto.ApplicableSpeciesId;
            voucher.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật voucher thành công" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi cập nhật voucher", error = ex.Message });
        }
    }

    // ====================== XÓA VOUCHER ======================
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var store = await GetCurrentStoreAsync();

            var voucher = await _context.StoreVouchers
                .FirstOrDefaultAsync(v => v.Id == id && v.StoreId == store.Id);

            if (voucher == null) return NotFound("Không tìm thấy voucher");

            _context.StoreVouchers.Remove(voucher);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa voucher thành công" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi xóa voucher", error = ex.Message });
        }
    }
}

// ====================== DTO ======================
// ====================== DTO ======================
public class StoreVoucherCreateDto
{
    public string Code { get; set; } = null!;
    public string? Name { get; set; }
    public string? Description { get; set; }

    public int DiscountType { get; set; }
    public decimal DiscountValue { get; set; }

    public decimal? MinOrderValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }

    public int? UsageLimitPerUser { get; set; }
    public int? TotalUsageLimit { get; set; }

    public DateTime StartDate { get; set; }

    // Sửa ở đây: Cho phép null hoặc chuỗi rỗng
    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public Guid? ApplicableServiceId { get; set; }
    public Guid? ApplicableSpeciesId { get; set; }
}