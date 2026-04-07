using BookingSystem.DTOs;
using BookingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookingSystem.Controllers.Vendor;

[Authorize(Roles = "ServiceProvider")]
[ApiController]
[Route("api/vendor/store/services")]
public class VendorServicesController : ControllerBase
{
    private readonly VendorServiceService _vendorServiceService;

    public VendorServicesController(VendorServiceService vendorServiceService)
    {
        _vendorServiceService = vendorServiceService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var result = await _vendorServiceService.GetAllAsync(userId);
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

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ServiceCreateDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var serviceId = await _vendorServiceService.CreateAsync(userId, dto);
            return Ok(new { id = serviceId, message = "Tạo dịch vụ thành công" });
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

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ServiceCreateDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            await _vendorServiceService.UpdateAsync(userId, id, dto);
            return Ok(new { message = "Cập nhật dịch vụ thành công" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi cập nhật dịch vụ", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            await _vendorServiceService.DeleteAsync(userId, id);
            return Ok(new { message = "Xóa dịch vụ thành công" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi xóa dịch vụ", error = ex.Message });
        }
    }
}