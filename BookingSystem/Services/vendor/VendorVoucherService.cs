using BookingSystem.Data;
using BookingSystem.DTOs;
using BookingSystem.Entities;
using BookingSystem.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingSystem.Services;

public class VendorVoucherService
{
    private readonly BookingDbContext _context;

    public VendorVoucherService(BookingDbContext context)
    {
        _context = context;
    }

    private async Task<Store> GetCurrentStoreAsync(Guid userId)
    {
        var store = await _context.Stores
            .FirstOrDefaultAsync(s => s.OwnerId == userId);

        if (store == null)
            throw new UnauthorizedAccessException("Không tìm thấy cửa hàng của bạn");

        return store;
    }

    // ====================== LẤY DANH SÁCH VOUCHER + THỐNG KÊ ======================
    public async Task<List<object>> GetAllAsync(Guid userId)
    {
        var store = await GetCurrentStoreAsync(userId);

        var vouchers = await _context.StoreVouchers
            .Where(v => v.StoreId == store.Id)
            .Include(v => v.ApplicableService)
            .Include(v => v.ApplicableSpecies)
            .AsNoTracking()
            .ToListAsync();

        var result = new List<object>();

        foreach (var v in vouchers)
        {
            var usedQuery = _context.UsedVouchers
                .Where(uv => uv.StoreVoucherId == v.Id);

            if (v.EndDate.HasValue)
            {
                usedQuery = usedQuery.Where(uv =>
                    uv.UsedAt >= v.StartDate &&
                    uv.UsedAt <= v.EndDate.Value);
            }
            else
            {
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

                // Thống kê sử dụng
                usedCount = usedCount,
                totalDiscountApplied = totalDiscountApplied
            });
        }

        return result;
    }

    // ====================== TẠO VOUCHER ======================
    public async Task<string> CreateAsync(Guid userId, StoreVoucherCreateDto dto)
    {
        var store = await GetCurrentStoreAsync(userId);

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
            CreatedByStoreOwnerId = userId,
            CreatedAt = DateTime.UtcNow
        };

        // Kiểm tra mã voucher trùng trong cửa hàng
        if (await _context.StoreVouchers.AnyAsync(v => v.StoreId == store.Id && v.Code == voucher.Code))
            throw new InvalidOperationException("Mã voucher này đã tồn tại trong cửa hàng.");

        _context.StoreVouchers.Add(voucher);
        await _context.SaveChangesAsync();

        return voucher.Id.ToString();
    }

    // ====================== CẬP NHẬT VOUCHER ======================
    public async Task UpdateAsync(Guid userId, Guid voucherId, StoreVoucherCreateDto dto)
    {
        var store = await GetCurrentStoreAsync(userId);

        var voucher = await _context.StoreVouchers
            .FirstOrDefaultAsync(v => v.Id == voucherId && v.StoreId == store.Id);

        if (voucher == null)
            throw new KeyNotFoundException("Không tìm thấy voucher");

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
    }

    // ====================== XÓA VOUCHER ======================
    public async Task DeleteAsync(Guid userId, Guid voucherId)
    {
        var store = await GetCurrentStoreAsync(userId);

        var voucher = await _context.StoreVouchers
            .FirstOrDefaultAsync(v => v.Id == voucherId && v.StoreId == store.Id);

        if (voucher == null)
            throw new KeyNotFoundException("Không tìm thấy voucher");

        _context.StoreVouchers.Remove(voucher);
        await _context.SaveChangesAsync();
    }
}