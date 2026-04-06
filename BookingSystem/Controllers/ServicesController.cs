using Microsoft.AspNetCore.Mvc;
using BookingSystem.Services;

namespace BookingSystem.Controllers;

[ApiController]
[Route("api/services")]
public class ServicesController : ControllerBase
{
    private readonly StoreServiceService _storeServiceService;

    public ServicesController(StoreServiceService storeServiceService)
    {
        _storeServiceService = storeServiceService;
    }

    // ====================== LẤY TẤT CẢ DỊCH VỤ CỦA STORE ======================
    [HttpGet("store/{storeId}")]
    public async Task<IActionResult> GetByStore(Guid storeId)
    {
        try
        {
            var result = await _storeServiceService.GetServicesByStoreAsync(storeId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Lỗi khi lấy danh sách dịch vụ",
                error = ex.Message
            });
        }
    }
}