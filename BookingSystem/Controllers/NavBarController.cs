using Microsoft.AspNetCore.Mvc;
using BookingSystem.Services;
using BookingSystem.DTOs;

namespace BookingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NavbarController : ControllerBase
{
    private readonly NavbarService _navbarService;

    public NavbarController(NavbarService navbarService)
    {
        _navbarService = navbarService;
    }

    // GET api/navbar/store-categories
    [HttpGet("store-categories")]
    public async Task<IActionResult> GetStoreCategories()
    {
        try
        {
            var categories = await _navbarService.GetStoreCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Lỗi khi lấy danh sách category",
                error = ex.Message
            });
        }
    }

    // DEBUG endpoint (có thể xoá sau)
    [HttpGet("test")]
    public IActionResult Test()
    {
        var result = _navbarService.Test();
        return Ok(result);
    }
}