using BookingSystem.Data;
using BookingSystem.DTOs;
using BookingSystem.Entities;
using BookingSystem.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingSystem.Services;

public class VendorServiceService
{
    private readonly BookingDbContext _context;

    public VendorServiceService(BookingDbContext context)
    {
        _context = context;
    }

    private async Task<Store> GetCurrentStoreAsync(Guid userId)
    {
        var store = await _context.Stores
            .FirstOrDefaultAsync(s => s.OwnerId == userId);

        if (store == null)
            throw new UnauthorizedAccessException("Không tìm thấy cửa hàng của bạn");

        return store;
    }

    // ====================== LẤY TẤT CẢ DỊCH VỤ CỦA STORE ======================
    public async Task<List<object>> GetAllAsync(Guid userId)
    {
        var store = await GetCurrentStoreAsync(userId);

        var services = await _context.Services
            .Where(s => s.StoreId == store.Id)
            .Include(s => s.OptionGroups)
                .ThenInclude(g => g.Options)
            .AsNoTracking()
            .ToListAsync();

        return services.Select(s => new
        {
            id = s.Id.ToString(),
            name = s.Name,
            description = s.Description,
            price = s.Price,
            durationMinutes = s.DurationMinutes,
            type = (int)s.Type,
            isActive = s.IsActive,
            optionGroups = s.OptionGroups.Select(g => new
            {
                id = g.Id.ToString(),
                name = g.Name,
                type = (int)g.Type,
                isRequired = g.IsRequired,
                options = g.Options.Select(o => new
                {
                    id = o.Id.ToString(),
                    name = o.Name,
                    price = o.Price,
                    durationMinutes = o.DurationMinutes,
                    isActive = o.IsActive
                }).ToList()
            }).ToList()
        }).ToList<object>();
    }

    // ====================== TẠO DỊCH VỤ MỚI ======================
    public async Task<string> CreateAsync(Guid userId, ServiceCreateDto dto)
    {
        var store = await GetCurrentStoreAsync(userId);

        var service = new Service
        {
            StoreId = store.Id,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            DurationMinutes = dto.DurationMinutes,
            Type = (ServiceType)dto.Type,
            IsActive = dto.IsActive
        };

        foreach (var gDto in dto.OptionGroups)
        {
            var group = new ServiceOptionGroup
            {
                Name = gDto.Name,
                Type = (OptionGroupType)gDto.Type,
                IsRequired = gDto.IsRequired,
                Options = gDto.Options.Select(o => new ServiceOption
                {
                    Name = o.Name,
                    Price = o.Price,
                    DurationMinutes = o.DurationMinutes,
                    IsActive = o.IsActive
                }).ToList()
            };
            service.OptionGroups.Add(group);
        }

        _context.Services.Add(service);
        await _context.SaveChangesAsync();

        return service.Id.ToString();
    }

    // ====================== CẬP NHẬT DỊCH VỤ ======================
    public async Task UpdateAsync(Guid userId, Guid serviceId, ServiceCreateDto dto)
    {
        var store = await GetCurrentStoreAsync(userId);

        var service = await _context.Services
            .Include(s => s.OptionGroups)
                .ThenInclude(g => g.Options)
            .FirstOrDefaultAsync(s => s.Id == serviceId && s.StoreId == store.Id);

        if (service == null)
            throw new KeyNotFoundException("Không tìm thấy dịch vụ");

        // Cập nhật thông tin cơ bản
        service.Name = dto.Name;
        service.Description = dto.Description;
        service.Price = dto.Price;
        service.DurationMinutes = dto.DurationMinutes;
        service.Type = (ServiceType)dto.Type;
        service.IsActive = dto.IsActive;

        var existingGroups = service.OptionGroups.ToList();

        // Xóa group không còn tồn tại trong request
        foreach (var existing in existingGroups)
        {
            if (!dto.OptionGroups.Any(g => Guid.TryParse(g.Id ?? "", out var gid) && gid == existing.Id))
            {
                _context.ServiceOptionGroups.Remove(existing);
            }
        }

        // Thêm / Cập nhật group và options
        foreach (var gDto in dto.OptionGroups)
        {
            ServiceOptionGroup? group = null;

            if (!string.IsNullOrEmpty(gDto.Id) && Guid.TryParse(gDto.Id, out var groupId))
            {
                group = existingGroups.FirstOrDefault(g => g.Id == groupId);
            }

            if (group == null)
            {
                // Tạo group mới
                group = new ServiceOptionGroup
                {
                    Name = gDto.Name,
                    Type = (OptionGroupType)gDto.Type,
                    IsRequired = gDto.IsRequired
                };
                service.OptionGroups.Add(group);
            }
            else
            {
                // Cập nhật group cũ
                group.Name = gDto.Name;
                group.Type = (OptionGroupType)gDto.Type;
                group.IsRequired = gDto.IsRequired;
            }

            // Sync options trong group
            var existingOptions = group.Options.ToList();

            foreach (var opt in existingOptions)
            {
                if (!gDto.Options.Any(o => Guid.TryParse(o.Id ?? "", out var oid) && oid == opt.Id))
                {
                    _context.ServiceOptions.Remove(opt);
                }
            }

            foreach (var oDto in gDto.Options)
            {
                if (!string.IsNullOrEmpty(oDto.Id) && Guid.TryParse(oDto.Id, out var optId))
                {
                    var existingOpt = existingOptions.FirstOrDefault(o => o.Id == optId);
                    if (existingOpt != null)
                    {
                        existingOpt.Name = oDto.Name;
                        existingOpt.Price = oDto.Price;
                        existingOpt.DurationMinutes = oDto.DurationMinutes;
                        existingOpt.IsActive = oDto.IsActive;
                    }
                }
                else
                {
                    // Thêm option mới
                    group.Options.Add(new ServiceOption
                    {
                        Name = oDto.Name,
                        Price = oDto.Price,
                        DurationMinutes = oDto.DurationMinutes,
                        IsActive = oDto.IsActive
                    });
                }
            }
        }

        await _context.SaveChangesAsync();
    }

    // ====================== XÓA DỊCH VỤ ======================
    public async Task DeleteAsync(Guid userId, Guid serviceId)
    {
        var store = await GetCurrentStoreAsync(userId);

        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == serviceId && s.StoreId == store.Id);

        if (service == null)
            throw new KeyNotFoundException("Không tìm thấy dịch vụ");

        _context.Services.Remove(service);
        await _context.SaveChangesAsync();
    }
}