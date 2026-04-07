using BookingSystem.Data;
using BookingSystem.DTOs;
using BookingSystem.Entities;
using BookingSystem.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingSystem.Services;

public class AdminUserService
{
    private readonly BookingDbContext _context;

    public AdminUserService(BookingDbContext context)
    {
        _context = context;
    }

    // ==================== DANH SÁCH USER (CÓ PHÂN TRANG + FILTER) ====================
    public async Task<object> GetAllAsync(
        string? search,
        UserRole? role,
        bool? isActive,
        int page = 1,
        int pageSize = 20)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));

        if (role.HasValue)
            query = query.Where(u => u.Role == role.Value);

        //if (isActive.HasValue)
        //    query = query.Where(u => u.IsActive == isActive.Value);

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new AdminUserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Role = u.Role,
                //IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return new
        {
            items = users,
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    // ==================== CHI TIẾT USER ====================
    public async Task<AdminUserDto?> GetByIdAsync(Guid id)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return null;

        return new AdminUserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            //IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    // ==================== CẬP NHẬT ROLE USER ====================
    public async Task UpdateRoleAsync(Guid id, UserRole newRole)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            throw new KeyNotFoundException("Không tìm thấy người dùng");

        if (user.Role == newRole)
            return;

        user.Role = newRole;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    // ==================== BẬT / TẮT USER ====================
    public async Task ToggleActiveAsync(Guid id, bool isActive)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            throw new KeyNotFoundException("Không tìm thấy người dùng");

        //user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    // ==================== XÓA USER (Soft Delete hoặc Hard) ====================
    public async Task DeleteAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            throw new KeyNotFoundException("Không tìm thấy người dùng");

        // Hard delete (nếu bạn muốn soft delete thì thêm cột IsDeleted)
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }
}