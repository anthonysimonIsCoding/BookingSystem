using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BookingSystem.Entities.Enums;
using BookingSystem.Services;
using BookingSystem.DTOs;

namespace BookingSystem.Controllers;

[Authorize(Roles = "Customer")]
[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly BookingService _bookingService;

    public BookingsController(BookingService bookingService)
    {
        _bookingService = bookingService;
    }

    // ====================== TẠO ĐƠN ĐẶT LỊCH ======================
    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr == null) return Unauthorized();

        var userId = Guid.Parse(userIdStr);

        try
        {
            var booking = await _bookingService.CreateBookingAsync(request, userId);

            return Ok(new
            {
                message = "Đặt lịch thành công",
                bookingId = booking.Id,
                totalPrice = booking.TotalPrice
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
        }
    }

    // ====================== HỦY ĐƠN ======================
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelBooking(Guid id, [FromBody] CancelBookingRequest? request = null)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr == null) return Unauthorized();

        var userId = Guid.Parse(userIdStr);

        try
        {
            await _bookingService.CancelBookingAsync(id, userId, request?.Reason);
            return Ok(new { message = "Hủy đơn thành công" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ====================== LẤY LỊCH SỬ BOOKING CỦA PET ======================
    [HttpGet("pet/{petId}")]
    public async Task<IActionResult> GetPetHistory(Guid petId)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr == null) return Unauthorized();

        var userId = Guid.Parse(userIdStr);

        try
        {
            var history = await _bookingService.GetPetBookingHistoryAsync(petId, userId);
            return Ok(history);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
        }
    }

    // ====================== LẤY BOOKING THEO NGÀY ======================
    [HttpGet]
    public async Task<IActionResult> GetByDate(DateOnly? date)
    {
        try
        {
            var result = await _bookingService.GetBookingsByDateAsync(date);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
        }
    }

    // ====================== SERVER TIME ======================
    [HttpGet("server-time")]
    public IActionResult GetServerTime()
    {
        var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);

        return Ok(new
        {
            utcTime = DateTime.UtcNow,
            serverLocalTime = DateTime.Now,
            vietnamTime = vietnamTime,
            vietnamTimeString = vietnamTime.ToString("yyyy-MM-dd HH:mm:ss")
        });
    }
}