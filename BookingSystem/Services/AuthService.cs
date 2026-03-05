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
            Role = UserRole.ServiceProvider,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        _context.SaveChanges();
    }

    public string? Login(string email, string password)
    {
        Console.WriteLine("Email nhận: " + email);
        Console.WriteLine("Password nhận: " + password);
        var user = _context.Users.FirstOrDefault(x => x.Email.ToLower() == email.ToLower());

        if (user == null)
            return null;

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;
        Console.WriteLine("Verify: " +
    BCrypt.Net.BCrypt.Verify(password, user.PasswordHash));
        Console.WriteLine(
    BCrypt.Net.BCrypt.Verify("123456",
    "$2a$11$X3eA3OXg5R4FgcAeXpJGluMAQDoKCWcJx6rPRCk3lu2mbj3qaAhv."));
        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("THIS_IS_A_SUPER_SECRET_KEY_1234567890"));

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}