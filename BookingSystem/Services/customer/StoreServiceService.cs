using BookingSystem.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingSystem.Services;

public class StoreServiceService   // Tên hơi dài nhưng rõ ràng: Service của Store
{
    private readonly BookingDbContext _context;

    public StoreServiceService(BookingDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lấy tất cả dịch vụ của một Store (kèm OptionGroups và Options)
    /// </summary>
    public async Task<List<object>> GetServicesByStoreAsync(Guid storeId)
    {
        var services = await _context.Services
            .Where(s => s.StoreId == storeId && s.IsActive)
            .Include(s => s.OptionGroups)
                .ThenInclude(g => g.Options.Where(o => o.IsActive))   // Chỉ lấy option đang active
            .AsNoTracking()
            .ToListAsync();

        // Project theo đúng format mà Frontend đang mong đợi
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
        }).ToList<object>();

        return result;
    }
}