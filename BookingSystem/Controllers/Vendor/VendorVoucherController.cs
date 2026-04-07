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
[Route("api/vendor/store/vouchers")]
public class VendorVoucherController : ControllerBase
{
    private readonly VendorVoucherService _vendorVoucherService;

    public VendorVoucherController(VendorVoucherService vendorVoucherService)
    {
        _vendorVoucherService = vendorVoucherService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var result = await _vendorVoucherService.GetAllAsync(userId);
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
    public async Task<IActionResult> Create([FromBody] StoreVoucherCreateDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var voucherId = await _vendorVoucherService.CreateAsync(userId, dto);
            return Ok(new { id = voucherId, message = "Tạo voucher thành công" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi tạo voucher", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] StoreVoucherCreateDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            await _vendorVoucherService.UpdateAsync(userId, id, dto);
            return Ok(new { message = "Cập nhật voucher thành công" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Không tìm thấy voucher");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi cập nhật voucher", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            await _vendorVoucherService.DeleteAsync(userId, id);
            return Ok(new { message = "Xóa voucher thành công" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Không tìm thấy voucher");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi xóa voucher", error = ex.Message });
        }
    }
}