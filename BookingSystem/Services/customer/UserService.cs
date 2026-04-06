using BookingSystem.Data;
using BookingSystem.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookingSystem.Services;

public class UserService
{
    private readonly BookingDbContext _context;

    public UserService(BookingDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lấy thông tin profile của user hiện tại
    /// </summary>
    public async Task<object?> GetProfileAsync(Guid userId)
    {
        var user = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.Id,
                u.FullName,
                u.Email,
                u.PhoneNumber
            })
            .FirstOrDefaultAsync();

        return user;
    }

    /// <summary>
    /// Cập nhật thông tin profile của user
    /// </summary>
    public async Task<object> UpdateProfileAsync(Guid userId, UpdateUserRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new InvalidOperationException("Không tìm thấy người dùng");

        // Cập nhật thông tin
        user.FullName = request.FullName?.Trim() ?? user.FullName;
        user.PhoneNumber = request.PhoneNumber?.Trim();

        await _context.SaveChangesAsync();

        // Trả về thông tin sau khi cập nhật
        return new
        {
            user.Id,
            user.FullName,
            user.Email,
            user.PhoneNumber
        };
    }
}