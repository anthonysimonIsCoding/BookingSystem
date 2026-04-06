using BookingSystem.Entities.Enums;
using BookingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using BookingSystem.DTOs;

namespace BookingSystem.Controllers.Vendor;

[Authorize(Roles = "ServiceProvider")]
[ApiController]
[Route("api/vendor/orders")]
public class VendorOrdersController : ControllerBase
{
    private readonly VendorOrderService _vendorOrderService;

    public VendorOrdersController(VendorOrderService vendorOrderService)
    {
        _vendorOrderService = vendorOrderService;
    }

    // ====================== LỊCH TUẦN ======================
    [HttpGet("calendar")]
    public async Task<IActionResult> GetCalendarOrders([FromQuery] DateOnly weekStart)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var result = await _vendorOrderService.GetCalendarOrdersAsync(userId, weekStart);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return BadRequest("Không tìm thấy cửa hàng");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
        }
    }

    // ====================== CHI TIẾT ĐƠN HÀNG ======================
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderDetail(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var dto = await _vendorOrderService.GetOrderDetailAsync(userId, id);
            return Ok(dto);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Không tìm thấy đơn hàng");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
        }
    }

    // ====================== CẬP NHẬT TRẠNG THÁI ======================
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            await _vendorOrderService.UpdateOrderStatusAsync(userId, id, request.Status);
            return Ok(new { message = "Cập nhật trạng thái thành công", newStatus = request.Status });
        }
        catch (Exception ex) when (ex is KeyNotFoundException || ex is InvalidOperationException)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ====================== CÁC TAB ======================
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingOrders()
        => await GetOrdersByStatus(new[] { BookingStatus.Pending, BookingStatus.Received, BookingStatus.Caring, BookingStatus.WaitingPickup });

    [HttpGet("completed")]
    public async Task<IActionResult> GetCompletedOrders()
        => await GetOrdersByStatus(new[] { BookingStatus.Completed });

    [HttpGet("cancelled")]
    public async Task<IActionResult> GetCancelledOrders()
        => await GetOrdersByStatus(new[] { BookingStatus.Cancelled });

    private async Task<IActionResult> GetOrdersByStatus(BookingStatus[] statuses)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var result = await _vendorOrderService.GetOrdersByStatusAsync(userId, statuses);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return BadRequest("Không tìm thấy cửa hàng");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
        }
    }
}