using BookingSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace BookingSystem.Controllers;

[Authorize(Roles = "Customer")]
[ApiController]
[Route("api/[controller]")]
public class TimeSlotsController : ControllerBase
{
    private readonly BookingDbContext _context;

    public TimeSlotsController(BookingDbContext context)
    {
        _context = context;
    }

    [HttpGet("store/{storeId}")]
    public async Task<IActionResult> GetByStore(Guid storeId)
    {
        var slots = await _context.TimeSlots
            .Where(s => s.StoreId == storeId && s.IsActive)
            .ToListAsync();

        return Ok(slots);
    }
}