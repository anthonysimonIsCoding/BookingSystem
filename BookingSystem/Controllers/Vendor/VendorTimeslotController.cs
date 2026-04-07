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
[Route("api/vendor/timeslot")]
public class VendorTimeslotController : ControllerBase
{
    private readonly VendorTimeSlotService _vendorTimeSlotService;

    public VendorTimeslotController(VendorTimeSlotService vendorTimeSlotService)
    {
        _vendorTimeSlotService = vendorTimeSlotService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTimeSlots()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var slots = await _vendorTimeSlotService.GetTimeSlotsAsync(userId);
            return Ok(slots);
        }
        catch (UnauthorizedAccessException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> SaveTimeSlots([FromBody] List<TimeSlotRequest> requests)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            await _vendorTimeSlotService.SaveTimeSlotsAsync(userId, requests);
            return Ok(new { message = "Lưu timeslot thành công" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("overrides")]
    public async Task<IActionResult> GetOverrides([FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var data = await _vendorTimeSlotService.GetOverridesAsync(userId, fromDate, toDate);
            return Ok(data);
        }
        catch (UnauthorizedAccessException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("overrides")]
    public async Task<IActionResult> CreateOverride([FromBody] TimeSlotOverrideRequest req)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            await _vendorTimeSlotService.CreateOverrideAsync(userId, req);
            return Ok(new { message = "Tạo override thành công" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("overrides/{id}")]
    public async Task<IActionResult> UpdateOverride(Guid id, [FromBody] TimeSlotOverrideRequest req)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            await _vendorTimeSlotService.UpdateOverrideAsync(userId, id, req);
            return Ok(new { message = "Cập nhật override thành công" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return NotFound(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}