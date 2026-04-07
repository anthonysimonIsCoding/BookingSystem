using BookingSystem.Data;
using BookingSystem.DTOs;
using BookingSystem.Entities;
using BookingSystem.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingSystem.Services;

public class AdminPlatformVoucherService
{
    private readonly BookingDbContext _context;

    public AdminPlatformVoucherService(BookingDbContext context)
    {
        _context = context;
    }

    // ==================== LẤY DANH SÁCH VOUCHER SÀN ====================
    public async Task<List<PlatformVoucher>> GetAllAsync()
    {
        return await _context.PlatformVouchers
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();
    }

    // ==================== TẠO VOUCHER SÀN ====================
    public async Task<PlatformVoucher> CreateAsync(PlatformVoucherCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Code))
            throw new InvalidOperationException("Mã voucher không được để trống");

        if (dto.DiscountValue <= 0)
            throw new InvalidOperationException("Giá trị giảm phải lớn hơn 0");

        if (dto.DiscountType == VoucherDiscountType.Percent && dto.DiscountValue > 100)
            throw new InvalidOperationException("Phần trăm giảm không được vượt quá 100%");

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
            CreatedAt = DateTime.UtcNow
        };

        // Kiểm tra mã voucher trùng
        if (await _context.PlatformVouchers.AnyAsync(v => v.Code == voucher.Code))
            throw new InvalidOperationException("Mã voucher này đã tồn tại trên sàn");

        _context.PlatformVouchers.Add(voucher);
        await _context.SaveChangesAsync();

        return voucher;
    }

    // ==================== CẬP NHẬT VOUCHER ====================
    public async Task<PlatformVoucher> UpdateAsync(Guid id, PlatformVoucherCreateDto dto)
    {
        var voucher = await _context.PlatformVouchers.FindAsync(id);
        if (voucher == null)
            throw new KeyNotFoundException("Không tìm thấy voucher");

        voucher.Code = dto.Code.ToUpper().Trim();
        voucher.Name = dto.Name?.Trim();
        voucher.Description = dto.Description?.Trim();
        voucher.DiscountType = dto.DiscountType;
        voucher.DiscountValue = dto.DiscountValue;
        voucher.MinOrderValue = dto.MinOrderValue;
        voucher.MaxDiscountAmount = dto.MaxDiscountAmount;
        voucher.UsageLimitPerUser = dto.UsageLimitPerUser > 0 ? dto.UsageLimitPerUser : null;
        voucher.TotalUsageLimit = dto.TotalUsageLimit > 0 ? dto.TotalUsageLimit : null;
        voucher.StartDate = dto.StartDate;
        voucher.EndDate = dto.EndDate;
        voucher.IsActive = dto.IsActive;
        voucher.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return voucher;
    }

    // ==================== XÓA VOUCHER ====================
    public async Task DeleteAsync(Guid id)
    {
        var voucher = await _context.PlatformVouchers.FindAsync(id);
        if (voucher == null)
            throw new KeyNotFoundException("Không tìm thấy voucher");

        _context.PlatformVouchers.Remove(voucher);
        await _context.SaveChangesAsync();
    }
}