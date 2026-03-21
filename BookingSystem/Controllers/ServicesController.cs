using BookingSystem.Data;
using BookingSystem.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace BookingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicesController : ControllerBase
{
    private readonly BookingDbContext _context;

    public ServicesController(BookingDbContext context)
    {
        _context = context;
    }

    // GET api/services/store/{storeId}
    [HttpGet("store/{storeId}")]
    [Authorize]
    public async Task<IActionResult> GetByStore(Guid storeId)
    {
        var services = await _context.Services
            .Where(s => s.StoreId == storeId && s.IsActive)
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.Price,
                s.DurationMinutes
            })
            .ToListAsync();

        return Ok(services);
    }
}