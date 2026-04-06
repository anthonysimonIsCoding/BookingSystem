using BookingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookingSystem.Controllers;

[Authorize]
[ApiController]
[Route("api/vouchers")]
public class VoucherController : ControllerBase
{
    private readonly VoucherService _voucherService;

    public VoucherController(VoucherService voucherService)
    {
        _voucherService = voucherService;
    }

    // ====================== VOUCHER SÀN ======================
    [HttpGet("platform/available")]
    public async Task<IActionResult> GetAvailablePlatformVouchers()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdStr))
            return Unauthorized(new { message = "Vui lòng đăng nhập" });

        var userId = Guid.Parse(userIdStr);

        try
        {
            var result = await _voucherService.GetAvailablePlatformVouchersAsync(userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Lỗi khi lấy voucher sàn",
                error = ex.Message
            });
        }
    }

    // ====================== VOUCHER CỦA STORE ======================
    [HttpGet("store/{storeId}/available")]
    public async Task<IActionResult> GetAvailableStoreVouchers(Guid storeId)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdStr))
            return Unauthorized(new { message = "Vui lòng đăng nhập" });

        var userId = Guid.Parse(userIdStr);

        try
        {
            var result = await _voucherService.GetAvailableStoreVouchersAsync(storeId, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Lỗi khi lấy voucher cửa hàng",
                error = ex.Message
            });
        }
    }
}