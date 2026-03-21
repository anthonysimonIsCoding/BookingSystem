using BookingSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace BookingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NavbarController : ControllerBase
{
    private readonly BookingDbContext _context;

    public NavbarController(BookingDbContext context)
    {
        _context = context;
    }

    // GET api/navbar/store-categories
    [HttpGet("store-categories")]
    public async Task<IActionResult> GetStoreCategories()
    {
        try
        {
            var categories = await _context.StoreCategories
                .OrderBy(c => c.Name)
                .Select(c => new StoreCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();

            return Ok(categories);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Lỗi khi lấy danh sách category",
                error = ex.Message
            });
        }
    }

    // DEBUG endpoint (có thể xoá sau)
    // GET api/navbar/test
    [HttpGet("test")]
    public IActionResult Test()
    {
        return Ok("Navbar API OK");
    }
}

// DTO
public class StoreCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}