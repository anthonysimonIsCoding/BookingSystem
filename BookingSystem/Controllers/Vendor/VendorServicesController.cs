using BookingSystem.Data;
using BookingSystem.Entities;
using BookingSystem.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookingSystem.Controllers.Vendor;

[ApiController]
[Route("api/vendor/store/services")]
[Authorize(Roles = "ServiceProvider")]
public class VendorServicesController : ControllerBase
{
    private readonly BookingDbContext _context;

    public VendorServicesController(BookingDbContext context)
    {
        _context = context;
    }

    private async Task<Store> GetCurrentStoreAsync()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var store = await _context.Stores
            .FirstOrDefaultAsync(s => s.OwnerId == userId);

        if (store == null)
            throw new UnauthorizedAccessException("Không tìm thấy cửa hàng của bạn");

        return store;
    }

    // ====================== LẤY TẤT CẢ DỊCH VỤ ======================
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var store = await GetCurrentStoreAsync();

            var services = await _context.Services
                .Where(s => s.StoreId == store.Id)
                .Include(s => s.OptionGroups)
                    .ThenInclude(g => g.Options)
                .AsNoTracking()
                .ToListAsync();

            var result = services.Select(s => new
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
            }).ToList();

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
        }
    }

    // ====================== TẠO MỚI DỊCH VỤ ======================
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ServiceCreateDto dto)
    {
        try
        {
            var store = await GetCurrentStoreAsync();

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

            return Ok(new { id = service.Id.ToString(), message = "Tạo dịch vụ thành công" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi tạo dịch vụ", error = ex.Message });
        }
    }

    // ====================== CẬP NHẬT DỊCH VỤ ======================
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ServiceCreateDto dto)
    {
        try
        {
            var store = await GetCurrentStoreAsync();

            var service = await _context.Services
                .Include(s => s.OptionGroups)
                    .ThenInclude(g => g.Options)
                .FirstOrDefaultAsync(s => s.Id == id && s.StoreId == store.Id);

            if (service == null)
                return NotFound("Không tìm thấy dịch vụ");

            // Cập nhật thông tin dịch vụ
            service.Name = dto.Name;
            service.Description = dto.Description;
            service.Price = dto.Price;
            service.DurationMinutes = dto.DurationMinutes;
            service.Type = (ServiceType)dto.Type;
            service.IsActive = dto.IsActive;

            var existingGroups = service.OptionGroups.ToList();

            // Xóa các group không còn trong request
            foreach (var existing in existingGroups)
            {
                if (!dto.OptionGroups.Any(g => Guid.TryParse(g.Id ?? "", out var gid) && gid == existing.Id))
                {
                    _context.ServiceOptionGroups.Remove(existing);
                }
            }

            // Thêm/Sửa group và option
            foreach (var gDto in dto.OptionGroups)
            {
                ServiceOptionGroup? group;
                if (!string.IsNullOrEmpty(gDto.Id) && Guid.TryParse(gDto.Id, out var groupId))
                {
                    group = existingGroups.FirstOrDefault(g => g.Id == groupId);
                }
                else
                {
                    group = null;
                }

                if (group == null)
                {
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
                    group.Name = gDto.Name;
                    group.Type = (OptionGroupType)gDto.Type;
                    group.IsRequired = gDto.IsRequired;
                }

                // Sync options
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
            return Ok(new { message = "Cập nhật dịch vụ thành công" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi cập nhật dịch vụ", error = ex.Message });
        }
    }

    // ====================== XÓA DỊCH VỤ ======================
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var store = await GetCurrentStoreAsync();

            var service = await _context.Services
                .FirstOrDefaultAsync(s => s.Id == id && s.StoreId == store.Id);

            if (service == null)
                return NotFound("Không tìm thấy dịch vụ");

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa dịch vụ thành công" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi xóa dịch vụ", error = ex.Message });
        }
    }

    // ====================== DTOs ======================
    public class ServiceCreateDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }
        public int Type { get; set; }           // 0 = Single, 1 = Multiple
        public bool IsActive { get; set; } = true;

        public List<OptionGroupDto> OptionGroups { get; set; } = new();
    }

    public class OptionGroupDto
    {
        public string? Id { get; set; }
        public string Name { get; set; } = null!;
        public int Type { get; set; }           // 0 = SingleChoice, 1 = MultiChoice
        public bool IsRequired { get; set; }
        public List<OptionDto> Options { get; set; } = new();
    }

    public class OptionDto
    {
        public string? Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }
        public bool IsActive { get; set; } = true;
    }
}