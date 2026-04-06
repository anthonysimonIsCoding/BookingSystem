using BookingSystem.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using BookingSystem.DTOs;


namespace BookingSystem.Services;

public class NavbarService
{
    private readonly BookingDbContext _context;

    public NavbarService(BookingDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lấy danh sách Store Categories để hiển thị trên Navbar
    /// </summary>
    public async Task<List<StoreCategoryDto>> GetStoreCategoriesAsync()
    {
        return await _context.StoreCategories
            .OrderBy(c => c.Name)
            .Select(c => new StoreCategoryDto
            {
                Id = c.Id,
                Name = c.Name
            })
            .ToListAsync();
    }

    /// <summary>
    /// Test endpoint (có thể giữ hoặc xóa sau)
    /// </summary>
    public string Test()
    {
        return "Navbar API OK";
    }
}