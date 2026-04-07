using BookingSystem.DTOs;
using BookingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingSystem.Controllers.Admin;

[ApiController]
[Route("api/admin/stores")]
//[Authorize(Roles = "Admin")]
public class AdminStoreController : ControllerBase
{
    private readonly AdminStoreService _adminStoreService;

    public AdminStoreController(AdminStoreService adminStoreService)
    {
        _adminStoreService = adminStoreService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _adminStoreService.GetAllAsync(search, status, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var store = await _adminStoreService.GetByIdAsync(id);
        if (store == null)
            return NotFound(new { message = "Không tìm thấy cửa hàng" });

        return Ok(store);
    }

    [HttpGet("{storeId}/services")]
    public async Task<IActionResult> GetServices(Guid storeId)
    {
        var services = await _adminStoreService.GetServicesAsync(storeId);
        return Ok(services);
    }

    [HttpPut("{storeId}/services/{serviceId}/status")]
    public async Task<IActionResult> ToggleServiceStatus(Guid storeId, Guid serviceId, [FromBody] ToggleServiceStatusRequest request)
    {
        try
        {
            await _adminStoreService.ToggleServiceStatusAsync(storeId, serviceId, request.IsActive);
            return Ok(new { message = $"Dịch vụ đã được {(request.IsActive ? "mở" : "khóa")}" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Không tìm thấy dịch vụ" });
        }
    }

    [HttpGet("{storeId}/timeslots")]
    public async Task<IActionResult> GetTimeSlots(Guid storeId)
        => Ok(await _adminStoreService.GetTimeSlotsAsync(storeId));

    [HttpGet("{storeId}/timeslot-overrides")]
    public async Task<IActionResult> GetOverrides(Guid storeId)
        => Ok(await _adminStoreService.GetTimeSlotOverridesAsync(storeId));

    [HttpGet("{storeId}/vouchers")]
    public async Task<IActionResult> GetVouchers(Guid storeId)
        => Ok(await _adminStoreService.GetVouchersAsync(storeId));

    [HttpGet("{storeId}/bookings")]
    public async Task<IActionResult> GetBookings(Guid storeId)
    {
        var result = await _adminStoreService.GetBookingsAsync(storeId);
        return Ok(result);
    }

    [HttpGet("{storeId}/reviews")]
    public async Task<IActionResult> GetReviews(Guid storeId)
    {
        var result = await _adminStoreService.GetReviewsAsync(storeId);
        return Ok(result);
    }
}