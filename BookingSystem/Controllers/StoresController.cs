using BookingSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace BookingSystem.Controllers;

[Authorize(Roles = "Customer")]
[ApiController]
[Route("api/[controller]")]
public class StoresController : ControllerBase
{
    private readonly BookingDbContext _context;

    public StoresController(BookingDbContext context)
    {
        _context = context;
    }

    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var stores = await _context.Stores.ToListAsync();
        return Ok(stores);
    }
}