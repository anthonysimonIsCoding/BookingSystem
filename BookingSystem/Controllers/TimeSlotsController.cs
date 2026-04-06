using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BookingSystem.Services;

namespace BookingSystem.Controllers;

[Authorize(Roles = "Customer")]
[ApiController]
[Route("api/[controller]")]
public class TimeSlotsController : ControllerBase
{
    private readonly TimeSlotService _timeSlotService;

    public TimeSlotsController(TimeSlotService timeSlotService)
    {
        _timeSlotService = timeSlotService;
    }

    [HttpGet("store/{storeId}")]
    public async Task<IActionResult> GetByStore(Guid storeId, [FromQuery] DateOnly? date = null)
    {
        try
        {
            var result = await _timeSlotService.GetByStoreAsync(storeId, date);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Lỗi khi lấy khung giờ",
                error = ex.Message
            });
        }
    }
}