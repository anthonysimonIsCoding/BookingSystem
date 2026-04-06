using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookingSystem.Data;
using BookingSystem.Entities.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BookingSystem.Controllers.Admin;

//[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/users")]
public class AdminUserController : ControllerBase
{
    private readonly BookingDbContext _context;

    public AdminUserController(BookingDbContext context)
    {
        _context = context;
    }

    // ==================== DANH SÁCH NGƯỜI DÙNG (CHỈ CUSTOMER) ====================
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _context.Users
            .Where(u => u.Role == UserRole.Customer)   // Chỉ lấy Customer
            .Include(u => u.Pets)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(u => u.FullName.Contains(search) ||
                                    u.Email.Contains(search) ||
                                    u.PhoneNumber!.Contains(search));
        }

        //if (isActive.HasValue)
        //{
        //    query = query.Where(u => u.IsActive == isActive.Value);
        //}

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new
            {
                u.Id,
                u.FullName,
                u.Email,
                u.PhoneNumber,
                u.CreatedAt,
                PetCount = u.Pets.Count
            })
            .ToListAsync();

        return Ok(new
        {
            items = users,
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }

    // ==================== CHI TIẾT NGƯỜI DÙNG + DANH SÁCH PET ====================
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _context.Users
            .Include(u => u.Pets)
                .ThenInclude(p => p.Species)
            .Include(u => u.Pets)
                .ThenInclude(p => p.Breed)
            .FirstOrDefaultAsync(u => u.Id == id && u.Role == UserRole.Customer);

        if (user == null)
            return NotFound(new { message = "Không tìm thấy người dùng" });

        var result = new
        {
            user.Id,
            user.FullName,
            user.Email,
            user.PhoneNumber,
            user.CreatedAt,
            user.UpdatedAt,

            Pets = user.Pets.Select(p => new
            {
                p.Id,
                p.Name,
                Species = p.Species?.Name,
                Breed = p.Breed?.Name,
                p.Gender,
                p.DateOfBirth,
                p.Color,
                p.Weight,
                p.ProfileImageUrl,
                p.IsActive,
                p.Notes
            }).ToList()
        };

        return Ok(result);
    }
}