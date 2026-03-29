using BookingSystem.Data;
using BookingSystem.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BookingSystem.Controllers;

[ApiController]
[Route("api/services")]
public class ServicesController : ControllerBase
{
    private readonly BookingDbContext _context;

    public ServicesController(BookingDbContext context)
    {
        _context = context;
    }

    // ====================== LẤY TẤT CẢ DỊCH VỤ CỦA STORE (FULL NESTED) ======================
    [HttpGet("store/{storeId}")]
    public async Task<IActionResult> GetByStore(Guid storeId)
    {
        var services = await _context.Services
            .Where(s => s.StoreId == storeId && s.IsActive)
            .Include(s => s.OptionGroups)                          // Load ServiceOptionGroup
                .ThenInclude(g => g.Options.Where(o => o.IsActive)) // Load ServiceOption (chỉ active)
            .AsNoTracking()                                        // Tăng tốc
            .ToListAsync();

        // Project ra đúng format frontend đang mong đợi
        var result = services.Select(s => new
        {
            id = s.Id.ToString(),
            name = s.Name,
            price = s.Price,
            durationMinutes = s.DurationMinutes,

            optionGroups = s.OptionGroups.Select(g => new
            {
                id = g.Id.ToString(),
                name = g.Name,
                type = (int)g.Type,           // 0 = SingleChoice, 1 = MultiChoice
                isRequired = g.IsRequired,
                options = g.Options.Select(o => new
                {
                    id = o.Id.ToString(),
                    name = o.Name,
                    price = o.Price,
                    durationMinutes = o.DurationMinutes
                }).ToList()
            }).ToList()
        }).ToList();

        return Ok(result);
    }
}