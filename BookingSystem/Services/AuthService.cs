using System;
using System.Text;
using BookingSystem.Data;
using BookingSystem.DTOs;
using BookingSystem.Entities;
using BookingSystem.Entities.Enums;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace BookingSystem.Services;
public class AuthService
{
    private readonly BookingDbContext _context;

    public AuthService(BookingDbContext context)
    {
        _context = context;
    }

    public void Register(RegisterRequest request)
    {
        var existingUser = _context.Users
            .FirstOrDefault(x => x.Email == request.Email);

        if (existingUser != null)
            throw new Exception("Email already exists");

        var hashed = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FullName = request.FullName,
            PasswordHash = hashed,
            Role = UserRole.Customer,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        _context.SaveChanges();
    }

    public string? Login(string email, string password)
    {
        var user = _context.Users.FirstOrDefault(x => x.Email.ToLower() == email.ToLower());
        if (user == null) return null;

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role.ToString())   // ← SỬA THÀNH ClaimTypes.Role
    };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("THIS_IS_A_SUPER_SECRET_KEY_1234567890"));

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Thêm vào AuthService.cs
    public void RegisterVendor(VendorRegisterRequest request)
    {
        var existingUser = _context.Users.FirstOrDefault(x => x.Email == request.Email);
        if (existingUser != null)
            throw new Exception("Email already exists");

        var hashed = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FullName = request.FullName,
            PasswordHash = hashed,
            Role = UserRole.ServiceProvider,     // ← Quan trọng
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        // TODO: Sau này bạn sẽ tạo luôn Store cho user này
        // Hiện tại tạm thời chỉ tạo User với Role StoreOwner
    }
}