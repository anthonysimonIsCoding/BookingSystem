using BookingSystem.DTOs;
using BookingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookingSystem.Controllers.Vendor;

[Authorize(Roles = "ServiceProvider")]
[ApiController]
[Route("api/vendor")]
public class VendorDashboardController : ControllerBase
{
    private readonly VendorDashboardService _vendorDashboardService;

    public VendorDashboardController(VendorDashboardService vendorDashboardService)
    {
        _vendorDashboardService = vendorDashboardService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr))
            return Unauthorized(new { message = "Vui lòng đăng nhập" });

        var userId = Guid.Parse(userIdStr);

        try
        {
            var dashboard = await _vendorDashboardService.GetDashboardAsync(userId);
            return Ok(dashboard);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Lỗi khi tải dashboard",
                error = ex.Message
            });
        }
    }
}