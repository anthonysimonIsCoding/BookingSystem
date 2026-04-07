using BookingSystem.Data;
using BookingSystem.DTOs;
using BookingSystem.Entities;
using BookingSystem.Entities.Enums;
using BookingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingSystem.Controllers.Admin;

[ApiController]
[Route("api/admin/platform-vouchers")]
//[Authorize(Roles = "Admin")]
public class AdminPlatformVoucherController : ControllerBase
{
    private readonly AdminPlatformVoucherService _adminPlatformVoucherService;

    public AdminPlatformVoucherController(AdminPlatformVoucherService adminPlatformVoucherService)
    {
        _adminPlatformVoucherService = adminPlatformVoucherService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var vouchers = await _adminPlatformVoucherService.GetAllAsync();
        return Ok(vouchers);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PlatformVoucherCreateDto dto)
    {
        try
        {
            var voucher = await _adminPlatformVoucherService.CreateAsync(dto);
            return Ok(voucher);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi tạo voucher", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] PlatformVoucherCreateDto dto)
    {
        try
        {
            var voucher = await _adminPlatformVoucherService.UpdateAsync(id, dto);
            return Ok(voucher);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Không tìm thấy voucher" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi cập nhật voucher", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _adminPlatformVoucherService.DeleteAsync(id);
            return Ok(new { message = "Đã xóa voucher" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Không tìm thấy voucher" });
        }
    }
}