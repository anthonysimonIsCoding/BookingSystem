using BookingSystem.DTOs;
using BookingSystem.Entities.Enums;
using BookingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingSystem.Controllers.Admin;

[ApiController]
[Route("api/admin/users")]
//[Authorize(Roles = "Admin")]
public class AdminUserController : ControllerBase
{
    private readonly AdminUserService _adminUserService;

    public AdminUserController(AdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] UserRole? role,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _adminUserService.GetAllAsync(search, role, isActive, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _adminUserService.GetByIdAsync(id);
        if (user == null)
            return NotFound(new { message = "Không tìm thấy người dùng" });

        return Ok(user);
    }

    [HttpPut("{id}/role")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateUserRoleRequest request)
    {
        try
        {
            await _adminUserService.UpdateRoleAsync(id, request.Role);
            return Ok(new { message = "Cập nhật vai trò thành công" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Không tìm thấy người dùng" });
        }
    }

    [HttpPut("{id}/toggle-active")]
    public async Task<IActionResult> ToggleActive(Guid id, [FromBody] bool isActive)
    {
        try
        {
            await _adminUserService.ToggleActiveAsync(id, isActive);
            return Ok(new { message = $"Người dùng đã được {(isActive ? "kích hoạt" : "vô hiệu hóa")}" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Không tìm thấy người dùng" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _adminUserService.DeleteAsync(id);
            return Ok(new { message = "Đã xóa người dùng" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Không tìm thấy người dùng" });
        }
    }
}